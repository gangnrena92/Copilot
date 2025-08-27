using System.Windows.Forms;
using System.Collections.Generic;

using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

using Copilot.Settings.Tasks;

namespace Copilot.Settings;

public class CopilotSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(true);

    [Menu("Following", "This will enable everything related to following.")]
    public ToggleNode IsFollowing { get; set; } = new ToggleNode(false);
    
    public HotkeyNode TogglePauseHotkey { get; set; } = new HotkeyNode(Keys.OemPeriod);

    public ListNode TargetPlayerName { get; set; } = new ListNode();

    [Menu(null, "~460 as default")]
    public RangeNode<int> FollowDistance { get; set; } = new RangeNode<int>(460, 10, 1000);

    [Menu(null, "~100 as default")]
    public RangeNode<int> ActionCooldown { get; set; } = new RangeNode<int>(100, 50, 20000); // Cooldown in milliseconds

    [Menu("Tasks Settings")]
    public TasksSettings Tasks { get; set; } = new TasksSettings();

    [Menu("Additional Settings")]
    public AdditionalSettings Additional { get; set; } = new AdditionalSettings();

    public List<CustomSettings> CustomSettingsList { get; set; } = new List<CustomSettings>();
}

[Submenu(CollapsedByDefault = true)]
public class TasksSettings
{
    [Menu("Enable UI Checker", "This will enable the UI checker task.")]
    public ToggleNode IsUiCheckerEnabled { get; set; } = new ToggleNode(true);

    [Menu("Enable Blink", "This will enable the blink task.")]
    public ToggleNode IsBlinkEnabled { get; set; } = new ToggleNode(false);

    [Menu("Enable Pickup", "This will enable the pickup task.")]
    public ToggleNode IsPickupEnabled { get; set; } = new ToggleNode(false);

    [Menu("Enable Guild Stash Dumper", "This will enable the guild stash dumper task.")]
    public ToggleNode IsDumperEnabled { get; set; } = new ToggleNode(false);

    public UiCheckerSettings UiChecker { get; set; } = new UiCheckerSettings();
    public BlinkSettings Blink { get; set; } = new BlinkSettings();
    public PickupSettings Pickup { get; set; } = new PickupSettings();
    public GuildStashDumperSettings Dumper { get; set; } = new GuildStashDumperSettings();
}

[Submenu(CollapsedByDefault = true)]
public class AdditionalSettings
{
    public AdditionalSettings()
    {
        // Правильная инициализация ListNode
        MovementType = new ListNode();
        MovementType.Value = "Follow Key";
        MovementType.Values = new List<string> { "Follow Key", "WASD", "Mouse Click" };
    }

    [Menu("Use Mouse to Follow", "This is not recommended.")]
    public ToggleNode UseMouse { get; set; } = new ToggleNode(true);

    [Menu("Follow with key")]
    public HotkeyNode FollowKey { get; set; } = new HotkeyNode(Keys.T);

    [Menu("Random Delay Minimum")]
    public RangeNode<int> RandomDelayMin { get; set; } = new RangeNode<int>(30, 1, 500);

    [Menu("Random Delay Maximum")]
    public RangeNode<int> RandomDelayMax { get; set; } = new RangeNode<int>(100, 1, 500);

    [Menu("Debug", "This will enable the debug mode.")]
    public ToggleNode Debug { get; set; } = new ToggleNode(false);
    
    [Menu("Movement Type", "Method of movement")]
    public ListNode MovementType { get; set; }
    
    [Menu("W Key", "Key for moving forward")]
    public HotkeyNode WKey { get; set; } = new HotkeyNode(Keys.W);
    
    [Menu("A Key", "Key for moving left")]
    public HotkeyNode AKey { get; set; } = new HotkeyNode(Keys.A);
    
    [Menu("S Key", "Key for moving backward")] 
    public HotkeyNode SKey { get; set; } = new HotkeyNode(Keys.S);
    
    [Menu("D Key", "Key for moving right")]
    public HotkeyNode DKey { get; set; } = new HotkeyNode(Keys.D);
}