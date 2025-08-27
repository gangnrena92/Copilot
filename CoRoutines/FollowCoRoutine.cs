using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

using ExileCore2;
using ExileCore2.PoEMemory.MemoryObjects;

using Copilot.Api;
using Copilot.Settings;
using Copilot.Classes;

namespace Copilot.CoRoutines
{
    internal class FollowCoRoutine
    {
        private static CopilotSettings Settings => Copilot.Main.Settings;
        private static EntityWrapper _player => Copilot._player;
        private static EntityWrapper _target => Copilot._target;

        private static Vector3 lastTargetPosition = Vector3.Zero;

        public static void Init()
        {
            Task.Run(Follow_Task);
        }

        public static void Stop()
        {
            // ничего не делаем, так как задача работает асинхронно
        }

        private static async Task Follow_Task()
        {
            while (true)
            {
                await Task.Delay(Settings.ActionCooldown);

                try
                {
                    if (_player == null || State.IsLoading || Ui.ResurrectPanel.IsVisible || Copilot.DontFollow)
                        continue;

                    var distanceToTarget = _target != null ? Vector3.Distance(_player.Pos, _target.Pos) : 0;

                    if (_target == null || distanceToTarget <= Settings.FollowDistance)
                        continue;

                    await MoveToward();
                }
                catch (Exception e)
                {
                    Copilot.Main.GameController.LogMessage($"FollowCoRoutine Error: {e.Message}");
                }
            }
        }

        private static async Task MoveToward()
        {
            if (Settings.Additional.MovementMode == AdditionalSettings.MovementModeEnum.Mouse)
            {
                // простое движение мышью
                if (Settings.Additional.UseMouse)
                {
                    // эмуляция клика на цель
                    var pos = _target.Pos;
                    Cursor.Position = new System.Drawing.Point((int)pos.X, (int)pos.Y);
                    MouseClick();
                }
                else
                {
                    // движение с помощью FollowKey
                    SendKey(Settings.Additional.FollowKey);
                }
            }
            else if (Settings.Additional.MovementMode == AdditionalSettings.MovementModeEnum.WASD)
            {
                var playerPos = _player.Pos;
                var targetPos = _target.Pos;

                Vector3 direction = targetPos - playerPos;
                float distance = direction.Length();

                if (distance < 0.5f) return;

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

                List<Keys> keysToPress = new List<Keys>();
                if (key1 != Keys.None) keysToPress.Add(key1);
                if (key2 != Keys.None) keysToPress.Add(key2);

                while (distance > 0.5f)
                {
                    foreach (var key in keysToPress)
                        SendKeyDown(key);

                    await Task.Delay(50);

                    foreach (var key in keysToPress)
                        SendKeyUp(key);

                    playerPos = _player.Pos;
                    direction = targetPos - playerPos;
                    distance = direction.Length();
                }
            }

            lastTargetPosition = _target.Pos;
        }

        private static void MouseClick()
        {
            // простая эмуляция клика мыши
            System.Windows.Forms.Cursor.Position = System.Windows.Forms.Cursor.Position;
            // Можно добавить P/Invoke SendInput для более точного клика
        }

        private static void SendKey(Keys key)
        {
            SendKeyDown(key);
            Task.Delay(50).Wait();
            SendKeyUp(key);
        }

        private static void SendKeyDown(Keys key)
        {
            // P/Invoke или SendKeys
            System.Windows.Forms.SendKeys.SendWait("{" + key.ToString() + " down}");
        }

        private static void SendKeyUp(Keys key)
        {
            System.Windows.Forms.SendKeys.SendWait("{" + key.ToString() + " up}");
        }
    }
}