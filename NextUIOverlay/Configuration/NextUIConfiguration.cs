using Dalamud.Configuration;
using ImGuiNET;
using NextUIOverlay.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

#nullable enable
namespace NextUIOverlay.Configuration
{
  [Serializable]
  public class NextUIConfiguration : IPluginConfiguration
  {
    public int socketPort = 32805;
    public bool firstInstalled = true;
    public List<OverlayConfig> overlays = new List<OverlayConfig>();

    public int Version { get; set; } = 3;

    public void PrepareConfiguration() {
      if (socketPort is <= 1024 or > short.MaxValue) {
        NextUIPlugin.pluginLog.Info("Resetting port to 32805");
        socketPort = 32805;
      }

      if (firstInstalled) {
        var ov = new Overlay(
          "https://kaminaris.github.io/Next-UI/?OVERLAY_WS=ws://127.0.0.1:10501/ws",
          new Size(800, 600)
        );
        var fsSize = ImGui.GetMainViewport().Size;
        ov.FullScreenSize = new Size((int)fsSize.X, (int)fsSize.Y);
        ov.FullScreen = true;
        ov.Name = "NextUI";
        overlays.Add(OverlayConfig.FromOverlay(ov));

        firstInstalled = false;
      }
    }

  }
}
