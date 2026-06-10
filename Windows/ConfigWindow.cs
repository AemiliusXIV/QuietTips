using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;

namespace QuietTips.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private Configuration Config => plugin.Config;

    private static readonly Vector4 HeaderColor = new(0.65f, 0.88f, 1f, 0.95f);
    private static readonly Vector4 HiddenColor = new(1f, 0.78f, 0.45f, 1f);
    private static readonly Vector4 ShownColor = new(0.55f, 0.85f, 0.55f, 1f);

    public ConfigWindow(Plugin plugin) : base("Quiet Tips Settings##qt_cfg")
    {
        this.plugin = plugin;
        Size = new Vector2(460, 520);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var enabled = Config.Enabled;
        if (ImGui.Checkbox("Plugin enabled", ref enabled))
        {
            Config.Enabled = enabled;
            Commit();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("When off, your original game tooltip settings are restored.");

        DrawStatusLine();

        ImGui.BeginDisabled(!Config.Enabled);

        DrawSectionHeader("What to hide");
        ImGui.TextDisabled("Per type, choose when it should be hidden.");
        ImGui.Spacing();

        if (ImGui.BeginTable("##types", 3, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Tooltip");
            ImGui.TableSetupColumn("In combat");
            ImGui.TableSetupColumn("Out of combat");
            ImGui.TableHeadersRow();

            DrawTypeRow("Action tooltips",
                () => Config.HideActionInCombat, v => Config.HideActionInCombat = v,
                () => Config.HideActionOoc, v => Config.HideActionOoc = v);
            DrawTypeRow("Item tooltips",
                () => Config.HideItemInCombat, v => Config.HideItemInCombat = v,
                () => Config.HideItemOoc, v => Config.HideItemOoc = v);
            DrawTypeRow("Pop-up help",
                () => Config.HidePopUpInCombat, v => Config.HidePopUpInCombat = v,
                () => Config.HidePopUpOoc, v => Config.HidePopUpOoc = v);
            DrawTypeRow("Crossbar hints",
                () => Config.HideCrossbarInCombat, v => Config.HideCrossbarInCombat = v,
                () => Config.HideCrossbarOoc, v => Config.HideCrossbarOoc = v);

            ImGui.EndTable();
        }

        DrawSectionHeader("While in a duty");
        var dutyIdx = (int)Config.DutyBehaviour;
        ImGui.SetNextItemWidth(220);
        if (ImGui.Combo("##duty", ref dutyIdx, "Follow combat rules\0Always show in duty\0Always hide in duty\0\0"))
        {
            Config.DutyBehaviour = (DutyOverride)dutyIdx;
            Commit();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(
                "Follow combat rules: inside instances the in/out-of-combat toggles still apply.\n" +
                "Always show / Always hide: overrides those toggles while you're bound by a duty.");

        DrawSectionHeader("Exception");
        var showInMenus = Config.ShowInInventoryMenus;
        if (ImGui.Checkbox("Show tooltips while an inventory or character window is open", ref showInMenus))
        {
            Config.ShowInInventoryMenus = showInMenus;
            Commit();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(
                "Brings hidden tooltips back whenever Inventory, Armoury Chest, the Character\n" +
                "window and similar are open, so you can still inspect gear. This always wins\n" +
                "over the rules above.");

        ImGui.EndDisabled();

        DrawSectionHeader("Toggle feedback");
        var showToast = Config.ShowToastOnToggle;
        if (ImGui.Checkbox("Show toast notification on toggle", ref showToast))
        {
            Config.ShowToastOnToggle = showToast;
            Commit();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(
                "Shows the in-game splash notification when the plugin is toggled on or off via command.\n" +
                "When off, a chat message is shown instead.");

        var showBar = Config.ShowStatusBarIcon;
        if (ImGui.Checkbox("Show icon in server info bar", ref showBar))
        {
            Config.ShowStatusBarIcon = showBar;
            Commit();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(
                "Adds a small entry to the server info bar at the top of the screen while the plugin\n" +
                "is active. Click it to toggle the plugin off.");
    }

    private void DrawTypeRow(
        string label,
        Func<bool> getCombat, Action<bool> setCombat,
        Func<bool> getOoc, Action<bool> setOoc)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(label);

        ImGui.TableNextColumn();
        var inCombat = getCombat();
        if (ImGui.Checkbox($"##{label}_ic", ref inCombat)) { setCombat(inCombat); Commit(); }

        ImGui.TableNextColumn();
        var ooc = getOoc();
        if (ImGui.Checkbox($"##{label}_ooc", ref ooc)) { setOoc(ooc); Commit(); }
    }

    private void DrawStatusLine()
    {
        ImGui.Spacing();
        if (!Config.Enabled)
        {
            ImGui.TextDisabled("Off. Your original game settings are in effect.");
            return;
        }

        var controller = plugin.Controller;
        var inCombat = Plugin.Condition[ConditionFlag.InCombat];
        var inDuty = Plugin.Condition[ConditionFlag.BoundByDuty];

        var context = inCombat ? "in combat" : "out of combat";
        if (inDuty) context += ", in a duty";
        if (controller.MenuExceptionActive) context += ", inventory open";

        var hidden = new List<string>();
        if (controller.WouldHide(Config.HideActionInCombat, Config.HideActionOoc)) hidden.Add("action");
        if (controller.WouldHide(Config.HideItemInCombat, Config.HideItemOoc)) hidden.Add("item");
        if (controller.WouldHide(Config.HidePopUpInCombat, Config.HidePopUpOoc)) hidden.Add("pop-up");
        if (controller.WouldHide(Config.HideCrossbarInCombat, Config.HideCrossbarOoc)) hidden.Add("crossbar");

        ImGui.TextDisabled($"Right now ({context}):");
        ImGui.SameLine();
        if (controller.MenuExceptionActive)
            ImGui.TextColored(ShownColor, "all shown (menu exception)");
        else if (hidden.Count == 0)
            ImGui.TextColored(ShownColor, "all shown");
        else
            ImGui.TextColored(HiddenColor, $"hiding {string.Join(", ", hidden)}");
    }

    private void Commit()
    {
        Config.Save();
        plugin.Controller.OnConfigChanged();
        Plugin.Framework.RunOnFrameworkThread(plugin.UpdateDtrEntry);
    }

    private static void DrawSectionHeader(string text)
    {
        ImGui.Spacing();
        ImGui.TextColored(HeaderColor, text);
        ImGui.Separator();
    }
}
