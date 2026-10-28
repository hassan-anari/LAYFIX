namespace LayoutFixer;

static class Tests
{
    static int Main(string[] args)
    {
        var tests = new TestRunner();
        var dictionaries = new Dictionaries(Path.Combine(AppContext.BaseDirectory, "data"));
        tests.Add("normalize Arabic/Persian letters and punctuation", () => Check.Equal("یکی", Dictionaries.Normalize(" !يكي؟ ")));
        tests.Add("normalize Unicode canonical composition", () => Check.Equal("é", Dictionaries.Normalize("e\u0301")));
        tests.Add("retain ZWNJ inside Persian words", () => Check.Equal("می‌رود", Dictionaries.Normalize("می‌رود")));
        tests.Add("empty normalization", () => Check.Equal("", Dictionaries.Normalize(" ! ")));
        tests.Add("missing dictionaries are a failure", () => Check.Throws<FileNotFoundException>(() => new Dictionaries(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()))));
        tests.Add("FA to EN frequency detection", () => Check.True(dictionaries.IsMistake("صشدف", "fa", "want")));
        tests.Add("EN to FA frequency detection", () => Check.True(dictionaries.IsMistake("sghl", "en", "سلام")));
        tests.Add("real English words remain untouched", () => Check.False(dictionaries.IsMistake("hello", "en", "سلام")));
        tests.Add("Webster protects uncommon English words", () => Check.False(dictionaries.IsMistake("aardvark", "en", "سلام")));
        tests.Add("short ambiguous word not suggested alone", () => Check.False(dictionaries.IsMistake("هد", "fa", "in")));
        tests.Add("short word eligible only before wrong word", () => Check.True(dictionaries.IsMistake("هد", "fa", "in", 1)));
        tests.Add("unknown source language rejected", () => Check.False(dictionaries.IsMistake("sghl", "xx", "سلام")));
        tests.Add("unknown alternate word rejected", () => Check.False(dictionaries.IsMistake("صشدف", "fa", "zzzzzzq")));
        tests.Add("URL false positive rejected", () => Check.False(dictionaries.IsMistake("https://example.com", "en", "سلام")));
        tests.Add("email false positive rejected", () => Check.False(dictionaries.IsMistake("me@example.com", "en", "سلام")));
        tests.Add("contraction remains English", () => Check.False(dictionaries.IsMistake("don't", "en", "سلام")));
        tests.Add("non-ASCII alternate English rejected", () => Check.False(dictionaries.IsMistake("صشدف", "fa", "wánt")));

        var safe = new ContextFacts(false, false, true, true, true, true, false, true);
        tests.Add("complete independent safe verdict", () => Check.True(SafetyPolicy.MayObserve(safe)));
        tests.Add("password exclusion", () => Check.False(SafetyPolicy.MayObserve(safe with { Password = true })));
        tests.Add("sensitive input exclusion", () => Check.False(SafetyPolicy.MayObserve(safe with { Sensitive = true })));
        tests.Add("unknown context exclusion", () => Check.False(SafetyPolicy.MayObserve(safe with { Certain = false })));
        tests.Add("unsupported control exclusion", () => Check.False(SafetyPolicy.MayObserve(safe with { Supported = false })));
        tests.Add("read-only control exclusion", () => Check.False(SafetyPolicy.MayObserve(safe with { Editable = false })));
        tests.Add("secure desktop exclusion", () => Check.False(SafetyPolicy.MayObserve(safe with { DefaultDesktop = false })));
        tests.Add("integrity boundary exclusion", () => Check.False(SafetyPolicy.MayObserve(safe with { SameIntegrity = false })));
        tests.Add("remote session exclusion", () => Check.False(SafetyPolicy.MayObserve(safe with { Remote = true })));
        tests.Add("shipping observation remains blocked", () => Check.False(RuntimeSafety.GlobalObservationApproved));

