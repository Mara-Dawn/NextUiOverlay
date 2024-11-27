using Dalamud.Configuration;
using ImGuiNET;
using NextUIOverlay.Gui;
using NextUIOverlay.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

#nullable enable
namespace NextUIOverlay.Configuration;

public static class ConfigWindow
{
  public static bool isConfigOpen;
  internal static OverlayGui? selectedOverlay;

  public static void RenderConfig()
  {
    ImGui.SetNextWindowSize(new Vector2(640f, 480f));
    ImGui.Begin("NextUI Configuration", ref ConfigWindow.isConfigOpen, (ImGuiWindowFlags) 42);
    ImGui.Text("Websocket Server Port");
    ImGui.SameLine();
    ImGui.TextColored(new Vector4(1f, 0.0f, 0.0f, 1f), "WARNING: Do not touch unless you know what you are doing");
    ImGui.Separator();
    ImGui.InputInt("Socket Port", ref NextUIPlugin.configuration.socketPort);
    ImGui.Text("Overlays");
    ImGui.Separator();
    ConfigWindow.RenderPaneSelector();
    ConfigWindow.RenderOverlayPane();
    ImGui.SetCursorPos(new Vector2(8f, 450f));
    if (ImGui.Button("Save"))
      ConfigWindow.SaveConfig();
    ImGui.SameLine();
    if (ImGui.Button("Save and Close"))
    {
      ConfigWindow.SaveConfig();
      ConfigWindow.isConfigOpen = false;
    }
    ImGui.End();
  }

  internal static void SaveConfig()
  {
    NextUIPlugin.configuration.overlays = NextUIPlugin.guiManager.SaveOverlays();
    if (NextUIPlugin.socketServer.Port != NextUIPlugin.configuration.socketPort)
    {
      NextUIPlugin.socketServer.Port = NextUIPlugin.configuration.socketPort;
      NextUIPlugin.socketServer.Restart();
    }
    NextUIPlugin.pluginInterface.SavePluginConfig((IPluginConfiguration) NextUIPlugin.configuration);
  }

  internal static void RenderPaneSelector()
  {
    ImGui.BeginGroup();
    ImGui.PushStyleVar((ImGuiStyleVar) 13, new Vector2(0.0f, 0.0f));
    int x = 200;
    ImGui.BeginChild("panes", new Vector2((float) x, 300f), true);
    foreach (OverlayGui overlay in NextUIPlugin.guiManager.overlays)
    {
      if (ImGui.Selectable(overlay.overlay.Name + "##" + overlay.overlay.Guid.ToString(), ConfigWindow.selectedOverlay == overlay))
        ConfigWindow.selectedOverlay = overlay;
    }
    ImGui.EndChild();
    ImGui.PushStyleVar((ImGuiStyleVar) 11, 0.0f);
    if (ImGui.Button("Add", new Vector2((float) x, 0.0f)))
      ConfigWindow.selectedOverlay = NextUIPlugin.guiManager.CreateOverlay("https://google.com", new Size(800, 600));
    ImGui.PopStyleVar(2);
    ImGui.EndGroup();
  }

  internal static void RenderOverlayPane()
  {
    ImGui.SameLine();
    ImGui.BeginChild("details");
    ConfigWindow.RenderOverlayConfig();
    ImGui.EndChild();
  }

