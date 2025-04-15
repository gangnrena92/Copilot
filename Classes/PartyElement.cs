using System.Numerics;

using ExileCore2.PoEMemory;

namespace Copilot.Classes;
public class PartyElement
{
    public string PlayerName { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public Element TpButton { get; set; } = new Element();

    public Vector2 GetTpButtonPosition()
    {
        if (TpButton != null && TpButton.IsValid)
        {
            var center = (Vector2) TpButton?.GetClientRectCache.Center;
            return center;
        }

        return Vector2.Zero;
    }

    public override string ToString()
    {
        string not = TpButton != null ? TpButton.Text : "not ";
        return $"{PlayerName}, current zone: {ZoneName}, and does {not}have tp button";
    }
}