        tests.Add("physical scan resolved in destination", () =>
        {
            var m = new Mapping { Remapped = 123 };
            m.Values[(0x409, 123, false, false)] = "x";
            Check.Equal("x", new Layout(0x409, "en", m).Character(44, 55, false));
            Check.Equal(123u, m.LastVk);
        });
        foreach (var (shift, caps, output) in new[] { (false, false, "a"), (true, false, "A"), (false, true, "A"), (true, true, "a") })
            tests.Add($"Shift={shift} Caps={caps} reaches Windows boundary", () =>
            {
                var m = new Mapping(); m.Values[(0x409, 65, shift, caps)] = output;
                Check.Equal(output, new Layout(0x409, "en", m).Character(65, 65, shift, caps));
                Check.Equal(shift, m.LastShift); Check.Equal(caps, m.LastCaps);
            });
        foreach (var value in new[] { "!", "؟", "e\u0301", "😀" })
            tests.Add("mapping preserves punctuation/Unicode " + value.Length + " " + (int)value[0], () =>
            { var m = new Mapping(); m.Values[(0x429, 1, false, false)] = value; Check.Equal(value, new Layout(0x429, "fa", m).Character(1, 1, false)); });
        foreach (var value in new string?[] { null, "", "\n", "\ud800", "\udc00" })
            tests.Add("unsupported or malformed mapping " + (value is null ? "null" : string.Join("-", value.Select(c => (int)c))), () =>
            { var m = new Mapping(); m.Values[(0x429, 1, false, false)] = value; Check.Equal<string?>(null, new Layout(0x429, "fa", m).Character(1, 1, false)); });
        tests.Add("both Persian variants retained", () => Check.Equal(3, Layouts.All(new PlatformFake { Layouts = [0x409, 0x429, 0x50429] }).Count));
        tests.Add("ambiguous target variants rejected", () => Check.Equal<Layout?>(null, Layouts.Other(new(0x409, "en"), new PlatformFake { Layouts = [0x409, 0x429, 0x50429] })));
        tests.Add("multiple English targets rejected", () => Check.Equal<Layout?>(null, Layouts.Other(new(0x429, "fa"), new PlatformFake { Layouts = [0x409, 0x809, 0x429] })));
        tests.Add("focused thread identity used", () => { var p = new PlatformFake(); Check.Equal((nint)0x429, Layouts.Current(p)!.Hkl); Check.Equal(7u, p.ReadThread); });
        tests.Add("unknown focus never falls back to calling thread", () => Check.Equal<Layout?>(null, Layouts.Current(new PlatformFake { Found = false })));
        tests.Add("zero focused thread rejected", () => Check.Equal<Layout?>(null, Layouts.Current(new PlatformFake { Thread = 0 })));
        tests.Add("unsupported active layout rejected", () => Check.Equal<Layout?>(null, Layouts.Current(new PlatformFake { Hkl = 0x407 })));

