using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

using ExileCore2;
using ExileCore2.Shared;
using ExileCore2.PoEMemory.MemoryObjects;

using Copilot.Api;

namespace Copilot.Utils;

public static class SyncInput
{
    public static void ReleaseKeys()
    {
        Input.KeyUp(Keys.LControlKey);
        Input.KeyUp(Keys.Shift);
        Input.KeyUp(Keys.Space);
        Input.KeyUp(Copilot.Main.Settings.Additional.FollowKey.Value);
    }

    public static async SyncTask<bool> PressKey(Keys key)
    {
        Input.KeyDown(key);
        await SyncInput.Delay(10);
        Input.KeyUp(key);
        await SyncInput.Delay(10);

        return true;
    }

    public static void KeyPress(Keys key)
    {
        Input.KeyDown(key);
        Input.KeyUp(key);
    }

    public static async SyncTask<bool> MoveMouse(object entityOrPos, int afterDelay)
    {
        MoveMouse(entityOrPos);
        await SyncInput.Delay(afterDelay);
        return true;
    }

    public static void MoveMouse(object entityOrPos)
    {
        Vector2 pos = entityOrPos switch
        {
            Entity entity => Ui.Camera.WorldToScreen(entity.Pos),
            EntityWrapper entityWrapper => Ui.Camera.WorldToScreen(entityWrapper.Pos),
            Vector3 vector3 => Ui.Camera.WorldToScreen(vector3),
            Vector2 vector2 => vector2,
            _ => throw new ArgumentException("Unsupported type for MoveMouse", nameof(entityOrPos))
        };

        Input.SetCursorPos(pos);
        Input.MouseMove();
    }

    public static async SyncTask<bool> LClick(object entityOrPos, int afterDelay)
    {
        LClick(entityOrPos);
        await SyncInput.Delay(afterDelay);
        return true;
    }

    public static void LClick(object entityOrPos)
    {
        MoveMouse(entityOrPos, 20);
        Input.Click(MouseButtons.Left);
    }

    public static async SyncTask<bool> Delay(int delay)
    {
        var settings = Copilot.Main.Settings.Additional;
        var random = new Random();
        int randomDelay = random.Next(settings.RandomDelayMin, settings.RandomDelayMax);
        await Task.Delay(delay + randomDelay);
        return true;
    }
}