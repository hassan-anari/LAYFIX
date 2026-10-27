using Layfix.Core;

namespace Layfix.Windows.Tests;

public sealed class AdapterTests
{
    private sealed class Api : IWindowsInputApi
    {
        public long Generation { get; set; } = 1;
        public bool Available { get; set; } = true;
        internal NativeTarget? Target = new(10, 11, 12, 13, 14);
        internal bool Allowed = true;
        internal bool Ime;
        internal readonly Dictionary<uint, long> Layouts = new() { [12] = 0x04290429, [13] = 0x04290429 };
        internal Action? DuringLayoutRead;
        internal readonly List<uint> Lookups = [];
        internal SwitchOutcome SendResult = SwitchOutcome.Pending;
        internal int Sends;
        internal NativeTarget? SentTarget;
        public NativeTarget? ResolveTarget() => Target;
        public bool IsTargetAllowed(NativeTarget target) => Allowed;
        public long GetLayout(uint thread) { Lookups.Add(thread); DuringLayoutRead?.Invoke(); return Layouts.GetValueOrDefault(thread); }
        public bool IsInputMethod(long layout) => Ime;
        public IReadOnlyList<KeyboardLayout> GetLayouts() => [new(0x04090409, "00000409", "English", InputLanguage.English), new(0x04290429, "00000429", "Persian", InputLanguage.Persian)];
        public SwitchOutcome SendLayoutRequest(NativeTarget target, long layout) { Sends++; SentTarget = target; return SendResult; }
    }
    [Fact]
    public void ResolvesFocusedThreadAndCrossChecksForegroundThread()
    {
        var api = new Api();
        var actual = new WindowsLanguagePlatform(api).Read();
        Assert.Equal(ObservationStatus.Available, actual.Status);
        Assert.Equal((uint)13, actual.Target?.Thread);
        Assert.Equal(11, actual.Target?.Focus);
        Assert.Equal(new uint[] { 13, 12 }, api.Lookups);
        Assert.DoesNotContain((uint)0, api.Lookups);
    }
    [Fact]
    public void ConflictingThreadsAreUnknown()
    {
        var api = new Api(); api.Layouts[12] = 0x04090409;
        var result = new WindowsLanguagePlatform(api).Read();
        Assert.Equal(ObservationStatus.Unknown, result.Status);
        Assert.Null(result.Layout);
    }
    [Fact]
    public void LayoutReadRacesInvalidateSnapshot()
    {
        var api = new Api(); api.DuringLayoutRead = () => api.Generation++;
        Assert.Equal(ObservationStatus.Unknown, new WindowsLanguagePlatform(api).Read().Status);
    }
    [Fact]
    public void ForegroundRaceInvalidatesSnapshot()
    {
        var api = new Api(); api.DuringLayoutRead = () => api.Target = api.Target!.Value with { Foreground = 99 };
        Assert.Equal(ObservationStatus.Unknown, new WindowsLanguagePlatform(api).Read().Status);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BlockedAndImeContextsAreUnsupported(bool ime)
    {
        var api = new Api { Allowed = ime, Ime = ime };
        Assert.Equal(ObservationStatus.Unsupported, new WindowsLanguagePlatform(api).Read().Status);
        Assert.DoesNotContain((uint)0, api.Lookups);
    }
    [Theory]
    [InlineData(0, ObservationStatus.Unknown)]
    [InlineData(0x04070407, ObservationStatus.Unsupported)]
    public void InvalidOrOtherLanguageIsExplicit(long handle, ObservationStatus status)
    {
        var api = new Api(); api.Layouts[12] = api.Layouts[13] = handle;
        Assert.Equal(status, new WindowsLanguagePlatform(api).Read().Status);
    }
    [Fact]
    public void NoDesktopOrNoFocusIsUnavailable()
    {
        var api = new Api { Available = false };
        Assert.Equal(LanguageSnapshot.Unavailable, new WindowsLanguagePlatform(api).Read());
        api.Available = true; api.Target = null;
        Assert.Equal(LanguageSnapshot.Unavailable, new WindowsLanguagePlatform(api).Read());
        Assert.Empty(api.Lookups);
    }
    [Theory]
    [InlineData(SwitchOutcome.Pending)]
    [InlineData(SwitchOutcome.Blocked)]
    [InlineData(SwitchOutcome.TimedOut)]
    public void SendsToCapturedFocusAndDoesNotInventSuccess(SwitchOutcome nativeResult)
    {
        var api = new Api { SendResult = nativeResult };
        var adapter = new WindowsLanguagePlatform(api);
        var snapshot = adapter.Read();
        Assert.Equal(nativeResult, adapter.RequestSwitch(snapshot.Target!, snapshot.Layout!.Handle, api.GetLayouts()[0]));
        Assert.Equal(api.Target, api.SentTarget);
        Assert.Equal("FA", adapter.Read().Indicator);
        Assert.Equal(1, api.Sends);
    }
    [Fact]
    public void AwayAndBackGenerationRejectsSwitch()
    {
        var api = new Api(); var adapter = new WindowsLanguagePlatform(api); var captured = adapter.Read();
        api.Generation += 2;
        Assert.Equal(SwitchOutcome.StaleContext, adapter.RequestSwitch(captured.Target!, captured.Layout!.Handle, api.GetLayouts()[0]));
        Assert.Equal(0, api.Sends);
    }
    [Fact]
    public void MissingTargetLayoutNeverSends()
    {
        var api = new Api(); var adapter = new WindowsLanguagePlatform(api); var captured = adapter.Read();
        Assert.Equal(SwitchOutcome.Unavailable, adapter.RequestSwitch(captured.Target!, captured.Layout!.Handle, new(99, "99", "Missing", InputLanguage.English)));
        Assert.Equal(0, api.Sends);
    }
    [Fact]
    public void ChangedSourceLayoutRejectsStaleRequest()
    {
        var api = new Api(); var adapter = new WindowsLanguagePlatform(api); var captured = adapter.Read();
        api.Layouts[12] = api.Layouts[13] = 0x04090409;
        Assert.Equal(SwitchOutcome.StaleContext, adapter.RequestSwitch(captured.Target!, captured.Layout!.Handle, api.GetLayouts()[0]));
        Assert.Equal(0, api.Sends);
    }
    [Fact]
    public void RegistrationCleanupIsReverseOrderAndIdempotent()
    {
        var released = new List<int>();
        var registrations = new NativeRegistrationSet();
        registrations.Add(() => released.Add(1)); registrations.Add(() => released.Add(2));
        registrations.Dispose(); registrations.Dispose();
        Assert.Equal(new[] { 2, 1 }, released);
        Assert.Throws<ObjectDisposedException>(() => registrations.Add(() => { }));
    }
    [Fact]
    public async Task ObservationWorkerStopsAndUnsubscribesWithoutFurtherReads()
    {
        var api = new Api();
        await using var service = new LanguageStateService(new WindowsLanguagePlatform(api));
        var observed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        service.Changed += _ => observed.TrySetResult();
        var worker = new LanguageObservationWorker(service);
        worker.Start();
        await observed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await worker.DisposeAsync();
        int reads = api.Lookups.Count;
        worker.Signal();
        Assert.Equal(reads, api.Lookups.Count);
    }
}