  internal static void RenderOverlayConfig()
  {
    if (ConfigWindow.selectedOverlay == null)
      return;
    Overlay overlay1 = ConfigWindow.selectedOverlay.overlay;
    ImGui.PushID(overlay1.Guid.ToString());
    string name = overlay1.Name;
    if (ImGui.InputText("Name", ref name, 150U))
      overlay1.Name = name;
    string url = overlay1.Url;
    ImGui.InputText("URL", ref url, 1000U);
    if (ImGui.IsItemDeactivatedAfterEdit())
    {
      ConfigWindow.selectedOverlay.overlay.Url = url;
      ConfigWindow.selectedOverlay.Navigate(url);
    }
    ImGui.SetNextItemWidth(140f);
    ImGui.Columns(2, "overlayOptions", false);
    int x1 = overlay1.Position.X;
    Point position;
    if (ImGui.DragInt("Position X", ref x1, 1f))
    {
      Overlay overlay2 = overlay1;
      int x2 = x1;
      position = overlay1.Position;
      int y = position.Y;
      Point point = new Point(x2, y);
      overlay2.Position = point;
    }
    ImGui.NextColumn();
    position = overlay1.Position;
    int y1 = position.Y;
    if (ImGui.DragInt("Position Y", ref y1, 1f))
    {
      Overlay overlay3 = overlay1;
      position = overlay1.Position;
      Point point = new Point(position.X, y1);
      overlay3.Position = point;
    }
    ImGui.NextColumn();
    int width1 = overlay1.Size.Width;
    Size size1;
    if (ImGui.DragInt("Width", ref width1, 1f))
    {
      Overlay overlay4 = overlay1;
      int width2 = width1;
      size1 = overlay1.Size;
      int height = size1.Height;
      Size size2 = new Size(width2, height);
      overlay4.Size = size2;
    }
    ImGui.NextColumn();
    size1 = overlay1.Size;
    int height1 = size1.Height;
    if (ImGui.DragInt("Height", ref height1, 1f))
    {
      Overlay overlay5 = overlay1;
      size1 = overlay1.Size;
      Size size3 = new Size(size1.Width, height1);
      overlay5.Size = size3;
    }
    ImGui.NextColumn();
    bool locked = overlay1.Locked;
    if (ImGui.Checkbox("Locked", ref locked))
      overlay1.Locked = locked;
    if (ImGui.IsItemHovered())
      ImGui.SetTooltip("Prevent the overlay from being resized or moved.");
    ImGui.NextColumn();
    bool hidden = overlay1.Hidden;
    if (ImGui.Checkbox("Hidden", ref hidden))
      overlay1.Hidden = hidden;
    if (ImGui.IsItemHovered())
      ImGui.SetTooltip("This does not stop the overlay from executing, only from being displayed.");
    ImGui.NextColumn();
    bool typeThrough = overlay1.TypeThrough;
    if (ImGui.Checkbox("Type Through", ref typeThrough))
      overlay1.TypeThrough = typeThrough;
    if (ImGui.IsItemHovered())
      ImGui.SetTooltip("Prevent the overlay from intercepting any keyboard events.");
    ImGui.NextColumn();
    bool clickThrough = overlay1.ClickThrough;
    if (ImGui.Checkbox("Click Through", ref clickThrough))
      overlay1.ClickThrough = clickThrough;
    if (ImGui.IsItemHovered())
      ImGui.SetTooltip("Prevent the inlay from intercepting any mouse events.");
    ImGui.NextColumn();
    bool fullScreen = overlay1.FullScreen;
    if (ImGui.Checkbox("Fullscreen", ref fullScreen))
      overlay1.FullScreen = fullScreen;
    if (ImGui.IsItemHovered())
      ImGui.SetTooltip("Makes overlay over entire screen");
    ImGui.Columns(1);
    IEnumerable<OverlayVisibility> overlayVisibilities = Enum.GetValues(typeof (OverlayVisibility)).Cast<OverlayVisibility>();
    OverlayVisibility visibilityShow = overlay1.VisibilityShow;
    if (ImGui.BeginCombo("Show When", visibilityShow == (OverlayVisibility) 0 ? "" : visibilityShow.ToString()))
    {
      foreach (OverlayVisibility flag1 in overlayVisibilities)
      {
        bool flag2 = visibilityShow.HasFlag((Enum) flag1);
        if (ImGui.Checkbox(flag1.ToString(), ref flag2))
        {
          if (flag2)
            overlay1.VisibilityShow |= flag1;
          else
            overlay1.VisibilityShow ^= flag1;
        }
      }
      ImGui.EndCombo();
    }
    OverlayVisibility visibilityHide = overlay1.VisibilityHide;
    if (ImGui.BeginCombo("Hide When", visibilityHide == (OverlayVisibility) 0 ? "" : visibilityHide.ToString()))
    {
      foreach (OverlayVisibility flag3 in overlayVisibilities)
      {
        bool flag4 = visibilityHide.HasFlag((Enum) flag3);
        if (ImGui.Checkbox(flag3.ToString(), ref flag4))
        {
          if (flag4)
            overlay1.VisibilityHide |= flag3;
          else
            overlay1.VisibilityHide ^= flag3;
        }
      }
      ImGui.EndCombo();
    }
    if (ImGui.Button("Reload"))
      ConfigWindow.selectedOverlay.Reload();
    ImGui.SameLine();
    if (ImGui.Button("Open Dev Tools"))
      ConfigWindow.selectedOverlay.Debug();
    ImGui.SameLine();
    ImGui.PushStyleColor((ImGuiCol) 21, new Vector4(1f, 0.0f, 0.0f, 1f));
    if (ImGui.Button("Delete"))
    {
      NextUIPlugin.pluginLog.Info("Start remove", Array.Empty<object>());
      ConfigWindow.selectedOverlay.Dispose();
    }
    ImGui.PopStyleColor();
    ImGui.PopID();
  }
}