namespace NextUIOverlay.Service;

using Dalamud.Plugin.Services;
using ImGuiNET;
using NextUIOverlay.Cef;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
public static class MicroPluginService
{
    internal const string MicroPluginDirName = "MicroPlugin";
    internal const string RequiredVersion = "8.0.0.3";
    internal static string? pluginDir;

    internal static readonly string baseDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NUCefSharp");

    internal static readonly string pidFile = Path.Combine(MicroPluginService.baseDir, "pid.txt");
    internal static readonly string cacheDir = Path.Combine(MicroPluginService.baseDir, "Cache");
    private static float downloadProgress = -1f;
    private static bool showWindowWarning;
    private static string warningMessage = "";
    internal static Thread microPluginThread = (Thread)null;
    internal static readonly ManualResetEventSlim microPluginResetEvent = new ManualResetEventSlim();
    internal static int lastPid;

    public static void Initialize()
    {
        MicroPluginService.pluginDir = NextUIPlugin.pluginInterface.AssemblyLocation.DirectoryName;
        MicroPluginService.ReadLastPid();
        MicroPluginService.microPluginThread = new Thread(new ThreadStart(MicroPluginService.LoadMicroPlugin));
        MicroPluginService.microPluginThread.Start();
    }

    public static void Shutdown()
    {
        MicroPluginService.microPluginResetEvent.Set();
        MicroPluginService.microPluginThread.Join(100);
    }

