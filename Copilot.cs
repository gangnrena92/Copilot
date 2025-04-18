using System.Numerics;

using ExileCore2;

using Copilot.Utils;
using Copilot.Settings;
using Copilot.CoRoutines;
using Copilot.Api;
using System.Collections.Generic;
using System.Linq;

// TODO: ghost follow
// - debug to see like a crosshair
// TODO: circle people
// TODO: pots
// TODO: fix AreaTransition
// TODO: better pickup filter make it able to use regex

namespace Copilot;

public sealed class Copilot : BaseSettingsPlugin<CopilotSettings>
{
    public static Copilot Main;
    public LoggerPlus Log => new LoggerPlus("Core");

    public bool AllowBlinkTask = false;

    public static EntityWrapper _target;
    public static EntityWrapper _player;
    public Vector3 lastTargetPosition = Vector3.Zero;

    public List<CustomCoRoutine> CustomCoRoutines = new List<CustomCoRoutine>();

    public override bool Initialise()
    {
        Main = this;
        Name = "Copilot";
        lastTargetPosition = Vector3.Zero;

        Settings.Enable.OnValueChanged += (sender, val) => TasksToggle(val);
        Settings.IsFollowing.OnValueChanged += (sender, val) => TasksToggle(val);

        _player = new EntityWrapper(GameController.Player);

        UpdateCustomCoRoutines();
        return base.Initialise();
    }

    public void UpdateCustomCoRoutines()
    {
        foreach (var customCoRoutine in CustomCoRoutines)
            customCoRoutine.Stop();

        CustomCoRoutines.Clear();
        CustomCoRoutines = Settings.CustomSettingsList
            .Select(s => new CustomCoRoutine(s))
            .ToList();
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

            foreach (var customCoRoutine in CustomCoRoutines)
                customCoRoutine.Init();
        }
        else
        {
            UiCheckerCoRoutine.Stop();
            PickUpCoRoutine.Stop();
            FollowCoRoutine.Stop();
            BlinkCoRoutine.Stop();
            DumperCoRoutine.Stop();

            foreach (var customCoRoutine in CustomCoRoutines)
                customCoRoutine.Stop();
        }
    }

    public override void DrawSettings()
    {
        base.DrawSettings();
        CopilotSettingsHandler.DrawSettings();
    }

    public override void AreaChange(AreaInstance area)
    {
        lastTargetPosition = Vector3.Zero;
        _target = null;
        base.AreaChange(area);
    }

    public override void Tick()
    {
        if (!Settings.Enable || !GameController.Window.IsForeground()) return;

        if (Settings.TogglePauseHotkey.PressedOnce())
        {
            Settings.IsFollowing.Value = !Settings.IsFollowing.Value;
        }

        _player = new EntityWrapper(GameController.Player);
    }
}
