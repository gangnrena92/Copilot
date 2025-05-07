using System.Linq;

using ExileCore2.Shared;

using static Copilot.Copilot;
using static Copilot.Api.Ui;
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
            await SyncInput.Delay(PickupSettings.Delay);
            try
            {
                if (!(_player.DistanceTo(_target.Entity) <= PickupSettings.RangeToIgnore)) continue;

                var entity = PickupSettings.UseTargetPosition ? _target : _player;
                var items = IngameUi.ItemsOnGroundLabelsVisible;
                if (items == null) continue;

                var filteredItems = PickupSettings.Filter.Value.Split(',');
                var item = items?
                    .OrderBy(x => entity.DistanceTo(x.ItemOnGround))
                    .FirstOrDefault(x => filteredItems.Any(y => x.Label.Text != null && x.Label.Text.Contains(y)));
                if (item == null) continue;

                var distanceToItem = entity.DistanceTo(item.ItemOnGround);
                if (!(distanceToItem <= PickupSettings.Range)) continue;

                Log.Message("Picking up item: " + item.Label.Text);
                await SyncInput.LClick(item.ItemOnGround, 10);
            }
            catch
            {
            }
        }
    }
}