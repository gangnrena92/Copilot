using System.Windows.Forms;

using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings;

public enum MovementMode
{
    Mouse,
    WASD
}

[Submenu(CollapsedByDefault = true)]
public class AdditionalSettings
{
    [Menu("Movement Mode")]
    public MovementMode MovementMode { get; set; } = MovementMode.Mouse;

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
}