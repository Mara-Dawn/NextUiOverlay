using NextUIOverlay.Model;
using System;
using Xilium.CefGlue;

#nullable enable
namespace NextUIOverlay.Cef.App;

public class NUCefClient : CefClient, IDisposable
{
    public readonly NUCefLoadHandler loadHandler;
    public readonly NUCefRenderHandler renderHandler;
    public readonly NUCefLifeSpanHandler lifeSpanHandler;
    public readonly NUCefDisplayHandler displayHandler;
    public readonly NUCefContextMenuHandler dialogHandler;

    public NUCefClient(Overlay overlay)
    {
        this.loadHandler = new NUCefLoadHandler();
        this.dialogHandler = new NUCefContextMenuHandler();
        this.renderHandler = new NUCefRenderHandler(overlay);
        this.displayHandler = new NUCefDisplayHandler(this.renderHandler);
        this.lifeSpanHandler = new NUCefLifeSpanHandler();
    }

    protected override CefRenderHandler GetRenderHandler() => (CefRenderHandler) this.renderHandler;

    protected override CefLoadHandler GetLoadHandler() => (CefLoadHandler) this.loadHandler;

    protected override CefLifeSpanHandler GetLifeSpanHandler()
    {
        return (CefLifeSpanHandler) this.lifeSpanHandler;
    }

    protected override CefDisplayHandler GetDisplayHandler()
    {
        return (CefDisplayHandler) this.displayHandler;
    }

    protected override CefContextMenuHandler GetContextMenuHandler()
    {
        return (CefContextMenuHandler) this.dialogHandler;
    }

    public void Dispose()
    {
        this.renderHandler.Dispose();
        GC.SuppressFinalize((object) this);
    }
}