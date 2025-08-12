using ExileCore2;

using static Copilot.Copilot;

namespace Copilot.Api;

public static class State
{
    private static GameController GameController => Main.GameController;

    public static bool IsLoading => GameController.IsLoading;

    // Area
    public static AreaController Area => GameController.Area;

    public static AreaInstance CurrentArea => Area.CurrentArea;

    public static string AreaName => Area.CurrentArea.Name;

    public static bool IsTown => CurrentArea.IsTown;

    public static bool IsHideout => CurrentArea.IsHideout;

    public static bool IsEscapeState => Area.TheGameState.IsEscapeState;
}
