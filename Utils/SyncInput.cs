using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

using ExileCore2;
using ExileCore2.Shared;

namespace Copilot.CoRoutines;
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
        await Task.Delay(10);
        Input.KeyUp(key);
        await Task.Delay(10);

        return true;
    }

    public static async SyncTask<bool> MoveMouse(Vector2 pos, int afterDelay = 10)
    {
        Input.SetCursorPos(pos);
        Input.MouseMove();
        await Task.Delay(afterDelay);
        return true;
    }

    public static async SyncTask<bool> LClick(Vector2 pos, int afterDelay = 10)
    {
        await MoveMouse(pos, afterDelay);
        Input.Click(MouseButtons.Left);
        await Task.Delay(afterDelay);
        return true;
    }
}