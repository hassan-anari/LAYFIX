using System.Runtime.Versioning;

namespace Layfix.Windows.Tests;

public sealed class NativeSmokeTests
{
    [Fact]
    [Trait("Category", "NativeSmoke")]
    [SupportedOSPlatform("windows")]
    public async Task RealWin32RegistrationsLayoutsAndTeardown()
    {
        // This read-only smoke test runs on Windows CI. It does not type, switch, or prove editor compatibility.
        Assert.True(OperatingSystem.IsWindows());
        var notifications = new WindowsNotifications();
        try
        {
            await notifications.Ready.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(3, notifications.ActiveHookCount);
            Assert.True(notifications.SessionNotificationsAvailable);
            var api = new Win32InputApi(notifications);
            var layouts = api.GetLayouts();
            Assert.NotEmpty(layouts);
            Assert.All(layouts, layout => Assert.NotEqual(0, layout.Handle));
            var snapshot = new WindowsLanguagePlatform(api).Read();
            if (snapshot.Target != null) Assert.NotEqual((uint)Environment.ProcessId, snapshot.Target.Process);
        }
        finally { notifications.Dispose(); }
        Assert.Equal(0, notifications.ActiveHookCount);
    }
}
