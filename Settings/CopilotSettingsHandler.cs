using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using ImGuiNET;

using ExileCore2.Shared.Nodes;

using Copilot.Utils;
using Copilot.Api;

namespace Copilot.Settings;
public class CopilotSettingsHandler
{
    private static CopilotSettings Settings => Copilot.Main.Settings;

    private static LoggerPlus Log = new LoggerPlus("CopilotSettingsHandler");

    public static void DrawSettings()
    {
    DrawPartyList();
    DrawGuildStashDropdown();
    DrawMovementSettings(); // НОВОЕ
    DrawCustomSettings();
    }

    public static void DrawPartyList()
    {
        try
        {
            Settings.TargetPlayerName.SetListValues(Ui.GetPartyList());
        } catch (Exception ex) {
            Log.Error("Error drawing party list: " + ex.Message);
        }
    }

    public static void DrawGuildStashDropdown()
    {
        try
        {
            Settings.Tasks.Dumper.SelectedTab.SetListValues(Ui.GetGuildStashList());
        } catch (Exception ex) {
            Log.Error("Error drawing guild stash dropdown: " + ex.Message);
        }
    }
    public static void DrawMovementSettings()
{
    if (!ImGui.TreeNode("Movement Settings")) return;
    
    // Обновляем список доступных методов перемещения
    Settings.Additional.MovementType.SetListValues(new List<string> 
    { 
        "Follow Key", 
        "WASD", 
        "Mouse Click" 
    });
    
    var movementType = Settings.Additional.MovementType.Value;
    if (ImGui.Combo("Movement Type", ref movementType, Settings.Additional.MovementType.ValuesArray, Settings.Additional.MovementType.ValuesArray.Length))
    {
        Settings.Additional.MovementType.Value = movementType;
    }
    
    if (movementType == "WASD")
    {
        ImGui.Text("WASD Configuration:");
        Settings.Additional.WKey.Value = DrawHotkey("W Key", Settings.Additional.WKey.Value);
        Settings.Additional.AKey.Value = DrawHotkey("A Key", Settings.Additional.AKey.Value);
        Settings.Additional.SKey.Value = DrawHotkey("S Key", Settings.Additional.SKey.Value);
        Settings.Additional.DKey.Value = DrawHotkey("D Key", Settings.Additional.DKey.Value);
    }
    else if (movementType == "Follow Key")
    {
        Settings.Additional.FollowKey.Value = DrawHotkey("Follow Key", Settings.Additional.FollowKey.Value);
    }
    
    ImGui.TreePop();
}

private static Keys DrawHotkey(string label, Keys currentKey)
{
    var key = (int)currentKey;
    if (ImGui.Combo(label, ref key, Enum.GetNames(typeof(Keys)), Enum.GetValues(typeof(Keys)).Length))
    {
        return (Keys)key;
    }
    return currentKey;
}
    public static void DrawCustomSettings()
    {
        if (!ImGui.TreeNode("Custom Tasks")) return;
        if (ImGui.Button("Add Custom Task"))
        {
            // random number
            var random = new Random();
            var randomNumber = random.Next(1, 1000);

            var customSettings = new CustomSettings(taskName: $"Task - {randomNumber}", codeSnippet: "var str = \"Hello, World!\";\nreturn str;");
            Settings.CustomSettingsList.Add(customSettings);
            Copilot.Main.UpdateCustomCoRoutines();
        }

        ImGui.Separator();

        foreach (var (customSettings, idx) in Settings.CustomSettingsList.Select((x, i) => (x, i)))
        {
            if (!ImGui.TreeNode($"Custom Task {idx}##{idx}")) continue;

            var isEnabled = customSettings.IsEnabled;
            if (ImGui.Checkbox($"Enabled##{idx}", ref isEnabled))
            {
                customSettings.IsEnabled = isEnabled;
            }

            var taskName = customSettings.TaskName;
            if (ImGui.InputText($"Task Name##{idx}", ref taskName, 100))
            {
                customSettings.TaskName = taskName;
            }

            ImGui.SameLine();
            if (ImGui.Button($"Delete##{idx}"))
            {
                Log.Message($"Deleted custom task: {taskName}");
                Settings.CustomSettingsList.Remove(customSettings);
                Copilot.Main.UpdateCustomCoRoutines();
                break; // exit the loop after removing the task
            }

            var actionCooldown = customSettings.ActionCooldown;
            if(ImGui.SliderInt($"Action Cooldown##{idx}", ref actionCooldown, 1, 2000))
            {
                customSettings.ActionCooldown = actionCooldown;
            }

            var codeSnippet = customSettings.CodeSnippet;
            var windowWidth = ImGui.GetWindowWidth();
            var windowHeight = ImGui.GetWindowHeight();
            if (ImGui.InputTextMultiline($"Code Snippet##{idx}", ref codeSnippet, 1000, new System.Numerics.Vector2(windowWidth - 200, windowHeight / 3)))
            {
                customSettings.CodeSnippet = codeSnippet;
            }

            ImGui.TreePop();
        }
    }
}
