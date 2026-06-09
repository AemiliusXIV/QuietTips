using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using QuietTips.Services;
using QuietTips.Windows;

namespace QuietTips;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Quiet Tips";

    private const string ConfigCommand = "/quiettips";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    internal Configuration Config { get; }
    internal WindowSystem WindowSystem { get; } = new("QuietTips");
    internal ConfigWindow ConfigWindow { get; }
    internal TooltipVisibilityController Controller { get; }

    public Plugin()
    {
        Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.Initialize(PluginInterface);

        Controller = new TooltipVisibilityController(Config);
        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += OpenSettings;
        PluginInterface.UiBuilder.OpenMainUi += OpenSettings;

        CommandManager.AddHandler(ConfigCommand, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open Quiet Tips settings. '/quiettips toggle|on|off' switches it without opening the window.",
            ShowInHelp = true,
        });
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenSettings;
        PluginInterface.UiBuilder.OpenMainUi -= OpenSettings;

        CommandManager.RemoveHandler(ConfigCommand);
        WindowSystem.RemoveAllWindows();

        Controller.Dispose();
    }

    internal void OpenSettings() => ConfigWindow.IsOpen = true;

    private void OnCommand(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "toggle":
                SetEnabled(!Config.Enabled);
                break;
            case "on":
                SetEnabled(true);
                break;
            case "off":
                SetEnabled(false);
                break;
            default:
                OpenSettings();
                break;
        }
    }

    private void SetEnabled(bool enabled)
    {
        Config.Enabled = enabled;
        Config.Save();
        Controller.OnConfigChanged();
        ChatGui.Print($"[Quiet Tips] {(enabled ? "Enabled" : "Disabled")}.");
    }
}
