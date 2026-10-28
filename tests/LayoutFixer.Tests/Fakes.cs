namespace LayoutFixer;

sealed class Mapping : IKeyboardMapping
{
    public readonly Dictionary<(nint, uint, bool, bool), string?> Values = [];
    public uint LastVk;
    public bool LastShift, LastCaps;
    public uint Remapped;
    public uint VirtualKey(uint scan, nint hkl) => Remapped == 0 ? scan : Remapped;
    public string? Translate(uint vk, uint scan, bool shift, bool capsLock, nint hkl)
    {
        LastVk = vk; LastShift = shift; LastCaps = capsLock;
        return Values.GetValueOrDefault((hkl, vk, shift, capsLock));
    }
}

sealed class PopupFake : ISuggestionPopup
{
    public bool Visible { get; set; }
    public string Text = "";
    public void ShowSuggestion(string suggestion, string target) { Text = suggestion; Visible = true; }
    public void HideIt() { Text = ""; Visible = false; }
}

sealed class TimerFake : ISuggestionTimer
{
    public bool Running, Disposed;
    public void Restart() => Running = true;
    public void Stop() => Running = false;
    public void Dispose() { Disposed = true; Running = false; }
}

sealed class BoundaryFake : ICorrectionBoundary
{
    public EditContext? Context = new(1, 2, 3, 0x429, 1);
    public bool Eligible = true;
    public bool CanReplace { get; set; } = true;
    public bool ValidSpan = true, SwitchSucceeds = true;
    public int Replacements, Switches;
    public string Expected = "", Replacement = "";
    public ReplacementResult Outcome = ReplacementResult.Success;
    public Action? OnValidate, OnReplace;
    public int TicketLengthAdjustment;
    public long Revision = 1;
    public EditContext? Capture() => Context;
    public bool MayObserve(EditContext context) => Eligible && Context == context;
    public bool TryValidateSpan(EditContext context, string expected, out EditTicket? ticket)
    {
        Expected = expected;
        ticket = ValidSpan ? new EditTicket(context, 0, expected.Length + TicketLengthAdjustment, Revision) : null;
        OnValidate?.Invoke();
        return ValidSpan;
    }
    public ReplacementResult Replace(EditTicket ticket, string replacement)
    {
        Replacements++;
        Replacement = replacement;
        OnReplace?.Invoke();
        return Outcome;
    }
    public bool SwitchLayout(EditContext context, Layout target) { Switches++; return SwitchSucceeds; }
}

sealed class PlatformFake : ILayoutPlatform
{
    public nint[] Layouts = [];
    public bool Found = true;
    public uint Thread = 7, ReadThread;
    public nint Hkl = 0x429, Foreground = 1, Focus = 2;
    public IReadOnlyList<nint> Installed() => Layouts;
    public bool TryFocusedThread(out nint foreground, out nint focus, out uint thread)
    { foreground = Foreground; focus = Focus; thread = Thread; return Found; }
    public nint KeyboardLayout(uint thread) { ReadThread = thread; return Hkl; }
}

sealed class Fixture : IDisposable
{
    public readonly PopupFake Popup = new();
    public readonly TimerFake Timer = new();
    public readonly BoundaryFake Boundary = new();
    public readonly Mapping Mapping = new();
    public Layout From, To;
    public readonly Fixer Engine;

    public Fixture(bool toPersian = false)
    {
        const string en = "abcdefghijklmnopqrstuvwxyz";
        const string fa = "شذزیثبلاهتنمئدخحضقسفعرصطغظ";
        for (int i = 0; i < en.Length; i++)
        {
            uint vk = (uint)en[i];
            Mapping.Values[(0x409, vk, false, false)] = en[i].ToString();
            Mapping.Values[(0x429, vk, false, false)] = fa[i].ToString();
        }
        From = new(toPersian ? 0x409 : 0x429, toPersian ? "en" : "fa", Mapping);
        To = new(toPersian ? 0x429 : 0x409, toPersian ? "fa" : "en", Mapping);
        Boundary.Context = Boundary.Context! with { Hkl = From.Hkl };
        Engine = new(new Dictionaries(Path.Combine(AppContext.BaseDirectory, "data")), Popup, Timer, Boundary, () => From, _ => To);
    }

    public void Type(string physical)
    {
        foreach (char key in physical) Engine.HandleKey(new((uint)key, (uint)key));
    }
    public void Suggest() { Type("want"); Space(); Check.True(Popup.Visible); }
    public bool Space() => Engine.HandleKey(new(0x20, 0x20));
    public bool Tab() => Engine.HandleKey(new(9, 9));
    public void Dispose() => Engine.Dispose();
}
