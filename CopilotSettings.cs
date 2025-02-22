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

        [Menu(null, "~200 as default")]
        public RangeNode<int> IdleDistance { get; set; } = new RangeNode<int>(200, 5, 1000);
        public HotkeyNode MovementKey { get; set; } = new HotkeyNode(Keys.T); // Default to no key set
        public HotkeyNode TogglePauseHotkey { get; set; } = new HotkeyNode(Keys.OemPeriod); // Default to Period key
        public ToggleNode IsPaused { get; set; } = new ToggleNode(false); // Default to not paused
        public ToggleNode UseBlink { get; set; } = new ToggleNode(false);

        [Menu(null, "Minimum range to attempt to TP to the target player (default is 1000)")]
        public RangeNode<int> BlinkRange { get; set; } = new RangeNode<int>(1000, 10, 2000);

        [Menu(null, "If it's in range it will try to TP every {cooldown}, Cooldown in milliseconds (default is 500)")]
        public RangeNode<int> BlinkCooldown { get; set; } = new RangeNode<int>(500, 100, 10000);

        public CopilotSettings()
        {
            Enable = new ToggleNode(true);
        }
    }
}
