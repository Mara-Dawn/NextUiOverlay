using NextUIOverlay.Cef.App;
using System;
using System.IO;
using System.Threading;
using Xilium.CefGlue;

namespace NextUIOverlay.Cef;

internal static class CefHandler
{
    public static void Initialize(string cacheDir, string cefDir, string logDir)
    {
        CefSettings settings = new CefSettings()
        {
            CachePath = cacheDir,
            MultiThreadedMessageLoop = true,
            UncaughtExceptionStackSize = 5,
            WindowlessRenderingEnabled = true,
            BrowserSubprocessPath = Path.Combine(cefDir, "CustomSubProcess.exe"),
            LogFile = Path.Combine(logDir, "cef-debug.log"),
            LogSeverity = CefLogSeverity.Fatal
        };
        CefMainArgs args = new CefMainArgs(Array.Empty<string>());
        NUCefApp application = new NUCefApp();
        CefRuntime.Load(cefDir);
        CefRuntime.EnableHighDpiSupport();
        CefRuntime.Initialize(args, settings, (CefApp)application, IntPtr.Zero);
    }

    public static void Shutdown()
    {
        CefRuntime.Shutdown();
        Thread.Sleep(1000);
    }
}