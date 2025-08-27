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
        await Delay(10);
        Input.KeyUp(key);
        await Delay(10);

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
        await Delay(afterDelay);
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

    public static async SyncTask<bool> LClick(object entityOrPos, int afterDelay = 10)
    {
        await MoveMouse(entityOrPos, afterDelay);
        Input.Click(MouseButtons.Left);
        await Delay(afterDelay);
        return true;
    }

    public static async SyncTask<bool> Delay(int delay)
    {
        var settings = Copilot.Main.Settings.Additional;
        var random = new Random();
        int randomDelay = random.Next(settings.RandomDelayMin, settings.RandomDelayMax);
        await Task.Delay(delay + randomDelay);
        return true;
    }
    public static async SyncTask<bool> MoveWithWASD(Vector3 targetPosition, EntityWrapper player)
    {
        if (player == null) return false;
        
        var direction = targetPosition - player.Pos;
        direction = Vector3.Normalize(direction);
        
        // Определяем основные направления
        var forward = new Vector3(0, 0, 1); // Предполагаемая ориентация камеры
        var right = new Vector3(1, 0, 0);
        
        var dotForward = Vector3.Dot(direction, forward);
        var dotRight = Vector3.Dot(direction, right);
        
        // Отпускаем все клавиши движения
        ReleaseMovementKeys();
        
        // Нажимаем нужные клавиши based on direction
        if (Math.Abs(dotForward) > Math.Abs(dotRight))
        {
            if (dotForward > 0)
                Input.KeyDown(Copilot.Main.Settings.Additional.WKey.Value);
            else
                Input.KeyDown(Copilot.Main.Settings.Additional.SKey.Value);
        }
        else
        {
            if (dotRight > 0)
                Input.KeyDown(Copilot.Main.Settings.Additional.DKey.Value);
            else
                Input.KeyDown(Copilot.Main.Settings.Additional.AKey.Value);
        }
        
        await Delay(100);
        return true;
    }
    
    public static void ReleaseMovementKeys()
    {
        var settings = Copilot.Main.Settings.Additional;
        Input.KeyUp(settings.WKey.Value);
        Input.KeyUp(settings.AKey.Value);
        Input.KeyUp(settings.SKey.Value);
        Input.KeyUp(settings.DKey.Value);
        Input.KeyUp(settings.FollowKey.Value);
    }
    
    public static async SyncTask<bool> MoveToTarget(EntityWrapper target, EntityWrapper player)
    {
        var settings = Copilot.Main.Settings.Additional;
        
        switch (settings.MovementType.Value)
        {
            case "WASD":
                return await MoveWithWASD(target.Pos, player);
                
            case "Follow Key":
                await MoveMouse(target, 10);
                await PressKey(settings.FollowKey.Value);
                return true;
                
            case "Mouse Click":
                await LClick(target, 20);
                return true;
                
            default:
                return await MoveWithWASD(target.Pos, player);
        }
    }
}
