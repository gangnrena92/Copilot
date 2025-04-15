using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;

using ExileCore2;
using ExileCore2.Shared;

using static Copilot.Copilot;
using static Copilot.Utils.Ui;
using Copilot.Utils;
using Copilot.Settings;
using Copilot.Settings.Tasks;

namespace Copilot.CoRoutines;
internal class DumperCoRoutine
{
    private static TasksSettings Settings => Main.Settings.Tasks;
    private static GuildStashDumperSettings DumperSettings => Settings.Dumper;

    private static LoggerPlus Log = new LoggerPlus("DumperCoRoutine");

    public static void Init()
    {
        if (Settings.IsDumperEnabled.Value)
        {
            TaskRunner.Run(Dumper_Task, "DumperCoRoutine");
        }
    }

    public static void Stop()
    {
        TaskRunner.Stop("DumperCoRoutine");
    }

    public static async SyncTask<bool> Dumper_Task()
    {
        while (true)
        {
            Log.Message("Dumper Task running...");
            await Task.Delay(DumperSettings.Cooldown.Value);
            Log.Message("Dumper after delay...");
            if (!CurrentArea.IsHideout || InventoryList.Count == 0) continue;

            var stash = IngameUi.ItemsOnGroundLabelsVisible
                .Where(e => e.ItemOnGround.Metadata.ToLower().Contains("guildstash"))
                .FirstOrDefault();
            if (stash == null) continue;

            await SyncInput.LClick(Camera.WorldToScreen(stash.ItemOnGround.Pos), 1000);

            if (!AllStashPanel.IsVisible) continue;

            var tab = AllStashPanel.Children.FirstOrDefault(x => x?.GetChildAtIndex(0)?.GetChildAtIndex(1)?.Text == DumperSettings.SelectedTab.Value);
            if (tab == null) continue;

            await SyncInput.LClick(tab.GetClientRectCache.Center, 1000);

            Input.KeyDown(Keys.ControlKey);
            await Task.Delay(100);
            foreach (var item in InventoryList)
            {
                await SyncInput.LClick(item.GetClientRect().Center, 20);
                await Task.Delay(DumperSettings.ClickDelay.Value);
            }
            Input.KeyUp(Keys.ControlKey);
        }
    }
}