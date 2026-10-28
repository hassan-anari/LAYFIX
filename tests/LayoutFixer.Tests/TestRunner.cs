using System.Diagnostics;
using System.Xml.Linq;

namespace LayoutFixer;

// Dependency-free regression executable: every case invokes production source.
// Nonzero exit on any failure; JUnit XML is produced even when a case fails.
static class Check
{
    public static void True(bool value) { if (!value) throw new InvalidOperationException("Expected true"); }
    public static void False(bool value) => True(!value);
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException("Values differ"); // No input/content in diagnostics.
    }
    public static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected exception absent");
    }
}

sealed class TestRunner
{
    readonly List<(string Name, Action Test)> cases = [];
    public void Add(string name, Action test) => cases.Add((name, test));
    public int Run(string output)
    {
        int failures = 0;
        var suite = new XElement("testsuite", new XAttribute("tests", cases.Count));
        foreach (var (name, test) in cases)
        {
            var clock = Stopwatch.StartNew();
            var entry = new XElement("testcase", new XAttribute("name", name), new XAttribute("classname", "LayoutFixer"));
            try { test(); Console.WriteLine("PASS " + name); }
            catch (Exception error)
            {
                failures++;
                entry.Add(new XElement("failure", new XAttribute("type", error.GetType().Name), "Assertion failed; content omitted"));
                Console.WriteLine("FAIL " + name + " (" + error.GetType().Name + ")");
            }
            entry.Add(new XAttribute("time", clock.Elapsed.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            suite.Add(entry);
        }
        suite.Add(new XAttribute("failures", failures), new XAttribute("skipped", 0));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        new XDocument(new XElement("testsuites", suite)).Save(output);
        Console.WriteLine($"Tests: {cases.Count}; passed: {cases.Count - failures}; failed: {failures}; skipped: 0");
        return failures == 0 && cases.Count > 0 ? 0 : 1;
    }
}
