using System.Windows.Forms;
using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings
{
    [Submenu(CollapsedByDefault = true)]
    public class AdditionalSettings
    {
        // Режим движения: Mouse или WASD
        public enum MovementMode
        {
            Mouse,
            WASD
        }

        [Menu("Movement Mode", "Выберите способ движения персонажа")]
        public MovementMode MovementModeOption { get; set; } = MovementMode.Mouse;

        [Menu("Use Mouse to Follow", "Рекомендуется только для режима Mouse")]
        public ToggleNode UseMouse { get; set; } = new ToggleNode(true);

        [Menu("Follow Key", "Клавиша для следования (если UseMouse=false)")]
        public HotkeyNode FollowKey { get; set; } = new HotkeyNode(Keys.T);

        [Menu("Random Delay Minimum")]
        public RangeNode<int> RandomDelayMin { get; set; } = new RangeNode<int>(30, 1, 500);

        [Menu("Random Delay Maximum")]
        public RangeNode<int> RandomDelayMax { get; set; } = new RangeNode<int>(100, 1, 500);

        [Menu("Debug Mode", "Включает вывод отладочной информации")]
        public ToggleNode Debug { get; set; } = new ToggleNode(false);

        public bool IsMouseMode()
        {
            return MovementModeOption == MovementMode.Mouse;
        }
    }
}
