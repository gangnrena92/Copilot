using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings.Tasks;

[Submenu(CollapsedByDefault = true)]
public class GuildStashDumperSettings
{
    [Menu("Selected Tab", "The tab to dump the items.")]
    public ListNode SelectedTab { get; set; } = new ListNode();

    [Menu("Click Delay", "The delay between clicks. Default is 300ms because Guild Stash is slower than normal stash.")]
    public RangeNode<int> ClickDelay { get; set; } = new RangeNode<int>(300, 1, 1000);

    [Menu("Action Cooldown", "Delay to try to dump the items. Default: 5000.")]
    public RangeNode<int> Cooldown { get; set; } = new RangeNode<int>(5000, 1, 10000);
}