using System;
using System.Linq;
using System.Collections.Generic;

using Copilot.Utils;

namespace Copilot.Settings;
public class CopilotSettingsHandler
{
    private static CopilotSettings Settings => Copilot.Main.Settings;

    private static LoggerPlus Log = new LoggerPlus("CopilotSettingsHandler");

    public static void DrawCustomSettings()
    {
        DrawPartyList();
        DrawGuildStashDropdown();
    }

    public static void DrawPartyList()
    {
        try
        {
            Settings.TargetPlayerName.SetListValues(GetPartyList());
        } catch (Exception ex) {
            Log.Error("Error drawing party list: " + ex.Message);
        }
    }

    public static void DrawGuildStashDropdown()
    {
        try
        {
            Settings.Tasks.Dumper.SelectedTab.SetListValues(GetGuildStashList());
        } catch (Exception ex) {
            Log.Error("Error drawing guild stash dropdown: " + ex.Message);
        }
    }

    private static List<string> GetPartyList()
    {
        var list = Ui.IngameUi.PartyElement.Children?[0]?.Children;
        return list?.Select(x => x?.Children?[0]?.Children?[0]?.Text).ToList() ?? new List<string>();
    }

    private static List<string> GetGuildStashList()
    {
        var list = Ui.GuildStashInventory;
        return list != null && list.Count > 0 ? list.Select(stash => stash.TabName).ToList() : new List<string>();
    }
}
