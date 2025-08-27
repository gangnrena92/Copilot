using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;

using Copilot.Api;
using Copilot.Settings;

namespace Copilot.CoRoutines;

public static class FollowCoRoutine
{
    private static Copilot CopilotMain => Copilot.Main;
    private static AdditionalSettings Additional => CopilotMain.Settings.Additional;
    private static bool _isRunning = false;

    public static void Init()
    {
        if (_isRunning) return;
        _isRunning = true;
        _ = FollowTask();
    }

    public static void Stop()
    {
        _isRunning = false;
    }

    private static async Task FollowTask()
    {
        while (_isRunning)
        {
            try
            {
                var player = CopilotMain._player;
                var target = CopilotMain._target;

                if (player == null || target == null || State.IsLoading || Ui.ResurrectPanel.IsVisible)
                {
                    await Task.Delay(100);
                    continue;
                }

                // Рассчитываем дистанцию
                var distance = Vector3.Distance(player.Pos, target.Pos);

                if (distance < 1f)
                {
                    await Task.Delay(50);
                    continue;
                }

                if (Additional.IsMouseMode())
                {
                    // Движение через мышь
                    MoveMouseTowards(target.Pos);
                }
                else
                {
                    // Движение через WASD
                    MoveWASD(player.Pos, target.Pos);
                }

                await Task.Delay(50);
            }
            catch
            {
                await Task.Delay(100);
            }
        }
    }

    private static void MoveMouseTowards(Vector3 targetPos)
    {
        var screenPos = CopilotMain.GameController.Camera.WorldToScreen(targetPos);
        if (screenPos == null) return;

        Cursor.Position = new System.Drawing.Point((int)screenPos.Value.X, (int)screenPos.Value.Y);

        if (Additional.UseMouse)
        {
            MouseClick();
        }
        else
        {
            SetKeyDown(Additional.FollowKey);
        }
    }

    private static void MoveWASD(Vector3 playerPos, Vector3 targetPos)
    {
        var direction = targetPos - playerPos;
        double angle = Math.Atan2(direction.Y, direction.X) * (180 / Math.PI);

        var keysToPress = new List<Keys>();

        if (angle >= -22.5 && angle < 22.5) keysToPress.Add(Keys.D);                // E
        else if (angle >= 22.5 && angle < 67.5) { keysToPress.Add(Keys.W); keysToPress.Add(Keys.D); } // NE
        else if (angle >= 67.5 && angle < 112.5) keysToPress.Add(Keys.W);          // N
        else if (angle >= 112.5 && angle < 157.5) { keysToPress.Add(Keys.W); keysToPress.Add(Keys.A); } // NW
        else if (angle >= 157.5 || angle < -157.5) keysToPress.Add(Keys.A);        // W
        else if (angle >= -157.5 && angle < -112.5) { keysToPress.Add(Keys.S); keysToPress.Add(Keys.A); } // SW
        else if (angle >= -112.5 && angle < -67.5) keysToPress.Add(Keys.S);        // S
        else if (angle >= -67.5 && angle < -22.5) { keysToPress.Add(Keys.S); keysToPress.Add(Keys.D); } // SE

        foreach (var key in keysToPress)
            SetKeyDown(key);

        Task.Delay(50).Wait();

        foreach (var key in keysToPress)
            SetKeyUp(key);
    }

    private static void MouseClick()
    {
        // Симулируем левый клик мыши
        System.Windows.Forms.MouseButtons button = System.Windows.Forms.MouseButtons.Left;
        Cursor.Position = Cursor.Position; // обновляем
        // Можно добавить P/Invoke SendInput для реального клика
    }

    private static void SetKeyDown(Keys key)
    {
        CopilotMain.GameController.Input.SetKeyState(key, true);
    }

    private static void SetKeyUp(Keys key)
    {
        CopilotMain.GameController.Input.SetKeyState(key, false);
    }
}
