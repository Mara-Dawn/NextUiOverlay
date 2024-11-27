using Dalamud.Configuration;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using NextUIOverlay.Configuration;
using NextUIOverlay.Gui;
using NextUIOverlay.Service;
using NextUIOverlay.Socket;
using System;

#nullable enable
namespace NextUIOverlay;

public class NextUIPlugin : IDalamudPlugin, IDisposable
{
    public static NextUIConfiguration configuration;
    public static GuiManager guiManager;
    public static NextUISocket socketServer;
    public static ExcelSheet<BNpcName> npcNameSheet;

    public string Name => "NextUIOverlay";

    [PluginService] public static ICommandManager commandManager { get; set; }

    [PluginService] public static IDalamudPluginInterface pluginInterface { get; set; }

    [PluginService] public static IDataManager dataManager { get; set; }

    [PluginService] public static ICondition condition { get; set; }

    [PluginService] public static IPartyList partyList { get; set; }

    [PluginService] public static IPluginLog pluginLog { get; set; }

    public NextUIPlugin()
    {
        NextUIPlugin.pluginInterface.UiBuilder.DisableCutsceneUiHide = true;
        NextUIPlugin.npcNameSheet = NextUIPlugin.dataManager.GetExcelSheet<BNpcName>();
        if (!(NextUIPlugin.pluginInterface.GetPluginConfig() is NextUIConfiguration nextUiConfiguration))
            nextUiConfiguration = new NextUIConfiguration();
        NextUIPlugin.configuration = nextUiConfiguration;
        NextUIPlugin.configuration.PrepareConfiguration();
        NextUIPlugin.pluginLog.Information(JsonConvert.SerializeObject((object)NextUIPlugin.configuration),
            Array.Empty<object>());
        NextUIPlugin.pluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
        NextUIPlugin.pluginInterface.UiBuilder.Draw += Render;
        NextUIPlugin.socketServer = new NextUISocket(NextUIPlugin.configuration.socketPort);
        NextUIPlugin.socketServer.Start();
        NextUIPlugin.guiManager = new GuiManager(NextUIPlugin.pluginLog);
        NextUIPlugin.guiManager.Initialize(NextUIPlugin.pluginInterface);
        MicroPluginService.Initialize();
        commandManager.AddHandler("/nu", new CommandInfo(OnCommandDebugCombo) {
            HelpMessage = "Open NextUI Plugin configuration. \n" +
                          "/nu toggle → Toggles all visible overlays.",
            ShowInHelp = true
        });

    }

    public void OnOpenConfigUi() => ConfigWindow.isConfigOpen = true;

    public void Render()
    {
        NextUIPlugin.guiManager?.Render();
        MicroPluginService.DrawProgress();
        MicroPluginService.DrawWarningWindow();
        if (!ConfigWindow.isConfigOpen)
            return;
        ConfigWindow.RenderConfig();
    }

    public void Dispose()
    {
        NextUIPlugin.commandManager.RemoveHandler("/nu");
        NextUIPlugin.socketServer.Dispose();
        NextUIPlugin.guiManager?.Dispose();
        MicroPluginService.Shutdown();
    }

    protected void OnCommandDebugCombo(string command, string arguments)
    {
        switch (arguments.Split()[0])
        {
            case "toggle":
                NextUIPlugin.guiManager.ToggleOverlays();
                break;
            case "reload":
                NextUIPlugin.guiManager.ReloadOverlays();
                break;
            default:
                ConfigWindow.isConfigOpen = true;
                break;
        }

        NextUIPlugin.pluginInterface.SavePluginConfig((IPluginConfiguration)NextUIPlugin.configuration);
    }
}