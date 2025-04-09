using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;

namespace Copilot.Utils;
public static class Ui
{
    private static GameController GameController => Copilot.Main.GameController;

    public static IngameUIElements IngameUi => GameController.IngameState.IngameUi;
    public static Element UIRoot => GameController.IngameState.UIRoot;
    public static Camera Camera => GameController.Game.IngameState.Camera;
    public static AreaInstance CurrentArea => GameController.Area.CurrentArea;
    public static List<StashTabContainerInventory> GuildStashInventory => IngameUi.GuildStashElement.Inventories;
    public static List<Entity> EntityList => GameController.EntityListWrapper.OnlyValidEntities;
    public static IList<Element> InventoryList => IngameUi.InventoryPanel.GetChildAtIndex(3).GetChildAtIndex(33).Children.ToList().Skip(3).ToList();

    public static Point AdjustPosition(Vector2 position, string offset="TopLeft")
    {
        var windowOffset = GameController.Window.GetWindowRectangle();
        var adjustedOffset = offset switch
        {
            "TopLeft" => windowOffset.TopLeft,
            _ => Vector2.Zero
        };
        var adjustedPos = new Point((int)(position.X + adjustedOffset.X), (int)(position.Y + adjustedOffset.Y));
        return adjustedPos;
    }

    public static Element GetTpConfirmation()
    {
        try
        {
            var ui = IngameUi.PopUpWindow.Children[0].Children[0];

            if (ui.Children[0].Text.Equals("Are you sure you want to teleport to this player's location?"))
                return ui.Children[3].Children[0];

            return null;
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
}