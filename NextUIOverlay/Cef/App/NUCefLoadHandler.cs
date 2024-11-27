using Dalamud.Plugin.Services;
using System;
using System.Runtime.CompilerServices;
using Xilium.CefGlue;

#nullable enable
namespace NextUIOverlay.Cef.App
{
    public class NUCefLoadHandler : CefLoadHandler
    {
        protected override void OnLoadStart(
            CefBrowser browser,
            CefFrame frame,
            CefTransitionType transitionType)
        {
            if (frame.IsMain)
                NextUIPlugin.pluginLog.Information("START: " + browser.GetMainFrame().Url, Array.Empty<object>());
            NextUIPlugin.pluginLog.Information("START WAT: " + frame.Url, Array.Empty<object>());
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            if (!frame.IsMain)
                return;
            IPluginLog pluginLog = NextUIPlugin.pluginLog;
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 2);
            interpolatedStringHandler.AppendLiteral("END: ");
            interpolatedStringHandler.AppendFormatted(browser.GetMainFrame().Url);
            interpolatedStringHandler.AppendLiteral(", ");
            interpolatedStringHandler.AppendFormatted<int>(httpStatusCode);
            string stringAndClear = interpolatedStringHandler.ToStringAndClear();
            object[] objArray = Array.Empty<object>();
            pluginLog.Information(stringAndClear, objArray);
        }
    }
}