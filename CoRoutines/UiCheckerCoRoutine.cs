using System.Windows.Forms;

using ExileCore2.Shared;

using static Copilot.Copilot;
using static Copilot.Api.Ui;
using Copilot.Settings.Tasks;
using Copilot.Api;
using Copilot.Utils;
using Copilot.Settings;

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
            await SyncInput.Delay(UiCheckerSettings.Cooldown);

            if (State.IsHideout && Inventory.Items.Count != 0) continue;

            // Check if has resurrect UI open
            if (UiCheckerSettings.AutoRespawn && ResurrectPanel != null && ResurrectPanel.IsVisible) {
                DontFollow = true;
                var btn = ResurrectPanel?.ResurrectAtCheckpoint ?? ResurrectPanel?.ResurrectInTown; // if inTown is null, use atCheckpoint
                if (GetTpConfirmation() != null) await SyncInput.PressKey(Keys.Escape);
                if (btn != null && btn.IsVisible) {
                    Main.RessurectedRecently = true;
                    await SyncInput.LClick(btn.GetClientRectCache.Center);
                }
                _player = null;
                _target = null;
                DontFollow = false;
                await SyncInput.Delay(300);
            }

            if (IsAnyUiOpen())
                await SyncInput.PressKey(Keys.Space);
        }
    }
}