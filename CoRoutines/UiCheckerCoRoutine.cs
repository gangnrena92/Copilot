using System.Threading.Tasks;
using System.Windows.Forms;
using Copilot.Settings;
using ExileCore2.Shared;

using static Copilot.Copilot;
using static Copilot.Api.Ui;
using Copilot.Settings.Tasks;
using Copilot.Api;
using Copilot.Utils;

namespace Copilot.CoRoutines;

internal class UiCheckerCoRoutine
{
    private static TasksSettings Settings => Main.Settings.Tasks;
    private static UiCheckerSettings UiCheckerSettings => Settings.UiChecker;

    public static void Init()
    {
        if (Settings.IsUiCheckerEnabled)
        {
            TaskRunner.Run(UiChecker_Task, "UiCheckerCoRoutine");
        }
    }

    public static void Stop()
    {
        TaskRunner.Stop("UiCheckerCoRoutine");
    }

    public static async SyncTask<bool> UiChecker_Task()
    {
        while (true)
        {
            await Task.Delay(UiCheckerSettings.Cooldown);

            if (State.IsHideout && Inventory.Items.Count != 0) continue;

            // Check if has resurrect UI open
            var resurrectPanel = IngameUi.ResurrectPanel;
            if (UiCheckerSettings.AutoRespawn && resurrectPanel != null && resurrectPanel.IsVisible) {
                var btn = resurrectPanel?.ResurrectAtCheckpoint ?? resurrectPanel?.ResurrectInTown; // if inTown is null, use atCheckpoint
                if (btn != null && btn.IsVisible) {
                    await SyncInput.LClick(btn.GetClientRectCache.Center, 10);
                }
            }

            if (IsAnyUiOpen())
                await SyncInput.PressKey(Keys.Space);
        }
    }
}