    public static async void LoadMicroPlugin()
    {
        string cefDir;
        if (MicroPluginService.pluginDir == null)
        {
            NextUIPlugin.pluginLog.Error("Unable to load MicroPlugin, unexpected error", Array.Empty<object>());
            cefDir = (string)null;
        }
        else
        {
            if (!Directory.Exists(MicroPluginService.baseDir))
                Directory.CreateDirectory(MicroPluginService.baseDir);
            string str1 = Path.Combine(MicroPluginService.baseDir, "MicroPlugin");
            string path2 = "NextUIBrowser.dll";
            string str2 = Path.Combine(str1, path2);
            cefDir = str1;
            bool flag = false;
            if (!Directory.Exists(str1))
            {
                MicroPluginService.warningMessage = "MicroPlugin directory does not exist";
                flag = true;
            }
            else if (!File.Exists(str2))
            {
                MicroPluginService.warningMessage = "MicroPlugin DLL does not exist";
                flag = true;
            }
            else
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(str2);
                NextUIPlugin.pluginLog.Information("MicroPlugin version " + versionInfo.FileVersion,
                    Array.Empty<object>());
                if (versionInfo.FileVersion != "8.0.0.3")
                    flag = false;
            }

            if (flag)
            {
                if (MicroPluginService.IsColdBoot())
                {
                    await MicroPluginService.DownloadMicroPlugin(str1);
                }
                else
                {
                    MicroPluginService.warningMessage = "Unable to update, plugin please restart the game";
                    MicroPluginService.showWindowWarning = true;
                    NextUIPlugin.pluginLog.Error(MicroPluginService.warningMessage, Array.Empty<object>());
                    cefDir = (string)null;
                    return;
                }
            }

            if (!MicroPluginService.IsColdBoot())
            {
                MicroPluginService.warningMessage = "This plugin is not possible to reload, please restart the game";
                NextUIPlugin.pluginLog.Error(MicroPluginService.warningMessage, Array.Empty<object>());
            }

            NextUIPlugin.pluginLog.Information("Loaded MicroPlugin", Array.Empty<object>());
            MicroPluginService.InitializeBrowser(cefDir);
            MicroPluginService.WriteLastPid();
            MicroPluginService.microPluginResetEvent.Wait();
            MicroPluginService.ShutdownBrowser();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            cefDir = (string)null;
        }
    }

    public static void InitializeBrowser(string cefDir)
    {
        IPluginLog pluginLog = NextUIPlugin.pluginLog;
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 3);
        interpolatedStringHandler.AppendLiteral("Initializing Browser, ");
        interpolatedStringHandler.AppendFormatted(MicroPluginService.cacheDir);
        interpolatedStringHandler.AppendLiteral(", ");
        interpolatedStringHandler.AppendFormatted(cefDir);
        interpolatedStringHandler.AppendLiteral(", ");
        interpolatedStringHandler.AppendFormatted(MicroPluginService.baseDir);
        string stringAndClear = interpolatedStringHandler.ToStringAndClear();
        object[] objArray = Array.Empty<object>();
        pluginLog.Information(stringAndClear, objArray);
        CefHandler.Initialize(MicroPluginService.cacheDir, cefDir, MicroPluginService.baseDir);
        NextUIPlugin.guiManager.MicroPluginLoaded();
    }

    public static void ShutdownBrowser()
    {
        CefHandler.Shutdown();
        NextUIPlugin.pluginLog.Information("Cef was shut down", Array.Empty<object>());
    }

    internal static async Task DownloadMicroPlugin(string microPluginDir)
    {
        try
        {
            string downloadPath = Path.Combine(MicroPluginService.baseDir, "mp.zip");
            if (File.Exists(downloadPath))
                File.Delete(downloadPath);
            string uriString =
                "https://gitlab.com/kaminariss/nextui-plugin/-/jobs/artifacts/v8.0.0.3/raw/NextUIBrowser/bin/latest.zip?job=build";
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadProgressChanged += (DownloadProgressChangedEventHandler)((_, e) =>
                {
                    MicroPluginService.downloadProgress = (float)e.ProgressPercentage;
                    NextUIPlugin.pluginLog.Information("MicroPlugin progress " + e.ProgressPercentage.ToString(),
                        Array.Empty<object>());
                });
                webClient.DownloadFileCompleted +=
                    (AsyncCompletedEventHandler)((_1, _2) => MicroPluginService.downloadProgress = 100f);
                await webClient.DownloadFileTaskAsync(new Uri(uriString), downloadPath);
                NextUIPlugin.pluginLog.Information("Downloaded latest MicroPlugin", Array.Empty<object>());
                if (Directory.Exists(microPluginDir))
                    Directory.Delete(microPluginDir, true);
                Directory.CreateDirectory(microPluginDir);
                ZipFile.ExtractToDirectory(downloadPath, microPluginDir);
                NextUIPlugin.pluginLog.Information("Extracted MicroPlugin", Array.Empty<object>());
                if (File.Exists(downloadPath))
                    File.Delete(downloadPath);
            }
            finally
            {
                webClient?.Dispose();
            }

            downloadPath = (string)null;
            webClient = (WebClient)null;
        }
        catch (Exception ex)
        {
            NextUIPlugin.pluginLog.Warning("Unable to download MicroPlugin", Array.Empty<object>());
        }
    }

    public static void DrawProgress()
    {
        float downloadProgress = MicroPluginService.downloadProgress;
        if ((double)downloadProgress >= 100.0 || (double)downloadProgress < 0.0)
            return;
        ImGui.SetNextWindowSize(new Vector2(300f, 70f));
        ImGui.Begin("Downloading MicroPlugin", (ImGuiWindowFlags)42);
        ImGui.ProgressBar(MicroPluginService.downloadProgress / 100f, new Vector2(280f, 30f));
        ImGui.End();
    }

    public static void DrawWarningWindow()
    {
        if (!MicroPluginService.showWindowWarning)
            return;
        ImGui.SetNextWindowSize(new Vector2(500f, 80f));
        ImGui.Begin("NextUI", ref MicroPluginService.showWindowWarning, (ImGuiWindowFlags)42);
        ImGui.TextColored(new Vector4(1f, 0.0f, 0.0f, 1f), MicroPluginService.warningMessage);
        ImGui.End();
    }

    internal static void ReadLastPid()
    {
        if (!File.Exists(MicroPluginService.pidFile))
            return;
        string s = File.ReadAllText(MicroPluginService.pidFile);
        try
        {
            MicroPluginService.lastPid = int.Parse(s);
        }
        catch (Exception ex)
        {
            MicroPluginService.lastPid = 0;
        }
    }

    internal static void WriteLastPid()
    {
        if (!Directory.Exists(MicroPluginService.baseDir))
            Directory.CreateDirectory(MicroPluginService.baseDir);
        File.WriteAllText(MicroPluginService.pidFile, Environment.ProcessId.ToString());
    }

    internal static bool IsColdBoot() => MicroPluginService.lastPid != Environment.ProcessId;
}