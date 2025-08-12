using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings.Tasks;

[Submenu(CollapsedByDefault = true)]
public class PickupSettings
{
    [Menu("Use Target's Position", "This will use the target's position to pick up items.")]
    public ToggleNode UseTargetPosition { get; set; } = new ToggleNode(false);

    [Menu("Range", "The minimum range required to pick up an item. Default: 400.")]
    public RangeNode<int> Range { get; set; } = new RangeNode<int>(400, 1, 1000);

    [Menu("Item Filter", "Comma-separated list of item names to pick up. e.g. Orb,Mirror,...")]
    public TextNode Filter { get; set; } = new TextNode("Orb,Mirror");

    [Menu("Ignore if target too far", "If the target is too far away, ignore the items. Default: 1200.")]
    public RangeNode<int> RangeToIgnore { get; set; } = new RangeNode<int>(1200, 1, 3000);

    [Menu("Delay", "This will add a delay between each item pickup. Default: 300.")]
    public RangeNode<int> Delay { get; set; } = new RangeNode<int>(300, 0, 1000);
}