using System.Windows.Forms;

using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot
{
    public class CopilotSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);
        public string[] PartyElements { get; set; } = new string[5];

        public TextNode TargetPlayerName { get; set; } = new TextNode("Leader");

        [Menu(null, "~460 as default")]
        public RangeNode<int> FollowDistance { get; set; } = new RangeNode<int>(460, 10, 1000);

        [Menu(null, "~100 as default")]
        public RangeNode<int> ActionCooldown { get; set; } = new RangeNode<int>(100, 50, 20000); // Cooldown in milliseconds

        public HotkeyNode TogglePauseHotkey { get; set; } = new HotkeyNode(Keys.OemPeriod); // Default to Period key

        public ToggleNode IsPaused { get; set; } = new ToggleNode(false); // Default to not paused

        [Menu("Blink Settings")]
        public BlinkSettings Blink { get; set; } = new BlinkSettings();

        [Menu("Pickup Settings")]
        public PickupSettings Pickup { get; set; } = new PickupSettings();
    }

    [Submenu(CollapsedByDefault = true)]
    public class BlinkSettings
    {
        [Menu("Enable")]
        public ToggleNode Enable { get; set; } = new ToggleNode(false);

        [Menu("Range", "The minimum range required to teleport (TP) to the target player. Default: 1000.")]
        public RangeNode<int> Range { get; set; } = new RangeNode<int>(1000, 10, 2000);

        [Menu("Cooldown", "If within range, it will attempt to TP every {cooldown} milliseconds. Default: 500.")]
        public RangeNode<int> Cooldown { get; set; } = new RangeNode<int>(500, 100, 10000);
    }

    [Submenu(CollapsedByDefault = true)]
    public class PickupSettings
    {
        [Menu("Enable", "This will enable the item pickup of EVERY item that is within the range.")]
        public ToggleNode Enable { get; set; } = new ToggleNode(false);

        [Menu("Range", "The minimum range required to pick up an item. Default: 400.")]
        public RangeNode<int> Range { get; set; } = new RangeNode<int>(400, 1, 1000);

        [Menu("Item Filter", "Comma-separated list of item names to pick up. e.g. Orb,Mirror,...")]
        public TextNode Filter { get; set; } = new TextNode("Orb,Mirror");

        [Menu("Ignore if target too far", "If the target is too far away, ignore the items. Default: 1200.")]
        public RangeNode<int> RangeToIgnore { get; set; } = new RangeNode<int>(1200, 1, 3000);
    }
}
