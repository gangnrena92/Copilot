using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings.Tasks;

[Submenu(CollapsedByDefault = true)]
public class BlinkSettings
{
    [Menu("Range", "The minimum range required to teleport (TP) to the target player. Default: 1000.")]
    public RangeNode<int> Range { get; set; } = new RangeNode<int>(1000, 10, 2000);

    [Menu("Cooldown", "If within range, it will attempt to TP every {cooldown} milliseconds. Default: 500.")]
    public RangeNode<int> Cooldown { get; set; } = new RangeNode<int>(500, 100, 10000);
}