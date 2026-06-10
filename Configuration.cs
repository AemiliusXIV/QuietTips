using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace QuietTips;

/// <summary>
/// What the duty axis does to tooltips while bound by an instance. When set to
/// anything other than <see cref="FollowCombat"/> it overrides the combat toggles
/// (but never the inventory-menu exception, which always wins).
/// </summary>
public enum DutyOverride
{
    FollowCombat = 0,
    AlwaysShow = 1,
    AlwaysHide = 2,
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool Enabled { get; set; } = true;

    // Per tooltip type: hide while in combat / hide while out of combat.
    // Defaults hide action + item tooltips out of combat (the common "stop the
    // clutter when I'm just clicking around town" case) and leave them during
    // combat where the info is more likely wanted.
    public bool HideActionInCombat { get; set; } = false;
    public bool HideActionOoc { get; set; } = true;

    public bool HideItemInCombat { get; set; } = false;
    public bool HideItemOoc { get; set; } = true;

    public bool HidePopUpInCombat { get; set; } = false;
    public bool HidePopUpOoc { get; set; } = false;

    public bool HideCrossbarInCombat { get; set; } = false;
    public bool HideCrossbarOoc { get; set; } = false;

    /// <summary>
    /// Overrides the combat toggles while bound by duty. Default keeps the combat
    /// rules in effect inside instances too.
    /// </summary>
    public DutyOverride DutyBehaviour { get; set; } = DutyOverride.FollowCombat;

    /// <summary>
    /// When true, any tooltip hidden by the rules above is forced back on while an
    /// inventory / armoury / character window is open, so gear can be inspected.
    /// </summary>
    public bool ShowInInventoryMenus { get; set; } = true;

    // Toggle feedback
    public bool ShowToastOnToggle { get; set; } = true;
    public bool ShowStatusBarIcon { get; set; } = false;

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pi) => pluginInterface = pi;

    public void Save() => pluginInterface?.SavePluginConfig(this);
}
