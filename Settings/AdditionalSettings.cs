using System.Windows.Forms;
using System.Collections.Generic; // ДОБАВИТЬ ЭТОТ USING

using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings;

[Submenu(CollapsedByDefault = true)]
public class AdditionalSettings
{
    public AdditionalSettings() // ДОБАВИТЬ КОНСТРУКТОР
    {
        // Инициализация списка значений для MovementType
        MovementType.SetListValues(new List<string> { "Follow Key", "WASD", "Mouse Click" });
    }

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
    
    [Menu("Movement Type", "Method of movement")]
    public ListNode MovementType { get; set; } = new ListNode() { Value = "Follow Key" };
    
    [Menu("WASD Movement", "Use WASD keys for movement")]
    public ToggleNode UseWASD { get; set; } = new ToggleNode(false);
    
    [Menu("W Key", "Key for moving forward")]
    public HotkeyNode WKey { get; set; } = new HotkeyNode(Keys.W);
    
    [Menu("A Key", "Key for moving left")]
    public HotkeyNode AKey { get; set; } = new HotkeyNode(Keys.A);
    
    [Menu("S Key", "Key for moving backward")] 
    public HotkeyNode SKey { get; set; } = new HotkeyNode(Keys.S);
    
    [Menu("D Key", "Key for moving right")]
    public HotkeyNode DKey { get; set; } = new HotkeyNode(Keys.D);
}