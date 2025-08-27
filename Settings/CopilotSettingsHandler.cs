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
        DrawMovementSettings();
        DrawCustomSettings();
    }

    public static void DrawPartyList()
    {
        try
        {
            Settings.TargetPlayerName.SetListValues(Ui.GetPartyList());
        }
        catch (Exception ex)
        {
            Log.Error("Error drawing party list: " + ex.Message);
        }
    }

    public static void DrawGuildStashDropdown()
    {
        try
        {
            Settings.Tasks.Dumper.SelectedTab.SetListValues(Ui.GetGuildStashList());
        }
        catch (Exception ex)
        {
            Log.Error("Error drawing guild stash dropdown: " + ex.Message);
        }
    }

    public static void DrawMovementSettings()
    {
        if (!ImGui.TreeNode("Movement Settings")) return;

        try
        {
            // Гарантируем что список значений существует
            if (Settings.Additional.MovementType.Values == null || Settings.Additional.MovementType.Values.Count == 0)
            {
                Settings.Additional.MovementType.Values = new List<string> { "Follow Key", "WASD", "Mouse Click" };
                if (string.IsNullOrEmpty(Settings.Additional.MovementType.Value))
                    Settings.Additional.MovementType.Value = "Follow Key";
            }

            // Получаем текущий индекс выбранного значения
            var currentIndex = Settings.Additional.MovementType.Values
                .IndexOf(Settings.Additional.MovementType.Value);
            if (currentIndex == -1) currentIndex = 0;

            // Создаем массив для ImGui.Combo
            var valuesArray = Settings.Additional.MovementType.Values.ToArray();

            if (ImGui.Combo("Movement Type", ref currentIndex, valuesArray, valuesArray.Length))
            {
                Settings.Additional.MovementType.Value = Settings.Additional.MovementType.Values[currentIndex];
            }

            // Отображаем соответствующие настройки для выбранного типа движения
            switch (Settings.Additional.MovementType.Value)
            {
                case "WASD":
                    DrawWASDSettings();
                    break;
                case "Follow Key":
                    DrawFollowKeySettings();
                    break;
                case "Mouse Click":
                    ImGui.Text("Mouse movement - click on target to move");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error drawing movement settings: " + ex.Message);
        }

        ImGui.TreePop();
    }

    private static void DrawWASDSettings()
    {
        ImGui.Text("WASD Configuration:");
        
        Settings.Additional.WKey.Value = DrawHotkey("W Key", Settings.Additional.WKey.Value);
        Settings.Additional.AKey.Value = DrawHotkey("A Key", Settings.Additional.AKey.Value);
        Settings.Additional.SKey.Value = DrawHotkey("S Key", Settings.Additional.SKey.Value);
        Settings.Additional.DKey.Value = DrawHotkey("D Key", Settings.Additional.DKey.Value);
    }

    private static void DrawFollowKeySettings()
    {
        Settings.Additional.FollowKey.Value = DrawHotkey("Follow Key", Settings.Additional.FollowKey.Value);
    }

    private static Keys DrawHotkey(string label, Keys currentKey)
    {
        var keyNames = Enum.GetNames(typeof(Keys));
        var keyValues = Enum.GetValues(typeof(Keys));
        
        var currentIndex = Array.IndexOf(keyValues, currentKey);
        if (currentIndex == -1) currentIndex = 0;

        if (ImGui.Combo(label, ref currentIndex, keyNames, keyNames.Length))
        {
            return (Keys)keyValues.GetValue(currentIndex);
        }
        return currentKey;
    }

    public static void DrawCustomSettings()
    {
        if (!ImGui.TreeNode("Custom Tasks")) return;
        
        if (ImGui.Button("Add Custom Task"))
        {
            var random = new Random();
            var randomNumber = random.Next(1, 1000);

            var customSettings = new CustomSettings(
                taskName: $"Task - {randomNumber}", 
                codeSnippet: "var str = \"Hello, World!\";\nreturn str;"
            );
            
            Settings.CustomSettingsList.Add(customSettings);
            Copilot.Main.UpdateCustomCoRoutines();
        }

        ImGui.Separator();

        for (int idx = 0; idx < Settings.CustomSettingsList.Count; idx++)
        {
            var customSettings = Settings.CustomSettingsList[idx];
            
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
                Settings.CustomSettingsList.RemoveAt(idx);
                Copilot.Main.UpdateCustomCoRoutines();
                ImGui.TreePop();
                continue;
            }

            var actionCooldown = customSettings.ActionCooldown;
            if (ImGui.SliderInt($"Action Cooldown##{idx}", ref actionCooldown, 1, 2000))
            {
                customSettings.ActionCooldown = actionCooldown;
            }

            var codeSnippet = customSettings.CodeSnippet;
            var windowWidth = ImGui.GetWindowWidth();
            var windowHeight = ImGui.GetWindowHeight();
            
            if (ImGui.InputTextMultiline(
                $"Code Snippet##{idx}", 
                ref codeSnippet, 
                1000, 
                new System.Numerics.Vector2(windowWidth - 200, windowHeight / 3)))
            {
                customSettings.CodeSnippet = codeSnippet;
            }

            ImGui.TreePop();
        }
        
        ImGui.TreePop();
    }
}