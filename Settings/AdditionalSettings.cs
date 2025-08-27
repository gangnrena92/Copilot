using System.Collections.Generic;
using System.Windows.Forms;
using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace Copilot.Settings
{
    [Submenu(CollapsedByDefault = true)]
    public class AdditionalSettings
    {
        public enum MovementMode
        {
            Mouse,
            WASD
        }

        [Menu("Movement Mode", "Выберите способ передвижения")]
        public MovementMode MovementMode { get; set; } = MovementMode.Mouse;

        [Menu("Use Mouse to Follow", "Если включено, бот будет кликать по цели мышью (не рекомендуется)")]
        public ToggleNode UseMouse { get; set; } = new ToggleNode(true);

        [Menu("Follow with Key", "Клавиша для следования, если не используем мышь")]
        public HotkeyNode FollowKey { get; set; } = new HotkeyNode(Keys.T);

        [Menu("Random Delay Minimum", "Минимальная задержка между нажатиями клавиш WASD (мс)")]
        public RangeNode<int> RandomDelayMin { get; set; } = new RangeNode<int>(30, 1, 500);

        [Menu("Random Delay Maximum", "Максимальная задержка между нажатиями клавиш WASD (мс)")]
        public RangeNode<int> RandomDelayMax { get; set; } = new RangeNode<int>(100, 1, 500);

        [Menu("Debug", "Включает режим отладки")]
        public ToggleNode Debug { get; set; } = new ToggleNode(false);

        // Пример корректного ListNode
        [Menu("Follow Options")]
        public ListNode<string> FollowOptions { get; set; } = new ListNode<string>();

        // В конструкторе добавляем элементы
        public AdditionalSettings()
        {
            FollowOptions.Add("Option1");
            FollowOptions.Add("Option2");
        }
    }
}