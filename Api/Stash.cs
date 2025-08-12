using System.Collections.Generic;
using System.Linq;

using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;

using static Copilot.Copilot;

namespace Copilot.Api;

public static class Stash
{
    private static GameController GameController => Main.GameController;
    private static IngameState IngameState => GameController.IngameState;
    private static IngameUIElements IngameUi => IngameState.IngameUi;

    // Guild stash inventory
    public static List<StashTabContainerInventory> GuildInventories => IngameUi.GuildStashElement.Inventories;

    public static Element GuildTabs => IngameUi.GuildStashElement.ViewAllStashPanel.GetChildAtIndex(2);

    public static Element GetGuildTab(string tab) => GuildTabs.Children.FirstOrDefault(x => x?.GetChildAtIndex(0)?.GetChildAtIndex(1)?.Text == tab);
}