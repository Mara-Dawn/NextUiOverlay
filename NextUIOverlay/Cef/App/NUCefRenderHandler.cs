using NextUIOverlay.Model;
using System;
using System.Drawing;
using Xilium.CefGlue;

#nullable enable
namespace NextUIOverlay.Cef.App;

public class NUCefRenderHandler : CefRenderHandler, IDisposable
{
  protected const byte BytesPerPixel = 4;
  protected readonly Overlay overlay;
  protected byte[] internalBuffer = Array.Empty<byte>();
  protected int bufferWidth;
  protected int bufferHeight;
  protected int width;
  protected int height;

  public event Action<IntPtr, int, int>? Paint;

  public NUCefRenderHandler(Overlay overlay)
  {
    this.overlay = overlay;
    this.width = overlay.Size.Width;
    this.height = overlay.Size.Height;
  }

  protected override CefAccessibilityHandler GetAccessibilityHandler()
  {
    return (CefAccessibilityHandler) null;
  }

  public void Resize(Size size)
  {
    this.width = size.Width;
    this.height = size.Height;
  }

  public byte GetAlphaAt(int x, int y)
  {
    lock (this.overlay.renderLock)
    {
      int num = this.bufferWidth * 4;
      int index = Math.Min(Math.Max(x, 0), this.bufferWidth - 1) * 4 + Math.Min(Math.Max(y, 0), this.bufferHeight - 1) * num + 3;
      if (index < this.internalBuffer.Length)
      {
        try
        {
          return this.internalBuffer[index];
        }
        catch
        {
          return byte.MaxValue;
        }
      }
      else
      {
        Console.WriteLine("Could not determine alpha value");
        return byte.MaxValue;
      }
    }
  }

  protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
  {
    rect.X = 0;
    rect.Y = 0;
    rect.Width = this.width;
    rect.Height = this.height;
    return true;
  }

  protected override void GetViewRect(CefBrowser browser, out CefRectangle rect)
  {
    rect = DpiScaling.ScaleViewRect(new CefRectangle(0, 0, this.width, this.height));
  }

  protected override bool GetScreenPoint(
    CefBrowser browser,
    int viewX,
    int viewY,
    ref int screenX,
    ref int screenY)
  {
    screenX = viewX;
    screenY = viewY;
    return true;
  }

  protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
  {
    screenInfo.DeviceScaleFactor = DpiScaling.GetDeviceScale();
    return true;
  }

  protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
  {
  }

  protected override unsafe void OnPaint(
    CefBrowser browser,
    CefPaintElementType type,
    CefRectangle[] dirtyRects,
    IntPtr buffer,
    int paintWidth,
    int paintHeight)
  {
    if (type == CefPaintElementType.Popup)
      return;
    lock (this.overlay.renderLock)
    {
      int sourceBytesToCopy = paintWidth * paintHeight * 4;
      this.bufferWidth = paintWidth;
      this.bufferHeight = paintHeight;
      if (this.internalBuffer.Length != sourceBytesToCopy)
        this.internalBuffer = new byte[this.bufferWidth * this.bufferHeight * 4];
      fixed (byte* destination = this.internalBuffer)
      {
        Buffer.MemoryCopy(buffer.ToPointer(), (void*) destination, (long) this.internalBuffer.Length, (long) sourceBytesToCopy);
        this.overlay.Resizing = false;
        Action<IntPtr, int, int> paint = this.Paint;
        if (paint != null)
          paint((IntPtr) destination, paintWidth, paintHeight);
      }
    }
  }

  protected override void OnAcceleratedPaint(
    CefBrowser browser,
    CefPaintElementType type,
    CefRectangle[] dirtyRects,
    IntPtr sharedHandle)
  {
  }

  protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
  {
  }

  protected override void OnImeCompositionRangeChanged(
    CefBrowser browser,
    CefRange selectedRange,
    CefRectangle[] characterBounds)
  {
  }

  public void Dispose() => GC.SuppressFinalize((object) this);
}