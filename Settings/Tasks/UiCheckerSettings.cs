using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings.Tasks;

[Submenu(CollapsedByDefault = true)]
public class UiCheckerSettings
{
    public ToggleNode AutoRespawn { get; set; } = new ToggleNode(true);

    [Menu(null, "This will enable the UI checker task, which will close any open UI.")]
    public RangeNode<int> Cooldown { get; set; } = new RangeNode<int>(1000, 50, 2000);
}