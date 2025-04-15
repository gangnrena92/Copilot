using System.Numerics;

using ExileCore2;
using ExileCore2.PoEMemory.MemoryObjects;

using Copilot.Utils;
using Copilot.Settings;
using Copilot.CoRoutines;

// TODO: ghost follow
// - debug to see like a crosshair
// TODO: circle people
// TODO: pots
// TODO: fix AreaTransition
// TODO: better pickup filter make it able to use regex

namespace Copilot;
public class Copilot : BaseSettingsPlugin<CopilotSettings>
{
    public static Copilot Main;
    public LoggerPlus Log => new LoggerPlus("Core");

    public bool AllowBlinkTask = false;

    public Entity _followTarget;
    public Vector3 lastTargetPosition = Vector3.Zero;
    public Vector3 PlayerPos => GameController.Player.Pos;
    public float DistanceToTarget => Vector3.Distance(PlayerPos, _followTarget.Pos);

    public override bool Initialise()
    {
        Main = this;
        Name = "Copilot";
        lastTargetPosition = Vector3.Zero;

        Settings.Enable.OnValueChanged += (sender, val) => TasksToggle(val);
        Settings.IsFollowing.OnValueChanged += (sender, val) => TasksToggle(val);

        return base.Initialise();
    }

    private void TasksToggle(bool val)
    {
        if (val)
        {
            UiCheckerCoRoutine.Init();
            PickUpCoRoutine.Init();
            FollowCoRoutine.Init();
            BlinkCoRoutine.Init();
            DumperCoRoutine.Init();
            ShockBotCoRoutine.Init();
            CurseBotCoRoutine.Init();
        }
        else
        {
            UiCheckerCoRoutine.Stop();
            PickUpCoRoutine.Stop();
            FollowCoRoutine.Stop();
            BlinkCoRoutine.Stop();
            DumperCoRoutine.Stop();
            ShockBotCoRoutine.Stop();
            CurseBotCoRoutine.Stop();
        }
    }

    public override void DrawSettings()
    {
        CopilotSettingsHandler.DrawCustomSettings();
        base.DrawSettings();
    }

    public override void AreaChange(AreaInstance area)
    {
        lastTargetPosition = Vector3.Zero;
        _followTarget = null;
        base.AreaChange(area);
    }

    public override void Tick()
    {
        if (Settings.TogglePauseHotkey.PressedOnce())
        {
            Settings.IsFollowing.Value = !Settings.IsFollowing.Value;
        }
    }
}
