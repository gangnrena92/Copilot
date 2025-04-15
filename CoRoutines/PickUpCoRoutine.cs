using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using ExileCore2.Shared;

using static Copilot.Copilot;
using static Copilot.Utils.Ui;
using Copilot.Utils;
using Copilot.Settings;
using Copilot.Settings.Tasks;

namespace Copilot.CoRoutines;
internal class PickUpCoRoutine
{
    private static TasksSettings Settings => Main.Settings.Tasks;
    private static PickupSettings PickupSettings => Settings.Pickup;
    private static LoggerPlus Log = new LoggerPlus("PickUpCoRoutine");

    public static void Init()
    {
        if (Settings.IsPickupEnabled.Value)
        {
            TaskRunner.Run(PickUp_Task, "PickUp");
        }
    }

    public static void Stop()
    {
        TaskRunner.Stop("PickUp");
    }

    public static async SyncTask<bool> PickUp_Task()
    {
        while (true)
        {
            await Task.Delay(PickupSettings.Delay.Value);
            if (!(Main.DistanceToTarget <= PickupSettings.RangeToIgnore.Value)) continue;

            var pos = PickupSettings.UseTargetPosition.Value ? Main._followTarget.Pos : Main.PlayerPos;
            var items = IngameUi.ItemsOnGroundLabelsVisible;
            if (items == null) continue;
            var filteredItems = PickupSettings.Filter.Value.Split(',');
            var item = items?
                .OrderBy(x => Vector3.Distance(pos, x.ItemOnGround.Pos))
                .FirstOrDefault(x => filteredItems.Any(y => x.Label.Text != null && x.Label.Text.Contains(y)));
            if (item == null) continue;

            var distanceToItem = Vector3.Distance(pos, item.ItemOnGround.Pos);
            if (!(distanceToItem <= PickupSettings.Range.Value)) continue;

            Log.Message("Picking up item: " + item.Label.Text);
            await SyncInput.LClick(Camera.WorldToScreen(item.ItemOnGround.Pos));
        }
    }
}