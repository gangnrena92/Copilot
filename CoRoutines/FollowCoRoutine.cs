using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;

using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;

using Copilot.Settings;
using Copilot.Api;
using static Copilot.Copilot;

namespace Copilot.CoRoutines
{
    internal class FollowCoRoutine
    {
        private static CopilotSettings Settings => Main.Settings;
        private static LoggerPlus Log = new LoggerPlus("FollowCoRoutine");
        private static Vector3 lastTargetPosition = Vector3.Zero;

        public static void Init() => TaskRunner.Run(Follow_Task, "FollowCoRoutine");
        public static void Stop() => TaskRunner.Stop("FollowCoRoutine");

        private static async Task<bool> Follow_Task()
        {
            while (true)
            {
                await Task.Delay(Settings.ActionCooldown);

                try
                {
                    if (_player == null || State.IsLoading || Ui.ResurrectPanel.IsVisible || Copilot.DontFollow) 
                        continue;

                    if (_target == null) continue;

                    var distanceToTarget = Vector3.Distance(_player.Entity.Pos, _target.Entity.Pos);
                    if (distanceToTarget <= Settings.FollowDistance) continue;

                    await MoveToward();
                }
                catch (Exception e)
                {
                    Log.Error($"Error in Follow_Task: {e.Message}");
                }
            }
        }

        private static async Task MoveToward()
        {
            var additional = Settings.Additional;

            if (additional.MovementModeOption == AdditionalSettings.MovementMode.Mouse)
            {
                if (additional.UseMouse)
                {
                    // Наведение мыши и клик
                    var targetPos = _target.Entity.Pos;
                    GameController.Mouse.MoveCursor(targetPos.X, targetPos.Y);
                    GameController.Mouse.LeftClick();
                }
                else
                {
                    // Следование через клавишу FollowKey
                    GameController.Keyboard.KeyDown(additional.FollowKey.Value);
                    await Task.Delay(50);
                    GameController.Keyboard.KeyUp(additional.FollowKey.Value);
                }
            }
            else if (additional.MovementModeOption == AdditionalSettings.MovementMode.WASD)
            {
                Vector3 playerPos = _player.Entity.Pos;
                Vector3 targetPos = _target.Entity.Pos;

                Vector3 direction = targetPos - playerPos;
                float distance = direction.Length();

                if (distance < 0.5f) return;

                double angle = Math.Atan2(direction.Y, direction.X) * (180 / Math.PI);

                List<ExileCore2.Shared.Enums.Keys> keysToPress = new List<ExileCore2.Shared.Enums.Keys>();

                if (angle >= -22.5 && angle < 22.5) keysToPress.Add(ExileCore2.Shared.Enums.Keys.D);
                else if (angle >= 22.5 && angle < 67.5) { keysToPress.Add(ExileCore2.Shared.Enums.Keys.W); keysToPress.Add(ExileCore2.Shared.Enums.Keys.D); }
                else if (angle >= 67.5 && angle < 112.5) keysToPress.Add(ExileCore2.Shared.Enums.Keys.W);
                else if (angle >= 112.5 && angle < 157.5) { keysToPress.Add(ExileCore2.Shared.Enums.Keys.W); keysToPress.Add(ExileCore2.Shared.Enums.Keys.A); }
                else if (angle >= 157.5 || angle < -157.5) keysToPress.Add(ExileCore2.Shared.Enums.Keys.A);
                else if (angle >= -157.5 && angle < -112.5) { keysToPress.Add(ExileCore2.Shared.Enums.Keys.S); keysToPress.Add(ExileCore2.Shared.Enums.Keys.A); }
                else if (angle >= -112.5 && angle < -67.5) keysToPress.Add(ExileCore2.Shared.Enums.Keys.S);
                else if (angle >= -67.5 && angle < -22.5) { keysToPress.Add(ExileCore2.Shared.Enums.Keys.S); keysToPress.Add(ExileCore2.Shared.Enums.Keys.D); }

                // Удерживаем клавиши до приближения к цели
                while (distance > 0.5f)
                {
                    foreach (var key in keysToPress)
                        GameController.Keyboard.KeyDown(key);

                    await Task.Delay(50);

                    foreach (var key in keysToPress)
                        GameController.Keyboard.KeyUp(key);

                    playerPos = _player.Entity.Pos;
                    direction = targetPos - playerPos;
                    distance = direction.Length();
                }
            }

            lastTargetPosition = _target.Entity.Pos;
        }
    }
}
