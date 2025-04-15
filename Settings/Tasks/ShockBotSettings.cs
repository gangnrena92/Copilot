using System.Windows.Forms;

using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings.Tasks;

[Submenu(CollapsedByDefault = true)]
public class ShockBotSettings
{
    [Menu(null, "~1000 as default")]
    public RangeNode<int> ActionCooldown { get; set; } = new RangeNode<int>(1000, 50, 2000); // Cooldown in milliseconds

    [Menu("Monster Range to Shock", "The minimum range required to shock a monster. Default: 1000.")]
    public RangeNode<int> Range { get; set; } = new RangeNode<int>(1000, 1, 2000);

    [Menu("Ball Lightning Key", "The key to use for Ball Lightning. Default: Q.")]
    public HotkeyNode BallLightningKey { get; set; } = new HotkeyNode(Keys.Q);

    [Menu("Lightning Warp Key", "The key to use for Lightning Warp. Default: W.")]
    public HotkeyNode LightningWarpKey { get; set; } = new HotkeyNode(Keys.W);

    [Menu("Range of the ball to the boss use Lightning Warp", "The range to use Lightning Warp. Default: 600.")]
    public RangeNode<int> RangeToUseLightningWarp { get; set; } = new RangeNode<int>(600, 1, 1000);
}