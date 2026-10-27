using Layfix.Core;

namespace Layfix.Core.Tests;

public sealed class LanguageTests
{
    [Theory]
    [InlineData(0, InputLanguage.Unknown)]
    [InlineData(0x04090409, InputLanguage.English)]
    [InlineData(0x08090809, InputLanguage.English)]
    [InlineData(0x04290429, InputLanguage.Persian)]
    [InlineData(0xF0010429, InputLanguage.Persian)]
    [InlineData(0x04070407, InputLanguage.Unsupported)]
    public void ClassifiesLanguageRatherThanVariant(long handle, InputLanguage expected) => Assert.Equal(expected, KeyboardLayout.Classify(handle));

    [Fact]
    public void SelectsConfiguredVariantThenDeterministicFallback()
    {
        var layouts = new[] { new KeyboardLayout(2, "00050429", "Standard", InputLanguage.Persian), new KeyboardLayout(1, "00000429", "Persian", InputLanguage.Persian) };
        Assert.Equal(2, LayoutSelection.Select(layouts, InputLanguage.Persian, "00050429")?.Handle);
        Assert.Null(LayoutSelection.Select(layouts, InputLanguage.Persian, "missing"));
        Assert.Equal(1, LayoutSelection.Select(layouts, InputLanguage.Persian, null)?.Handle);
        Assert.Null(LayoutSelection.Select(layouts, InputLanguage.English, null));
    }
    [Theory]
    [InlineData(ObservationStatus.Unknown, "?")]
    [InlineData(ObservationStatus.Unsupported, "—")]
    [InlineData(ObservationStatus.Unavailable, "×")]
    [InlineData(ObservationStatus.Paused, "Ⅱ")]
    public void DoesNotFabricateLanguage(ObservationStatus status, string label) => Assert.Equal(label, new LanguageSnapshot(status, null, null).Indicator);

