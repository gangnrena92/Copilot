using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using ExileCore2.Shared;
using ExileCore2.Shared.Enums;

using static Copilot.Copilot;
using Copilot.Utils;
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
        if (Settings.IsShockBotEnabled.Value)
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
            await Task.Delay(ShockBotSettings.ActionCooldown.Value);

            var monster = Ui.EntityList
                .Where(e => e.Type == EntityType.Monster && e.IsAlive && (e.Rarity == MonsterRarity.Rare || e.Rarity == MonsterRarity.Unique))
                .OrderBy(e => Vector3.Distance(Main.PlayerPos, e.Pos))
                .FirstOrDefault();

            if (monster == null) continue;

            var distanceToMonster = Vector3.Distance(Main.PlayerPos, monster.Pos);
            if (distanceToMonster > ShockBotSettings.Range) continue;

            await SyncInput.MoveMouse(Ui.Camera.WorldToScreen(monster.Pos), 100);
            await SyncInput.PressKey(ShockBotSettings.BallLightningKey.Value);

            // track the balls
            var ball = Ui.EntityList
                .Where(e => e.IsDead && e.Metadata == "Metadata/Projectiles/BallLightningPlayer")
                .OrderBy(e => Vector3.Distance(monster.Pos, e.Pos))
                .FirstOrDefault();

            if (ball != null && Vector3.Distance(monster.Pos, ball.Pos) <= ShockBotSettings.RangeToUseLightningWarp.Value)
            {
                await SyncInput.MoveMouse(Ui.Camera.WorldToScreen(ball.Pos), 50);
                await SyncInput.PressKey(ShockBotSettings.LightningWarpKey.Value);
            }
        }
    }
}