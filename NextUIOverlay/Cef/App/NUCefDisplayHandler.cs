using Dalamud.Plugin.Services;
using NextUIOverlay.Data.Input;
using System;
using System.Runtime.CompilerServices;
using Xilium.CefGlue;

#nullable enable
namespace NextUIOverlay.Cef.App;

public class NUCefDisplayHandler : CefDisplayHandler
{
  protected NUCefRenderHandler renderHandler;
  protected bool cursorOnBackground;
  protected Cursor cursor;

  public event EventHandler<Cursor>? CursorChanged;

  public NUCefDisplayHandler(NUCefRenderHandler renderHandler)
  {
    this.renderHandler = renderHandler;
  }

  public void SetMousePosition(int x, int y)
  {
    bool flag = this.renderHandler.GetAlphaAt(x, y) == (byte) 0;
    if (flag == this.cursorOnBackground)
      return;
    this.cursorOnBackground = flag;
    EventHandler<Cursor> cursorChanged = this.CursorChanged;
    if (cursorChanged == null)
      return;
    cursorChanged((object) this, flag ? Cursor.BrowserHostNoCapture : this.cursor);
  }

  protected override bool OnAutoResize(CefBrowser browser, ref CefSize newSize)
  {
    NextUIPlugin.pluginLog.Information("RESIZE FINISHED " + newSize.ToString(), Array.Empty<object>());
    return false;
  }

  protected override bool OnCursorChange(
    CefBrowser browser,
    IntPtr cursorHandle,
    CefCursorType type,
    CefCursorInfo customCursorInfo)
  {
    this.cursor = NUCefDisplayHandler.EncodeCursor(type);
    if (!this.cursorOnBackground)
    {
      EventHandler<Cursor> cursorChanged = this.CursorChanged;
      if (cursorChanged != null)
        cursorChanged((object) this, this.cursor);
    }
    return false;
  }

  protected static Cursor EncodeCursor(CefCursorType cefCursor)
  {
    switch (cefCursor)
    {
      case CefCursorType.Pointer:
        return Cursor.Default;
      case CefCursorType.Cross:
        return Cursor.Crosshair;
      case CefCursorType.Hand:
        return Cursor.Pointer;
      case CefCursorType.IBeam:
        return Cursor.Text;
      case CefCursorType.Wait:
        return Cursor.Wait;
      case CefCursorType.Help:
        return Cursor.Help;
      case CefCursorType.EastResize:
        return Cursor.EResize;
      case CefCursorType.NorthResize:
        return Cursor.NResize;
      case CefCursorType.NorthEastResize:
        return Cursor.NEResize;
      case CefCursorType.NorthWestResize:
        return Cursor.NWResize;
      case CefCursorType.SouthResize:
        return Cursor.SResize;
      case CefCursorType.SouthEastResize:
        return Cursor.SEResize;
      case CefCursorType.SouthWestResize:
        return Cursor.SWResize;
      case CefCursorType.WestResize:
        return Cursor.WResize;
      case CefCursorType.NorthSouthResize:
        return Cursor.NSResize;
      case CefCursorType.EastWestResize:
        return Cursor.EWResize;
      case CefCursorType.NorthEastSouthWestResize:
        return Cursor.NESWResize;
      case CefCursorType.NorthWestSouthEastResize:
        return Cursor.NWSEResize;
      case CefCursorType.ColumnResize:
        return Cursor.ColResize;
      case CefCursorType.RowResize:
        return Cursor.RowResize;
      case CefCursorType.MiddlePanning:
      case CefCursorType.EastPanning:
      case CefCursorType.NorthPanning:
      case CefCursorType.NorthEastPanning:
      case CefCursorType.NorthWestPanning:
      case CefCursorType.SouthPanning:
      case CefCursorType.SouthEastPanning:
      case CefCursorType.SouthWestPanning:
      case CefCursorType.WestPanning:
        return Cursor.AllScroll;
      case CefCursorType.Move:
        return Cursor.Move;
      case CefCursorType.VerticalText:
        return Cursor.VerticalText;
      case CefCursorType.Cell:
        return Cursor.Cell;
      case CefCursorType.ContextMenu:
        return Cursor.ContextMenu;
      case CefCursorType.Alias:
        return Cursor.Alias;
      case CefCursorType.Progress:
        return Cursor.Progress;
      case CefCursorType.NoDrop:
        return Cursor.NoDrop;
      case CefCursorType.Copy:
        return Cursor.Copy;
      case CefCursorType.None:
        return Cursor.None;
      case CefCursorType.NotAllowed:
        return Cursor.NotAllowed;
      case CefCursorType.ZoomIn:
        return Cursor.ZoomIn;
      case CefCursorType.ZoomOut:
        return Cursor.ZoomOut;
      case CefCursorType.Grab:
        return Cursor.Grab;
      case CefCursorType.Grabbing:
        return Cursor.Grabbing;
      case CefCursorType.Custom:
        return Cursor.Default;
      default:
        IPluginLog pluginLog = NextUIPlugin.pluginLog;
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
        interpolatedStringHandler.AppendLiteral("Switching to unmapped cursor type ");
        interpolatedStringHandler.AppendFormatted<CefCursorType>(cefCursor);
        interpolatedStringHandler.AppendLiteral(".");
        string stringAndClear = interpolatedStringHandler.ToStringAndClear();
        object[] objArray = Array.Empty<object>();
        pluginLog.Warning(stringAndClear, objArray);
        return Cursor.Default;
    }
  }
}