namespace LayoutFixer;

sealed record Key(uint Vk, uint Scan, bool Shift, string Typed, bool CapsLock = false);

/// Tracks what you type, spots wrong-layout words, and fixes them on Tab.
/// Same behaviour as the Mac version's Fixer.
sealed class Fixer : IDisposable
{
    const uint VK_BACK = 0x08, VK_TAB = 0x09, VK_RETURN = 0x0D, VK_ESCAPE = 0x1B, VK_SPACE = 0x20, VK_DELETE = 0x2E;
    static readonly HashSet<uint> ResetKeys = [VK_RETURN, VK_ESCAPE, VK_DELETE, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28];
    static readonly HashSet<uint> ModifierKeys = [0x10, 0x11, 0x12, 0x14, 0x5B, 0x5C, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5];

    readonly Dictionaries dicts;
    readonly ISuggestionPopup popup;
    readonly ISuggestionTimer hideTimer;
    readonly ICorrectionBoundary boundary;
    readonly Func<Layout?> currentLayout;
    readonly Func<Layout, Layout?> otherLayout;
    EditContext? context;
    bool disposed;
    public ReplacementResult LastReplacement { get; private set; } = ReplacementResult.Rejected;
    public bool LastSwitchSucceeded { get; private set; }
    public int BufferedKeys => word.Count + previous.Count + run.Count;
    bool enabled = true;
    public bool Enabled { get => enabled; set { enabled = value; if (!value) Reset(); } }
    public bool Faulted { get; private set; }
    int resetVersion;

    List<Key> word = [];
    /// The word finished just before `word`, if only a single space separates them.
    List<Key> previous = [];
    /// Keystrokes from the first wrong word up to now, while the popup is showing.
    List<Key> run = [];
    IntPtr runHkl, runTargetHkl;
    int runWords;

    public Fixer(Dictionaries dicts, ISuggestionPopup popup, ISuggestionTimer timer,
        ICorrectionBoundary boundary, Func<Layout?> currentLayout, Func<Layout, Layout?> otherLayout)
    {
        this.dicts = dicts;
        this.popup = popup;
        hideTimer = timer;
        this.boundary = boundary;
        this.currentLayout = currentLayout;
        this.otherLayout = otherLayout;
    }

    public void Reset()
    {
        resetVersion++;
        context = null;
        word.Clear();
        previous.Clear();
        run.Clear();
        runHkl = runTargetHkl = IntPtr.Zero;
        runWords = 0;
        hideTimer.Stop();
        popup.HideIt();
    }

    /// Returns true to swallow the key.
    public bool HandleKey(ObservedKey k)
    {
        try { return HandleVerifiedKey(k); }
        catch (UnsupportedMappingException) { Reset(); return false; }
    }

    bool HandleVerifiedKey(ObservedKey k)
    {
        if (disposed || Faulted || !Enabled || k.Injected) { Reset(); return false; }
        var captured = boundary.Capture();
        if (captured is not { IsKnown: true } || !boundary.MayObserve(captured)) { Reset(); return false; }
        if (context is not null && context != captured) { Reset(); return false; }
        context = captured;
        if (currentLayout()?.Hkl != captured.Hkl) { Reset(); return false; }
        if (word.Count + previous.Count + run.Count >= 512) { Reset(); return false; }
        uint vk = k.Vk;
        if (k.Shortcut) { Reset(); return false; }
        if (ModifierKeys.Contains(vk)) return false;

        if (vk == VK_TAB)
        {
            if (run.Count > 0 && popup.Visible && Apply()) return true;
            Reset();
            return false;
        }
        if (ResetKeys.Contains(vk)) { Reset(); return false; }
        if (vk == VK_BACK)
        {
            // Editors disagree about grapheme deletion. Rebuild from new input only.
            Reset();
            return false;
        }
        if (vk == VK_SPACE)
        {
            var space = new Key(vk, k.Scan, false, " ");
            if (run.Count > 0) run.Add(space);
            FinishWord(space);
            return false;
        }

        var current = currentLayout();
        bool shift = k.Shift;
        var typed = current?.Character(vk, k.Scan, shift, k.CapsLock);
        if (string.IsNullOrEmpty(typed) || typed.Any(char.IsControl)) { Reset(); return false; }
        if (current is null || current.Hkl != captured.Hkl) { Reset(); return false; }
        var key = new Key(vk, k.Scan, shift, typed, k.CapsLock);
        word.Add(key);
        if (run.Count > 0) { run.Add(key); Refresh(); }
        return false;
    }

