using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Copilot.Settings;
using Copilot.Utils;
using Copilot.Classes;
using Copilot.Api;
using static Copilot.Copilot;
using static Copilot.Api.Ui;

namespace Copilot.CoRoutines
{
    internal class FollowCoRoutine
    {
        private static CopilotSettings Settings => Main.Settings;

        private static LoggerPlus Log = new LoggerPlus("FollowCoRoutine");
        private static Vector3 lastTargetPosition = Vector3.Zero;

        public static void Init()
        {
            TaskRunner.Run(Follow_Task, "FollowCoRoutine");
        }

        public static void Stop()
        {
            TaskRunner.Stop("FollowCoRoutine");
        }

        public static async Task<bool> Follow_Task()
        {
            while (true)
            {
                await Task.Delay(Settings.ActionCooldown);

                try
                {
                    if (_player == null || State.IsLoading || Ui.ResurrectPanel.IsVisible || DontFollow)
                        continue;

                    var leaderPE = GetLeaderPartyElement();

                    if (_target == null)
                    {
                        Log.Message("Target not found, trying to follow the leader...");
                        continue;
                    }

                    if (State.IsTown || (State.IsHideout && Settings.Tasks.IsDumperEnabled && Api.Inventory.Items.Count != 0))
                        continue;

                    var distanceToTarget = _player.DistanceTo(_target.Entity);
                    if (distanceToTarget <= Settings.FollowDistance) continue;

                    if (lastTargetPosition == Vector3.Zero) lastTargetPosition = _target.Pos;

                    await MoveToward();
                }
                catch (Exception e)
                {
                    Log.Error($"Error in main loop: {e.Message}");
                    continue;
                }
            }
        }

        private static async Task<bool> MoveToward()
        {
            if (Settings.Additional.MovementModeOption == AdditionalSettings.MovementMode.Mouse)
            {
                if (Settings.Additional.UseMouse)
                    await SyncInput.LClick(_target, 20);
                else
                    await SyncInput.PressKey(Settings.Additional.FollowKey);
            }
            else if (Settings.Additional.MovementModeOption == AdditionalSettings.MovementMode.WASD)
            {
                while (true)
                {
                    if (_player == null || _target == null) break;

                    var playerPos = _player.Pos;
                    var targetPos = _target.Pos;

                    var direction = targetPos - playerPos;
                    var distance = direction.Length();
                    if (distance < 0.5f) break;

                    double angle = Math.Atan2(direction.Y, direction.X) * (180 / Math.PI);

                    Keys key1 = Keys.None;
                    Keys key2 = Keys.None;

                    if (angle >= -22.5 && angle < 22.5) key1 = Keys.D;
                    else if (angle >= 22.5 && angle < 67.5) { key1 = Keys.W; key2 = Keys.D; }
                    else if (angle >= 67.5 && angle < 112.5) key1 = Keys.W;
                    else if (angle >= 112.5 && angle < 157.5) { key1 = Keys.W; key2 = Keys.A; }
                    else if (angle >= 157.5 || angle < -157.5) key1 = Keys.A;
                    else if (angle >= -157.5 && angle < -112.5) { key1 = Keys.S; key2 = Keys.A; }
                    else if (angle >= -112.5 && angle < -67.5) key1 = Keys.S;
                    else if (angle >= -67.5 && angle < -22.5) { key1 = Keys.S; key2 = Keys.D; }

                    if (key1 != Keys.None) await SyncInput.KeyDown(key1);
                    if (key2 != Keys.None) await SyncInput.KeyDown(key2);

                    await Task.Delay(new Random().Next(Settings.Additional.RandomDelayMin, Settings.Additional.RandomDelayMax));

                    if (key1 != Keys.None) await SyncInput.KeyUp(key1);
                    if (key2 != Keys.None) await SyncInput.KeyUp(key2);
                }
            }

            lastTargetPosition = _target.Pos;
            return true;
        }
    }
}
