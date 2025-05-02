using System.Threading.Tasks;
using System.Windows.Forms;

using ExileCore2.Shared;

using static Copilot.Copilot;
using Copilot.Utils;
using Copilot.Settings;
using Copilot.Settings.Tasks;

namespace Copilot.CoRoutines;

internal class BlinkCoRoutine
{
    private static TasksSettings Settings => Main.Settings.Tasks;
    private static BlinkSettings BlinkSettings => Settings.Blink;

    private static LoggerPlus Log = new LoggerPlus("BlinkCoRoutine");

    public static void Init()
    {
        if (Settings.IsBlinkEnabled)
        {
            TaskRunner.Run(Blink_Task, "Blink");
        }
    }

    public static void Stop()
    {
        TaskRunner.Stop("Blink");
    }

    public static async SyncTask<bool> Blink_Task()
    {
        while (true)
        {
            await SyncInput.Delay(BlinkSettings.Cooldown);
            if (!Main.AllowBlinkTask || _player.DistanceTo(_target) < BlinkSettings.Range) continue;
            Log.Message("Blinking...");
            Main.AllowBlinkTask = false;
            await SyncInput.PressKey(Keys.Space);
        }
    }
}