    void FinishWord(Key space)
    {
        var finished = word.ToList();
        int version = resetVersion;
        try
        {
            if (word.Count == 0 || currentLayout() is not { } current) return;
            if (run.Count > 0)
            {
                // Already suggesting: keep extending, unless you switched layouts.
                runWords++;
                if (current.Hkl != runHkl || runWords > 12) Reset(); else Refresh();
                return;
            }
            if (otherLayout(current) is not { } other) return;
            if (LooksWrong(word, current, other))
            {
                // A short word just before (like هد -> "in") is usually part of the same mistake.
                var before = LooksWrong(previous, current, other, 1) ? [.. previous, space] : new List<Key>();
                run = [.. before, .. word, space];
                runHkl = current.Hkl;
                runTargetHkl = other.Hkl;
                runWords = 1;
                Refresh();
            }
        }
        finally
        {
            word.Clear();
            previous = version == resetVersion ? finished : [];
        }
    }

    bool LooksWrong(List<Key> keys, Layout current, Layout other, int minLetters = 3) =>
        keys.Count > 0 && dicts.IsMistake(string.Concat(keys.Select(k => k.Typed)), current.Lang, Convert(keys, other), minLetters);

    static string Convert(IEnumerable<Key> keys, Layout layout) =>
        string.Concat(keys.Select(k => k.Vk == VK_SPACE ? " " : layout.Character(k.Vk, k.Scan, k.Shift, k.CapsLock)
            ?? throw new UnsupportedMappingException()));

    Layout? Target()
    {
        var target = currentLayout() is { } current ? otherLayout(current) : null;
        return target is not null && (runTargetHkl == 0 || target.Hkl == runTargetHkl) ? target : null;
    }

    void Refresh()
    {
        if (Target() is not { } target) { Reset(); return; }
        popup.ShowSuggestion(Convert(run, target).Trim(), target.DisplayName);
        hideTimer.Stop();
        hideTimer.Restart();
    }

    bool Apply()
    {
        LastReplacement = ReplacementResult.Rejected;
        LastSwitchSucceeded = false;
        if (context is null || Target() is not { } target || !boundary.CanReplace ||
            !boundary.TryValidateSpan(context, string.Concat(run.Select(k => k.Typed)), out var ticket) ||
            ticket is null || ticket.Context != context || ticket.Start < 0 ||
            ticket.Length != string.Concat(run.Select(k => k.Typed)).Length ||
            boundary.Capture() != context || !boundary.MayObserve(context))
        {
            Reset();
            return false;
        }
        var text = Convert(run, target);
        var captured = context;
        Reset();
        // The adapter must atomically compare its revision and exact text span and replace it.
        // Backspace and clipboard fallbacks are forbidden. A partial failure is never retried.
        LastReplacement = boundary.Replace(ticket, text);
        if (LastReplacement == ReplacementResult.Partial) { Faulted = true; Enabled = false; }
        if (LastReplacement == ReplacementResult.Success && boundary.Capture() == captured)
            LastSwitchSucceeded = boundary.SwitchLayout(captured, target);
        return LastReplacement != ReplacementResult.Rejected;
    }

    public void Dispose()
    {
        if (disposed) return;
        Reset();
        disposed = true;
        hideTimer.Dispose();
    }
}

sealed class UnsupportedMappingException : Exception { }
