﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using NextUIOverlay.Configuration;
using NextUIOverlay.Model;
using NextUIOverlay.Service;
using D3D11 = SharpDX.Direct3D11;

namespace NextUIOverlay.Gui
{
    public class GuiManager : IDisposable
    {
        public readonly List<OverlayGui> overlays = new();

        public long AdapterLuid { get; set; }
        public bool MicroPluginFullyLoaded { get; set; }

        public GuiManager(IPluginLog pluginLog) => this.PluginLog = pluginLog;
        public IPluginLog PluginLog { get; set; }

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            // Spin up DX handling from the plugin interface
            DxHandler.Initialize(pluginInterface);
            AdapterLuid = DxHandler.AdapterLuid;

            // Spin up WndProc hook
            WndProcHandler.Initialize(DxHandler.WindowHandle);
            WndProcHandler.WndProcMessage += OnWndProc;
        }

        // Overlay initialization code here, we need to wait till plugin fully loads
        public void MicroPluginLoaded()
        {
            MicroPluginFullyLoaded = true;
            // loading ov
            PluginLog.Information("OnMicroPluginFullyLoaded");
            LoadOverlays(NextUIPlugin.configuration.overlays);
        }

        protected (bool, long) OnWndProc(WindowsMessageS msg, ulong wParam, long lParam)
        {
            var responses = new List<(bool, long)>();
            foreach (var ov in overlays.ToArray())
            {
                responses.Add(ov.WndProcMessage(msg, wParam, lParam));
            }

            return responses.FirstOrDefault(ov => ov.Item1);
        }

        public OverlayGui? CreateOverlay(string url, Size size)
        {
            if (!MicroPluginFullyLoaded)
            {
                PluginLog.Warning("Overlay not created, MicroPlugin not ready");
                return null;
            }

            var overlay = new Overlay(url, size);
            var fsSize = ImGui.GetMainViewport().Size;
            overlay.FullScreenSize = new Size((int)fsSize.X, (int)fsSize.Y);

            var overlayGui = new OverlayGui(overlay, this.PluginLog);
            overlays.Add(overlayGui);

            return overlayGui;
        }

        public void RemoveOverlay(OverlayGui overlay)
        {
            overlays.Remove(overlay);
        }

        public void Render()
        {
            if (loadingOverlays)
            {
                return;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

            foreach (var ov in overlays)
            {
                ov.Render();
            }

            ImGui.PopStyleVar();
        }

        public void ToggleOverlays()
        {
            foreach (var ov in overlays)
            {
                ov.overlay.Toggled = !ov.overlay.Toggled;
            }
        }

        public void ReloadOverlays()
        {
            foreach (var ov in overlays)
            {
                ov.Reload();
            }
        }

        protected bool loadingOverlays;

        public void LoadOverlays(List<OverlayConfig> newOverlays)
        {
            loadingOverlays = true;
            if (!MicroPluginFullyLoaded)
            {
                PluginLog.Warning("Overlay not created, MicroPlugin not ready");
                loadingOverlays = false;
                return;
            }

            foreach (var overlayCfg in newOverlays)
            {
                var overlay = overlayCfg.ToOverlay();

                // Reload full screen size
                var fsSize = ImGui.GetMainViewport().Size;
                overlay.FullScreenSize = new Size((int)fsSize.X, (int)fsSize.Y);

                var overlayGui = new OverlayGui(overlay, this.PluginLog);
                overlays.Add(overlayGui);
            }

            loadingOverlays = false;
        }

        public List<OverlayConfig> SaveOverlays()
        {
            return overlays.Select(overlay => OverlayConfig.FromOverlay(overlay.overlay)).ToList();
        }

        public void Dispose()
        {
            foreach (var ov in overlays.ToList())
            {
                ov.Dispose();
            }

            WndProcHandler.Shutdown();
            DxHandler.Shutdown();
        }
    }
}