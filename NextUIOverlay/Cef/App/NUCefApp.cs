using Dalamud.Plugin.Services;
using System;
using System.Runtime.CompilerServices;
using Xilium.CefGlue;

namespace NextUIOverlay.Cef.App;

public class NUCefApp : CefApp
{
    protected override void OnBeforeCommandLineProcessing(
        string processType,
        CefCommandLine commandLine)
    {
        if (!string.IsNullOrEmpty(processType))
            return;
        commandLine.AppendSwitch("autoplay-policy", "no-user-gesture-required");
        if (commandLine.HasSwitch("disable-features"))
        {
            string str = commandLine.GetSwitchValue("disable-features") + ",ForcedColors";
            commandLine.AppendSwitch("disable-features", str);
        }
        else
            commandLine.AppendSwitch("disable-features", "ForcedColors");

        if (!commandLine.HasSwitch("disable-gpu"))
            commandLine.AppendSwitch("disable-gpu");
        if (!commandLine.HasSwitch("disable-gpu-compositing"))
            commandLine.AppendSwitch("disable-gpu-compositing");
        if (!commandLine.HasSwitch("enable-begin-frame-scheduling"))
            commandLine.AppendSwitch("enable-begin-frame-scheduling");
        IPluginLog pluginLog = NextUIPlugin.pluginLog;
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
        interpolatedStringHandler.AppendLiteral("CLI Switches: ");
        interpolatedStringHandler.AppendFormatted<CefCommandLine>(commandLine);
        string stringAndClear = interpolatedStringHandler.ToStringAndClear();
        object[] objArray = Array.Empty<object>();
        pluginLog.Information(stringAndClear, objArray);
    }
}