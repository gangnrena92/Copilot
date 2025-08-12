using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

using ExileCore2;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.PoEMemory.Components;
using ExileCore2.Shared.Enums;

using Copilot.Utils;
using Copilot.Settings;
using Copilot.CoRoutines;
using Copilot.Api;

// TODO: ghost follow
// - debug to see like a crosshair
// TODO: circle people
// TODO: pots

namespace Copilot;

public sealed class Copilot : BaseSettingsPlugin<CopilotSettings>
{
    public static Copilot Main;
    public LoggerPlus Log => new LoggerPlus("Core");

    public bool RessurectedRecently = false;
    public ushort TpTries = 0;

    public static bool DontFollow = false;

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
        TpTries = 0;
        base.AreaChange(area);
    }

    public override void Tick()
    {
        if (!Settings.Enable || !GameController.Window.IsForeground()) return;

        if (Settings.TogglePauseHotkey.PressedOnce())
        {
            Settings.IsFollowing.Value = !Settings.IsFollowing.Value;
        }

        _player = (GameController.Player == null || State.IsLoading) ? null : new EntityWrapper(GameController.Player);
        var followEntity = GetFollowingTarget();
        _target = followEntity != null ? new EntityWrapper(followEntity) : null;
    }

    private Entity GetFollowingTarget()
    {
        try
        {
            var leaderName = Settings.TargetPlayerName.Value.ToLower();
            var target = Entities.ListByType(EntityType.Player)
                .FirstOrDefault(x => string.Equals(x.GetComponent<Player>()?.PlayerName.ToLower(), leaderName, StringComparison.OrdinalIgnoreCase));
            return target;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
