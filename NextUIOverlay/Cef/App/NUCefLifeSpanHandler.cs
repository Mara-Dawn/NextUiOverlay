using Dalamud.Plugin.Services;
using System;
using System.Runtime.CompilerServices;
using Xilium.CefGlue;

#nullable enable
namespace NextUIOverlay.Cef.App;

public class NUCefLifeSpanHandler : CefLifeSpanHandler
{
  public event Action<CefBrowser>? AfterBrowserLoad;

  public event Action<CefBrowser>? AfterBrowserPopupLoad;

  protected override void OnAfterCreated(CefBrowser browser)
  {
    if (browser.IsPopup)
    {
      IPluginLog pluginLog = NextUIPlugin.pluginLog;
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 1);
      interpolatedStringHandler.AppendLiteral("Browser popup created ");
      interpolatedStringHandler.AppendFormatted<bool>(browser.IsValid);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      object[] objArray = Array.Empty<object>();
      pluginLog.Information(stringAndClear, objArray);
      Action<CefBrowser> browserPopupLoad = this.AfterBrowserPopupLoad;
      if (browserPopupLoad == null)
        return;
      browserPopupLoad(browser);
    }
    else
    {
      IPluginLog pluginLog = NextUIPlugin.pluginLog;
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
      interpolatedStringHandler.AppendLiteral("Browser created ");
      interpolatedStringHandler.AppendFormatted<bool>(browser.IsValid);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      object[] objArray = Array.Empty<object>();
      pluginLog.Information(stringAndClear, objArray);
      Action<CefBrowser> afterBrowserLoad = this.AfterBrowserLoad;
      if (afterBrowserLoad == null)
        return;
      afterBrowserLoad(browser);
    }
  }
}