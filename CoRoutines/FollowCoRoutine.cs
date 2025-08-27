using System;
using System.Numerics;
using System.Threading.Tasks;
using ExileCore2;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Nodes;
using Copilot.Settings;
using Copilot.Api;

namespace Copilot.CoRoutines
{
    public static class FollowCoRoutine
    {
        private static bool _isRunning;
        private static Task _followTask;

        public static void Init()
        {
            if (_isRunning) return;
            _isRunning = true;
            _followTask = Task.Run(FollowLoop);
        }

        public static void Stop()
        {
            _isRunning = false;
            _followTask?.Wait();
            _followTask = null;
        }

        private static async Task FollowLoop()
        {
            while (_isRunning)
            {
                await Task.Delay(Copilot.Main.Settings.ActionCooldown.Value);

                var player = Copilot._player;
                var target = Copilot._target;

                if (player == null || target == null || State.IsLoading || Ui.ResurrectPanel.IsVisible)
                    continue;

                var distance = Vector3.Distance(player.Pos, target.Pos);

                // движение к цели
                if (distance > Copilot.Main.Settings.FollowDistance.Value)
                {
                    MoveToTarget(player, target);
                }
            }
        }

        private static void MoveToTarget(EntityWrapper player, EntityWrapper target)
        {
            var settings = Copilot.Main.Settings.Additional;

            if (settings.MovementModeOption == AdditionalSettings.MovementMode.Mouse)
            {
                var screenPos = Copilot.Main.GameController.IngameState.Camera.WorldToScreen(target.Pos);
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)screenPos.X, (int)screenPos.Y);

                // эмуляция клика мыши
                MouseClick();
            }
            else // WASD
            {
                var direction = target.Pos - player.Pos;
                PressWASD(direction);
            }
        }

        private static void MouseClick()
        {
            var input = Copilot.Main.GameController.IngameState.IngameUi.Input;
            input.SetKeyState(System.Windows.Forms.Keys.LButton, true);
            input.SetKeyState(System.Windows.Forms.Keys.LButton, false);
        }

        private static void PressWASD(Vector3 direction)
        {
            var input = Copilot.Main.GameController.IngameState.IngameUi.Input;
            input.SetKeyState(System.Windows.Forms.Keys.W, direction.Z > 0);
            input.SetKeyState(System.Windows.Forms.Keys.S, direction.Z < 0);
            input.SetKeyState(System.Windows.Forms.Keys.A, direction.X < 0);
            input.SetKeyState(System.Windows.Forms.Keys.D, direction.X > 0);
        }
    }
}
