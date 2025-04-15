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
internal class CurseBotCoRoutine
{
    private static TasksSettings Settings => Main.Settings.Tasks;
    private static CurseBotSettings CurseBotSettings => Settings.CurseBot;

    private static LoggerPlus Log = new LoggerPlus("CurseBotCoRoutine");

    public static void Init()
    {
        if (Settings.IsCurseBotEnabled.Value)
        {
            TaskRunner.Run(CurseBot_Task, "CurseBotCoRoutine");
        }
    }

    public static void Stop()
    {
        TaskRunner.Stop("CurseBotCoRoutine");
    }

    public static async SyncTask<bool> CurseBot_Task()
    {
        while (true)
        {
            Log.Message("CurseBot Task running...");
            await Task.Delay(CurseBotSettings.ActionCooldown.Value);
            if (Main.DistanceToTarget > CurseBotSettings.IgnoreRange) continue;

            var monster = Ui.EntityList
                .Where(e => e.Type == EntityType.Monster && !e.IsAlive)
                .OrderBy(e => Vector3.Distance(Main.PlayerPos, e.Pos))
                .FirstOrDefault();

            if (monster == null) continue;

            var distanceToMonster = Vector3.Distance(Main.PlayerPos, monster.Pos);
            if (distanceToMonster > CurseBotSettings.Range) continue;

            await SyncInput.MoveMouse(Ui.Camera.WorldToScreen(monster.Pos), 100);
            await SyncInput.PressKey(CurseBotSettings.CurseKey.Value);
        }
    }
}