using System.Linq;
using System.Windows.Forms;

using ExileCore2;
using ExileCore2.Shared;

using static Copilot.Copilot;
using Copilot.Utils;
using Copilot.Settings;
using Copilot.Settings.Tasks;
using Copilot.Api;

namespace Copilot.CoRoutines;

internal class DumperCoRoutine
{
    private static TasksSettings Settings => Main.Settings.Tasks;
    private static GuildStashDumperSettings DumperSettings => Settings.Dumper;

    private static LoggerPlus Log = new LoggerPlus("DumperCoRoutine");

    public static void Init()
    {
        if (Settings.IsDumperEnabled)
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
            await SyncInput.Delay(DumperSettings.Cooldown);
            if (!State.IsHideout || Inventory.Items.Count == 0) continue;

            var stash = Ui.IngameUi.ItemsOnGroundLabelsVisible
                .Where(e => e.ItemOnGround.Metadata.ToLower().Contains("guildstash"))
                .FirstOrDefault();
            if (stash == null) continue;

            await SyncInput.LClick(stash.ItemOnGround.Pos, 1000);

            if (!Stash.GuildTabs.IsVisible) continue;

            var tab = Stash.GetGuildTab(DumperSettings.SelectedTab);
            if (tab == null) continue;

            await SyncInput.LClick(tab.GetClientRectCache.Center, 1000);

            Input.KeyDown(Keys.ControlKey);
            await SyncInput.Delay(100);
            foreach (var item in Inventory.Items)
            {
                await SyncInput.LClick(item.GetClientRect().Center, 20);
                await SyncInput.Delay(DumperSettings.ClickDelay);
            }
            Input.KeyUp(Keys.ControlKey);
        }
    }
}