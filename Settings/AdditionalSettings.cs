using System.Windows.Forms;

using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings;

[Submenu(CollapsedByDefault = false)]
public class AdditionalSettings
{
    [Menu("Use Mouse to Follow")]
    public ToggleNode UseMouse { get; set; } = new ToggleNode(true);

    [Menu("Follow with key")]
    public HotkeyNode FollowKey { get; set; } = new HotkeyNode(Keys.T);

    [Menu("Debug", "This will enable the debug mode.")]
    public ToggleNode Debug { get; set; } = new ToggleNode(false);
}