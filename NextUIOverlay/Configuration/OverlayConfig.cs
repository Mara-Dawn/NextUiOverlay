using NextUIOverlay.Model;
using System;
using System.Drawing;

#nullable enable
namespace NextUIOverlay.Configuration;

[Serializable]
public class OverlayConfig
{
  public string Guid { get; set; }

  public string Name { get; set; }

  public string Url { get; set; }

  public string Size { get; set; }

  public string Position { get; set; }

  public bool FullScreen { get; set; }

  public bool ClickThrough { get; set; }

  public bool TypeThrough { get; set; }

  public bool Locked { get; set; }

  public bool Hidden { get; set; }

  public OverlayVisibility VisibilityShow { get; set; }

  public OverlayVisibility VisibilityHide { get; set; }

  public string FullScreenSize { get; set; }

  public static OverlayConfig FromOverlay(Overlay overlay)
  {
    return new OverlayConfig()
    {
      Guid = overlay.Guid.ToString(),
      Name = overlay.Name,
      Url = overlay.Url,
      Size = OverlayConfig.SizeToString(overlay.Size),
      Position = OverlayConfig.PointToString(overlay.Position),
      FullScreen = overlay.FullScreen,
      ClickThrough = overlay.ClickThrough,
      TypeThrough = overlay.TypeThrough,
      Locked = overlay.Locked,
      Hidden = overlay.Hidden,
      VisibilityShow = overlay.VisibilityShow,
      VisibilityHide = overlay.VisibilityHide,
      FullScreenSize = OverlayConfig.SizeToString(overlay.FullScreenSize)
    };
  }

  public Overlay ToOverlay()
  {
    System.Drawing.Size size = OverlayConfig.ParseSize(this.Size);
    return new Overlay(this.Url, size)
    {
      Guid = new System.Guid(this.Guid),
      Name = this.Name,
      Url = this.Url,
      Size = size,
      Position = OverlayConfig.ParsePoint(this.Position),
      FullScreen = this.FullScreen,
      ClickThrough = this.ClickThrough,
      TypeThrough = this.TypeThrough,
      Locked = this.Locked,
      Hidden = this.Hidden,
      VisibilityShow = this.VisibilityShow,
      VisibilityHide = this.VisibilityHide,
      FullScreenSize = OverlayConfig.ParseSize(this.FullScreenSize)
    };
  }

  protected static Point ParsePoint(string value)
  {
    string[] strArray = value.Split('|');
    return new Point(int.Parse(strArray[0]), int.Parse(strArray[1]));
  }

  protected static System.Drawing.Size ParseSize(string value)
  {
    string[] strArray = value.Split('|');
    return new System.Drawing.Size(int.Parse(strArray[0]), int.Parse(strArray[1]));
  }

  protected static string SizeToString(System.Drawing.Size size)
  {
    int num = size.Width;
    string str1 = num.ToString();
    num = size.Height;
    string str2 = num.ToString();
    return str1 + "|" + str2;
  }

  protected static string PointToString(Point size)
  {
    int num = size.X;
    string str1 = num.ToString();
    num = size.Y;
    string str2 = num.ToString();
    return str1 + "|" + str2;
  }
}