using System.Windows.Forms;

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
}

[Submenu(CollapsedByDefault = false)]
public class TasksSettings
{
    [Menu("Enable UI Checker", "This will enable the UI checker task.")]
    public ToggleNode IsUiCheckerEnabled { get; set; } = new ToggleNode(true);

    [Menu("Enable Blink", "This will enable the blink task.")]
    public ToggleNode IsBlinkEnabled { get; set; } = new ToggleNode(false);

    [Menu("Enable Pickup", "This will enable the pickup task.")]
    public ToggleNode IsPickupEnabled { get; set; } = new ToggleNode(false);

    [Menu("Enable Shock Bot", "This will enable the shock bot task.")]
    public ToggleNode IsShockBotEnabled { get; set; } = new ToggleNode(false);

    [Menu("Enable Guild Stash Dumper", "This will enable the guild stash dumper task.")]
    public ToggleNode IsDumperEnabled { get; set; } = new ToggleNode(false);

    public UiCheckerSettings UiChecker { get; set; } = new UiCheckerSettings();
    public BlinkSettings Blink { get; set; } = new BlinkSettings();
    public PickupSettings Pickup { get; set; } = new PickupSettings();
    public ShockBotSettings ShockBot { get; set; } = new ShockBotSettings();
    public GuildStashDumperSettings Dumper { get; set; } = new GuildStashDumperSettings();
}
