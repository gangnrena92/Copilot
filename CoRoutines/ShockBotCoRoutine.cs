using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using ExileCore2.Shared;
using ExileCore2.Shared.Enums;

using static Copilot.Copilot;
using static Copilot.Api.Ui;
using Copilot.Utils;
using Copilot.Api;
using Copilot.Settings;
using Copilot.Settings.Tasks;

namespace Copilot.CoRoutines;
internal class ShockBotCoRoutine
{
    private static TasksSettings Settings => Main.Settings.Tasks;
    private static ShockBotSettings ShockBotSettings => Settings.ShockBot;

    private static LoggerPlus Log = new LoggerPlus("ShockBotCoRoutine");

    public static void Init()
    {
        if (Settings.IsShockBotEnabled)
        {
            TaskRunner.Run(ShockBot_Task, "ShockBotCoRoutine");
        }
    }

    public static void Stop()
    {
        TaskRunner.Stop("ShockBotCoRoutine");
    }

    public static async SyncTask<bool> ShockBot_Task()
    {
        while (true)
        {
            await Task.Delay(ShockBotSettings.ActionCooldown);

            var monster = Entities
                .NearbyMonsters(EntityRarity.AtLeastRare, additionalFilters: e => e.IsAlive)
                .FirstOrDefault();
            if (monster == null) continue;

            if (_player.DistanceTo(monster) > ShockBotSettings.Range) continue;

            await SyncInput.MoveMouse(monster, 100);
            await SyncInput.PressKey(ShockBotSettings.BallLightningKey);

            // track the balls
            var ball = Entities.ValidList
                .Where(e => e.IsDead && e.Metadata == "Metadata/Projectiles/BallLightningPlayer")
                .OrderBy(e => Vector3.Distance(monster.Pos, e.Pos))
                .FirstOrDefault();

            if (ball != null && Vector3.Distance(monster.Pos, ball.Pos) <= ShockBotSettings.RangeToUseLightningWarp)
            {
                await SyncInput.MoveMouse(ball, 50);
                await SyncInput.PressKey(ShockBotSettings.LightningWarpKey);
            }
        }
    }
}