using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

using ExileCore2;
using ExileCore2.PoEMemory.Elements;

using Copilot.Api;
using Copilot.Settings;
using static Copilot.Copilot;

namespace Copilot.CoRoutines
{
    internal static class FollowCoRoutine
    {
        private static CopilotSettings Settings => Main.Settings;

        private static Vector3 lastTargetPosition = Vector3.Zero;

        public static void Init()
        {
            Task.Run(FollowTask);
        }

        public static void Stop()
        {
            // ничего не делаем — Task завершится сама, когда условия не выполняются
        }

        private static async Task FollowTask()
        {
            while (true)
            {
                await Task.Delay(Settings.ActionCooldown);

                try
                {
                    if (_player == null || State.IsLoading || Ui.ResurrectPanel.IsVisible || DontFollow || _target == null)
                        continue;

                    var distance = Vector3.Distance(_player.Pos, _target.Pos);
                    if (distance <= Settings.FollowDistance)
                        continue;

                    await MoveToward();
                }
                catch (Exception e)
                {
                    Main.Log.Message($"FollowCoRoutine error: {e}");
                }
            }
        }

        private static async Task MoveToward()
        {
            if (Settings.Additional.MovementMode == MovementMode.Mouse)
            {
                if (Settings.Additional.UseMouse)
                {
                    // ЛКМ к цели
                    Input.SetCursorPos((int)_target.Pos.X, (int)_target.Pos.Y);
                    Input.LeftClick();
                }
                else
                {
                    // Нажимаем FollowKey
                    Input.KeyDown(Settings.Additional.FollowKey);
                    await Task.Delay(50);
                    Input.KeyUp(Settings.Additional.FollowKey);
                }
            }
            else if (Settings.Additional.MovementMode == MovementMode.WASD)
            {
                Vector3 playerPos = _player.Pos;
                Vector3 targetPos = _target.Pos;

                Vector3 direction = targetPos - playerPos;
                float distance = direction.Length();

                if (distance < 0.5f) return;

                double angle = Math.Atan2(direction.Y, direction.X) * (180 / Math.PI);

                Keys key1 = Keys.None;
                Keys key2 = Keys.None;

                if (angle >= -22.5 && angle < 22.5) key1 = Keys.D;                  // E
                else if (angle >= 22.5 && angle < 67.5) { key1 = Keys.W; key2 = Keys.D; }  // NE
                else if (angle >= 67.5 && angle < 112.5) key1 = Keys.W;             // N
                else if (angle >= 112.5 && angle < 157.5) { key1 = Keys.W; key2 = Keys.A; } // NW
                else if (angle >= 157.5 || angle < -157.5) key1 = Keys.A;           // W
                else if (angle >= -157.5 && angle < -112.5) { key1 = Keys.S; key2 = Keys.A; } // SW
                else if (angle >= -112.5 && angle < -67.5) key1 = Keys.S;           // S
                else if (angle >= -67.5 && angle < -22.5) { key1 = Keys.S; key2 = Keys.D; } // SE

                List<Keys> keysToPress = new List<Keys>();
                if (key1 != Keys.None) keysToPress.Add(key1);
                if (key2 != Keys.None) keysToPress.Add(key2);

                while (distance > 0.5f)
                {
                    foreach (var key in keysToPress)
                        Input.SetKeyState(key, true);

                    await Task.Delay(50);

                    foreach (var key in keysToPress)
                        Input.SetKeyState(key, false);

                    playerPos = _player.Pos;
                    direction = targetPos - playerPos;
                    distance = direction.Length();
                }
            }

            lastTargetPosition = _target.Pos;
        }
    }
}