    internal sealed class Platform : ILanguagePlatform
    {
        internal static readonly TargetContext Target = new(10, 11, 12, 13, 1);
        internal static readonly KeyboardLayout English = new(0x04090409, "00000409", "English", InputLanguage.English);
        internal static readonly KeyboardLayout Persian = new(0x04290429, "00000429", "Persian", InputLanguage.Persian);
        internal LanguageSnapshot Value = new(ObservationStatus.Available, Target, Persian);
        internal IReadOnlyList<KeyboardLayout> Layouts = [English, Persian];
        internal SwitchOutcome RequestOutcome = SwitchOutcome.Pending;
        internal int Requests;
        internal Action? OnRequest;
        internal int Reads;
        public LanguageSnapshot Read() { Reads++; return Value; }
        public IReadOnlyList<KeyboardLayout> DiscoverLayouts() => Layouts;
        public SwitchOutcome RequestSwitch(TargetContext target, long source, KeyboardLayout layout) { Requests++; OnRequest?.Invoke(); return RequestOutcome; }
    }
    [Fact]
    public async Task ExternalChangeUpdatesObservableStateWithoutRequest()
    {
        var platform = new Platform();
        await using var service = new LanguageStateService(platform);
        var changes = new List<LanguageSnapshot>();
        service.Changed += changes.Add;
        await service.RefreshAsync();
        platform.Value = platform.Value with { Layout = Platform.English };
        await service.RefreshAsync();
        await service.RefreshAsync();
        Assert.Equal(new[] { "FA", "EN" }, changes.Select(x => x.Indicator));
        Assert.Equal(0, platform.Requests);
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BothDirectionsRequireIndependentReadback(bool fromPersian)
    {
        var platform = new Platform();
        if (!fromPersian) platform.Value = platform.Value with { Layout = Platform.English };
        int waits = 0;
        await using var service = new LanguageStateService(platform, (_, _) =>
        {
            waits++;
            if (waits == 3) platform.Value = platform.Value with { Layout = fromPersian ? Platform.English : Platform.Persian };
            return Task.CompletedTask;
        });
        await service.RefreshAsync();
        string original = service.Current.Indicator;
        platform.OnRequest = () => Assert.Equal(original, service.Current.Indicator);
        Assert.Equal(SwitchOutcome.Succeeded, await service.SwitchAsync(service.Current, null, null));
        Assert.Equal(3, waits);
        Assert.Equal(1, platform.Requests);
        Assert.Equal(fromPersian ? "EN" : "FA", service.Current.Indicator);
    }
    [Theory]
    [InlineData(SwitchOutcome.Blocked)]
    [InlineData(SwitchOutcome.TimedOut)]
    [InlineData(SwitchOutcome.Unsupported)]
    public async Task RequestFailureNeverBecomesSuccess(SwitchOutcome outcome)
    {
        var platform = new Platform { RequestOutcome = outcome };
        await using var service = new LanguageStateService(platform, (_, _) => throw new InvalidOperationException("Must not verify failed send."));
        await service.RefreshAsync();
        Assert.Equal(outcome, await service.SwitchAsync(service.Current, null, null));
        Assert.Equal("FA", service.Current.Indicator);
    }
    [Fact]
    public async Task UnchangedReadbackTimesOutWithoutRetries()
    {
        var platform = new Platform();
        int waits = 0;
        await using var service = new LanguageStateService(platform, (_, _) => { waits++; return Task.CompletedTask; });
        await service.RefreshAsync();
        Assert.Equal(SwitchOutcome.TimedOut, await service.SwitchAsync(service.Current, null, null));
        Assert.Equal(8, waits);
        Assert.Equal(1, platform.Requests);
        Assert.Equal("FA", service.Current.Indicator);
    }
    [Fact]
    public async Task MissingLanguageDoesNotSend()
    {
        var platform = new Platform { Layouts = [Platform.Persian] };
        await using var service = new LanguageStateService(platform);
        await service.RefreshAsync();
        Assert.Equal(SwitchOutcome.Unavailable, await service.SwitchAsync(service.Current, null, null));
        Assert.Equal(0, platform.Requests);
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ChangedFocusOrGenerationRejectsCapturedContext(bool focus)
    {
        var platform = new Platform();
        await using var service = new LanguageStateService(platform);
        await service.RefreshAsync();
        var captured = service.Current;
        platform.Value = platform.Value with { Target = focus ? Platform.Target with { Focus = 99 } : Platform.Target with { Generation = 2 } };
        Assert.Equal(SwitchOutcome.StaleContext, await service.SwitchAsync(captured, null, null));
        Assert.Equal(0, platform.Requests);
    }
    [Fact]
    public async Task FocusChangeDuringVerificationAborts()
    {
        var platform = new Platform();
        await using var service = new LanguageStateService(platform, (_, _) =>
        {
            platform.Value = platform.Value with { Target = Platform.Target with { Foreground = 20 }, Layout = Platform.English };
            return Task.CompletedTask;
        });
        await service.RefreshAsync();
        Assert.Equal(SwitchOutcome.StaleContext, await service.SwitchAsync(service.Current, null, null));
        Assert.Equal(1, platform.Requests);
    }
    [Fact]
    public async Task ConcurrentClickIsBusyAndPauseCancelsReadback()
    {
        var platform = new Platform();
        var waiting = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var service = new LanguageStateService(platform, async (_, ct) => { waiting.SetResult(); await resume.Task.WaitAsync(ct); });
        await service.RefreshAsync();
        var first = service.SwitchAsync(service.Current, null, null);
        await waiting.Task;
        Assert.Equal(SwitchOutcome.Busy, await service.SwitchAsync(service.Current, null, null));
        service.SetPaused(true);
        resume.SetResult();
        Assert.Equal(SwitchOutcome.Cancelled, await first);
        Assert.Equal(ObservationStatus.Paused, service.Current.Status);
        int reads = platform.Reads;
        await service.RefreshAsync();
        Assert.Equal(reads, platform.Reads);
        Assert.Equal(1, platform.Requests);
    }
    [Fact]
    public async Task DisposalCancelsPendingVerificationAndUnsubscribes()
    {
        var platform = new Platform();
        var waiting = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new LanguageStateService(platform, async (_, ct) => { waiting.SetResult(); await Task.Delay(Timeout.Infinite, ct); });
        await service.RefreshAsync();
        var operation = service.SwitchAsync(service.Current, null, null);
        await waiting.Task;
        await service.DisposeAsync();
        Assert.Equal(SwitchOutcome.Cancelled, await operation);
        await service.RefreshAsync();
        Assert.Equal(SwitchOutcome.Cancelled, await service.SwitchAsync(service.Current, null, null));
    }
}
