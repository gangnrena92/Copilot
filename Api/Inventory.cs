using System.Collections.Generic;
using System.Linq;

using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;

using static Copilot.Copilot;

namespace Copilot.Api;

public static class Inventory
{
    private static GameController GameController => Main.GameController;
    private static IngameState IngameState => GameController.IngameState;
    private static IngameUIElements IngameUi => IngameState.IngameUi;

    public static InventoryElement InventoryPanel => IngameUi.InventoryPanel;

    public static IList<Element> Items => InventoryPanel.GetChildAtIndex(3).GetChildAtIndex(33).Children.ToList().Skip(3).ToList();
}