        tests.Add("upstream FA to EN suggestion and explicit acceptance", () =>
        { using var f = new Fixture(); f.Suggest(); Check.Equal("want", f.Popup.Text); Check.True(f.Tab()); Check.Equal("want ", f.Boundary.Replacement); Check.Equal(1, f.Boundary.Switches); Check.True(f.Engine.LastSwitchSucceeded); });
        tests.Add("upstream EN to FA suggestion and explicit acceptance", () =>
        { using var f = new Fixture(true); f.Type("sghl"); f.Space(); Check.Equal("سلام", f.Popup.Text); Check.True(f.Tab()); Check.Equal("سلام ", f.Boundary.Replacement); });
        tests.Add("consecutive words retain complete physical run", () =>
        { using var f = new Fixture(); f.Suggest(); f.Type("hello"); f.Space(); Check.True(f.Tab()); Check.Equal("want hello ", f.Boundary.Replacement); });
        tests.Add("short predecessor included conservatively", () =>
        { using var f = new Fixture(); f.Type("in"); f.Space(); Check.False(f.Popup.Visible); f.Suggest(); Check.Equal("in want", f.Popup.Text); });
        tests.Add("normal Tab preserved without suggestion", () => { using var f = new Fixture(); Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("Esc dismisses and remains normal application input", () => { using var f = new Fixture(); f.Suggest(); Check.False(f.Engine.HandleKey(new(27, 27))); Check.False(f.Popup.Visible); Check.Equal(0, f.Engine.BufferedKeys); });
        tests.Add("hidden suggestion cannot intercept Tab", () => { using var f = new Fixture(); f.Suggest(); f.Popup.Visible = false; Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("read-only preview does not intercept Tab", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.CanReplace = false; Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("focus change cancels without consuming Tab", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.Context = f.Boundary.Context! with { Focus = 99 }; Check.False(f.Tab()); Check.Equal(0, f.Engine.BufferedKeys); });
        tests.Add("layout change cancels without consuming Tab", () => { using var f = new Fixture(); f.Suggest(); f.From = new(0x409, "en", f.Mapping); Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("opposite layout change cancels", () => { using var f = new Fixture(); f.Suggest(); f.To = new(0x809, "en", f.Mapping); Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("sensitive verdict clears existing buffer", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.Eligible = false; Check.False(f.Tab()); Check.Equal(0, f.Engine.BufferedKeys); });
        tests.Add("unknown context buffers no keys", () => { using var f = new Fixture(); f.Boundary.Context = null; f.Type("want"); Check.Equal(0, f.Engine.BufferedKeys); });
        tests.Add("stale exact span rejects replacement", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.ValidSpan = false; Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("focus race after span validation rejects edit", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.OnValidate = () => f.Boundary.Context = null; Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("generation change cancels asynchronous stale work", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.Context = f.Boundary.Context! with { Generation = 2 }; Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("partial replacement latches fault without retry or switching", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.Outcome = ReplacementResult.Partial; Check.True(f.Tab()); Check.True(f.Engine.Faulted); Check.False(f.Tab()); Check.Equal(1, f.Boundary.Replacements); Check.Equal(0, f.Boundary.Switches); });
        tests.Add("failed replacement never switches layout", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.Outcome = ReplacementResult.Failed; Check.True(f.Tab()); Check.Equal(0, f.Boundary.Switches); Check.False(f.Engine.LastSwitchSucceeded); });
        tests.Add("adapter rejection preserves Tab", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.Outcome = ReplacementResult.Rejected; Check.False(f.Tab()); Check.Equal(0, f.Boundary.Switches); });
        tests.Add("focus race after replacement never switches another app", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.OnReplace = () => f.Boundary.Context = null; Check.True(f.Tab()); Check.Equal(0, f.Boundary.Switches); });
        tests.Add("failed switch is reported accurately", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.SwitchSucceeds = false; Check.True(f.Tab()); Check.False(f.Engine.LastSwitchSucceeded); });
        tests.Add("Backspace cancels instead of guessing grapheme length", () => { using var f = new Fixture(); f.Suggest(); Check.False(f.Engine.HandleKey(new(8, 8))); Check.Equal(0, f.Engine.BufferedKeys); });
        tests.Add("injected events are never buffered", () => { using var f = new Fixture(); f.Engine.HandleKey(new(65, 65, Injected: true)); Check.Equal(0, f.Engine.BufferedKeys); });
        tests.Add("shortcut cancels pending suggestion", () => { using var f = new Fixture(); f.Suggest(); f.Engine.HandleKey(new(17, 17, Shortcut: true)); Check.Equal(0, f.Engine.BufferedKeys); });
        tests.Add("timeout cancellation clears popup and text", () => { using var f = new Fixture(); f.Suggest(); f.Engine.Reset(); Check.False(f.Popup.Visible); Check.Equal(0, f.Engine.BufferedKeys); Check.False(f.Timer.Running); });
        tests.Add("pause immediately clears volatile text", () => { using var f = new Fixture(); f.Suggest(); f.Engine.Enabled = false; Check.Equal(0, f.Engine.BufferedKeys); Check.False(f.Tab()); });
        tests.Add("buffer growth bounded", () => { using var f = new Fixture(); f.Type(new string('a', 2000)); Check.True(f.Engine.BufferedKeys <= 512); });
        tests.Add("disposal stops timer and clears text idempotently", () => { var f = new Fixture(); f.Suggest(); f.Dispose(); f.Dispose(); Check.True(f.Timer.Disposed); Check.Equal(0, f.Engine.BufferedKeys); Check.False(f.Tab()); });
        tests.Add("hook lifecycle releases in reverse order exactly once", () => { var released = new List<nint>(); var hooks = new HookLifetime(); hooks.Register(() => 1, released.Add); hooks.Register(() => 2, released.Add); hooks.Dispose(); hooks.Dispose(); Check.True(released.SequenceEqual(new nint[] { 2, 1 })); });
        tests.Add("failed registration unwinds existing hooks", () => { int released = 0; using var hooks = new HookLifetime(); hooks.Register(() => 1, _ => released++); Check.Throws<InvalidOperationException>(() => hooks.Register(() => 0, _ => released++)); Check.Equal(1, released); });
        tests.Add("invalid revision rejects replacement", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.Revision = 0; Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("mismatched UTF-16 span rejects replacement", () => { using var f = new Fixture(); f.Suggest(); f.Boundary.TicketLengthAdjustment = 1; Check.False(f.Tab()); Check.Equal(0, f.Boundary.Replacements); });
        tests.Add("one failed hook cleanup cannot leak other registered handles", () => { int released = 0; var hooks = new HookLifetime(); hooks.Register(() => 1, _ => released++); hooks.Register(() => 2, _ => throw new InvalidOperationException()); Check.Throws<AggregateException>(hooks.Dispose); Check.Equal(1, released); hooks.Dispose(); });
        tests.Add("thirteenth word cancels entire run", () => { using var f = new Fixture(); f.Suggest(); for (int i = 0; i < 12; i++) { f.Type("hello"); f.Space(); } Check.False(f.Popup.Visible); Check.Equal(0, f.Engine.BufferedKeys); Check.False(f.Tab()); });
        tests.Add("unmappable destination cancels without source-character fallback", () => { using var f = new Fixture(); f.Mapping.Values[(0x409, (uint)'w', false, false)] = null; f.Type("want"); f.Space(); Check.False(f.Popup.Visible); Check.Equal(0, f.Engine.BufferedKeys); });
        return tests.Run(args.FirstOrDefault() ?? "TestResults/core.xml");
    }
}
