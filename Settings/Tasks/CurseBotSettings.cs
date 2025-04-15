using System.Windows.Forms;

using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings.Tasks;

[Submenu(CollapsedByDefault = true)]
public class CurseBotSettings
{
    [Menu(null, "~1000 as default")]
    public RangeNode<int> ActionCooldown { get; set; } = new RangeNode<int>(1000, 50, 2000); // Cooldown in milliseconds

    [Menu("Range to curse")]
    public RangeNode<int> Range { get; set; } = new RangeNode<int>(1000, 1, 2000);

    [Menu("Range to Ignore", "If too far from target")]
    public RangeNode<int> IgnoreRange { get; set; } = new RangeNode<int>(1000, 1, 2000);

    public HotkeyNode CurseKey { get; set; } = new HotkeyNode(Keys.Q);
}