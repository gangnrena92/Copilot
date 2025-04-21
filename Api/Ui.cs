using System.Collections.Generic;
using System.Linq;

using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;

using static Copilot.Copilot;

namespace Copilot.Api;

public static class Ui
{
    private static GameController GameController => Main.GameController;

    public static IngameState IngameState => GameController.IngameState;

    public static IngameUIElements IngameUi => IngameState.IngameUi;

    public static Element UIRoot => IngameState.UIRoot;

    public static Camera Camera => IngameState.Camera;

    public static ResurrectPanel ResurrectPanel => IngameUi.ResurrectPanel;

    public static Element GetTpConfirmation()
    {
        try
        {
            var ui = IngameUi.PopUpWindow.Children[0].Children[0];
            return ui.Children[0].Text.StartsWith("Are you sure you want to teleport")
                ? ui.Children[3].Children[0]
                : null;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsAnyUiOpen()
    {
        var checkpoint = UIRoot.Children?[1]?.Children?[64];
        var market = UIRoot.Children?[1]?.Children?[27];
        var leftPanel = IngameUi.OpenLeftPanel;
        var rightPanel = IngameUi.OpenRightPanel;
        var worldMap = IngameUi.WorldMap;
        var npcDialog = IngameUi.NpcDialog;

        return (checkpoint?.IsVisible != null && (bool)checkpoint?.IsVisible) ||
                (leftPanel?.IsVisible != null && (bool)leftPanel?.IsVisible) ||
                (rightPanel?.IsVisible != null && (bool)rightPanel?.IsVisible) ||
                (worldMap?.IsVisible != null && (bool)worldMap?.IsVisible) ||
                (npcDialog?.IsVisible != null && (bool)npcDialog?.IsVisible) ||
                (market?.IsVisible != null && (bool)market?.IsVisible);
    }

    public static List<string> GetPartyList()
    {
        var list = IngameUi.PartyElement.Children?[0]?.Children;
        return list?.Select(x => x?.Children?[0]?.Children?[0]?.Text).ToList() ?? new List<string>();
    }

    public static List<string> GetGuildStashList()
    {
        var list = Stash.GuildInventories;
        return list != null && list.Count > 0 ? list.Select(stash => stash.TabName).ToList() : new List<string>();
    }
}