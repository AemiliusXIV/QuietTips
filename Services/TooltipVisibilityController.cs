using System;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;

namespace QuietTips.Services;

/// <summary>
/// Drives tooltip visibility by flipping FFXIV's own UI-config booleans in response
/// to combat, duty and menu state. Recomputes the full desired state on every event
/// rather than toggling deltas, so it can't get stuck out of sync.
/// </summary>
public sealed class TooltipVisibilityController : IDisposable
{
    private readonly Configuration config;

    // Game-config flag names. The first three live under UiControl; the crossbar
    // hint flag lives under UiConfig.
    private const string ActionFlag = "ActionDetailDisp";
    private const string ItemFlag = "ItemDetailDisp";
    private const string PopUpFlag = "ToolTipDisp";
    private const string CrossbarFlag = "HotbarCrossHelpDisp";

    // Inventory-type windows. While any of these is open and the exception is on,
    // every tooltip is forced back so gear can be inspected.
    private static readonly string[] InventoryAddons =
    {
        "Inventory", "InventoryLarge", "InventoryExpansion", "InventoryGrid",
        "InventoryBuddy", "InventoryRetainer", "InventoryRetainerLarge",
        "ArmouryBoard", "Character", "CharacterInspect", "Cabinet", "CabinetWithdraw",
    };

    private bool originalsCaptured;
    private bool originalAction, originalItem, originalPopUp, originalCrossbar;

    // Set when LoggingOut fires, cleared on the next login. IsLoggedIn stays true
    // for a while during the logout sequence, so without this the per-frame
    // Recompute would re-apply the hide-values right after RestoreOriginals and
    // the game would save them as the character's own settings.
    private bool loggingOut;

    public TooltipVisibilityController(Configuration config)
    {
        this.config = config;

        Plugin.Condition.ConditionChange += OnConditionChange;
        Plugin.Framework.Update += OnFrameworkUpdate;
        Plugin.ClientState.Login += OnLogin;

        if (Plugin.ClientState.IsLoggedIn)
        {
            CaptureOriginals();
            Recompute();
        }
    }

    public void Dispose()
    {
        Plugin.Condition.ConditionChange -= OnConditionChange;
        Plugin.Framework.Update -= OnFrameworkUpdate;
        Plugin.ClientState.Login -= OnLogin;

        // Leave the game the way we found it.
        RestoreOriginals();
    }

    /// <summary>
    /// Re-reads state and re-applies. Safe to call from the UI thread: the actual
    /// game-config writes are marshalled onto the framework thread.
    /// </summary>
    public void OnConfigChanged() => Plugin.Framework.RunOnFrameworkThread(Recompute);

    /// <summary>
    /// True when an inventory-type window is forcing tooltips back on.
    /// Queried live from the game rather than tracked via lifecycle events, so it
    /// can't get stuck if an addon closes without firing PreFinalize.
    /// </summary>
    public bool MenuExceptionActive
    {
        get
        {
            if (!config.ShowInInventoryMenus) return false;
            foreach (var name in InventoryAddons)
                if (Plugin.GameGui.GetAddonByName(name) != nint.Zero)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// Whether a tooltip with these in/out-of-combat hide settings is hidden under
    /// the current live context. Used by the config window's status line.
    /// </summary>
    public bool WouldHide(bool hideInCombat, bool hideOoc) => ShouldHide(
        hideInCombat, hideOoc,
        Plugin.Condition[ConditionFlag.InCombat],
        Plugin.Condition[ConditionFlag.BoundByDuty],
        MenuExceptionActive);

    private void OnLogin()
    {
        loggingOut = false;
        CaptureOriginals();
        Recompute();
    }

    private void OnFrameworkUpdate(IFramework _) => Recompute();

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        if (flag == ConditionFlag.LoggingOut && value)
        {
            loggingOut = true;
            RestoreOriginals();
            originalsCaptured = false;
        }
    }

    private void CaptureOriginals()
    {
        if (originalsCaptured) return;
        if (!Plugin.GameConfig.UiControl.TryGetBool(ActionFlag, out originalAction)) return;
        if (!Plugin.GameConfig.UiControl.TryGetBool(ItemFlag, out originalItem)) return;
        if (!Plugin.GameConfig.UiControl.TryGetBool(PopUpFlag, out originalPopUp)) return;
        if (!Plugin.GameConfig.UiConfig.TryGetBool(CrossbarFlag, out originalCrossbar)) return;
        originalsCaptured = true;
    }

    private void RestoreOriginals()
    {
        if (!originalsCaptured) return;
        SetControl(ActionFlag, originalAction);
        SetControl(ItemFlag, originalItem);
        SetControl(PopUpFlag, originalPopUp);
        SetConfig(CrossbarFlag, originalCrossbar);
    }

    private void Recompute()
    {
        if (loggingOut) return;
        if (!Plugin.ClientState.IsLoggedIn) return;

        if (!config.Enabled)
        {
            RestoreOriginals();
            return;
        }

        var inCombat = Plugin.Condition[ConditionFlag.InCombat];
        var inDuty = Plugin.Condition[ConditionFlag.BoundByDuty];
        var menuOpen = MenuExceptionActive;

        SetControl(ActionFlag, !ShouldHide(config.HideActionInCombat, config.HideActionOoc, inCombat, inDuty, menuOpen));
        SetControl(ItemFlag, !ShouldHide(config.HideItemInCombat, config.HideItemOoc, inCombat, inDuty, menuOpen));
        SetControl(PopUpFlag, !ShouldHide(config.HidePopUpInCombat, config.HidePopUpOoc, inCombat, inDuty, menuOpen));
        SetConfig(CrossbarFlag, !ShouldHide(config.HideCrossbarInCombat, config.HideCrossbarOoc, inCombat, inDuty, menuOpen));
    }

    /// <summary>
    /// Decision order: an open inventory menu always wins (show), then the duty
    /// override if it isn't "follow combat", then the in/out-of-combat toggles.
    /// </summary>
    private bool ShouldHide(bool hideInCombat, bool hideOoc, bool inCombat, bool inDuty, bool menuOpen)
    {
        if (menuOpen) return false;

        if (inDuty)
        {
            switch (config.DutyBehaviour)
            {
                case DutyOverride.AlwaysShow: return false;
                case DutyOverride.AlwaysHide: return true;
            }
        }

        return inCombat ? hideInCombat : hideOoc;
    }

    private static void SetControl(string name, bool value)
    {
        if (Plugin.GameConfig.UiControl.TryGetBool(name, out var current) && current != value)
            Plugin.GameConfig.UiControl.Set(name, value);
    }

    private static void SetConfig(string name, bool value)
    {
        if (Plugin.GameConfig.UiConfig.TryGetBool(name, out var current) && current != value)
            Plugin.GameConfig.UiConfig.Set(name, value);
    }
}
