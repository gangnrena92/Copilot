using System;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using System.Threading.Tasks;

using ExileCore2;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.PoEMemory.Components;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;

using static Copilot.Copilot;
using static Copilot.Utils.Ui;
using Copilot.Utils;
using Copilot.Settings;
using Copilot.Classes;

namespace Copilot.CoRoutines;
internal class FollowCoRoutine
{
    private static Entity target => Main._followTarget;
    private static Vector3 PlayerPos => Main.PlayerPos;
    private static CopilotSettings Settings => Main.Settings;
    private static float DistanceToTarget => Vector3.Distance(PlayerPos, target.Pos);
    private static Vector3 lastTargetPosition = Vector3.Zero;

    private static LoggerPlus Log = new LoggerPlus("FollowCoRoutine");

    public static void Init()
    {
        TaskRunner.Run(Follow_Task, "FollowCoRoutine");
    }

    public static void Stop()
    {
        TaskRunner.Stop("FollowCoRoutine");
    }

    public static async SyncTask<bool> Follow_Task()
    {
        while (true)
        {
            await Task.Delay(Settings.ActionCooldown.Value);

            // If paused, disabled, or not ready for the next action, do nothing
            if (Ui.GameController.Player == null || Ui.GameController.IsLoading) continue;

            Main._followTarget = GetFollowingTarget();
            var leaderPE = GetLeaderPartyElement();

            // If the target is not found, or the player is not in the same zone
            if (target == null)
            {
                if (!leaderPE.ZoneName.Equals(CurrentArea.DisplayName))
                    await FollowUsingPortalOrTpButton(leaderPE);
                continue;
            }
            if (CurrentArea.IsTown || (CurrentArea.IsHideout && Ui.InventoryList.Count != 0)) continue;

            // If within the follow distance, do nothing
            if (DistanceToTarget <= Settings.FollowDistance.Value) continue;

            if (lastTargetPosition == Vector3.Zero) lastTargetPosition = target.Pos;

            // check if the distance of the target changed significantly from the last position OR if there is a boss near and the distance is less than 2000
            if (DistanceToTarget > 3000)
            {
                var portal = GetBestPortalLabel();
                await SyncInput.LClick(Ui.Camera.WorldToScreen(portal.ItemOnGround.Pos), 300);
            }
            else
            {
                await MoveToward(target.Pos);
                Main.AllowBlinkTask = true;
            }
        }
    }

    private static Entity GetFollowingTarget()
    {
        try
        {
            var leaderName = Settings.TargetPlayerName.Value.ToLower();
            var target = Ui.GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Player]
                .FirstOrDefault(x => string.Equals(x.GetComponent<Player>()?.PlayerName.ToLower(), leaderName, StringComparison.OrdinalIgnoreCase));
            return target;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static PartyElement GetLeaderPartyElement()
    {
        try
        {
            var partyElementList = IngameUi.PartyElement.Children?[0]?.Children;
            var leader = partyElementList?.FirstOrDefault(partyElement => partyElement?.Children?[0]?.Children?[0]?.Text?.ToLower() == Settings.TargetPlayerName.Value.ToLower());
            var leaderPartyElement = new PartyElement
            {
                PlayerName = leader.Children?[0]?.Children?[0]?.Text,
                TpButton = leader?.Children?[4],
                ZoneName = leader.Children[3].Text ?? CurrentArea.DisplayName
            };
            return leaderPartyElement;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async SyncTask<bool> FollowUsingPortalOrTpButton(PartyElement leaderPE)
    {
        var portal = GetBestPortalLabel();
        const int threshold = 1000;
        var distanceToPortal = portal != null ? Vector3.Distance(PlayerPos, portal.ItemOnGround.Pos) : threshold + 1;
        if ((CurrentArea.IsHideout ||
                (CurrentArea.Name.Equals("The Temple of Chaos") && leaderPE.ZoneName.Equals("The Trial of Chaos")
            )) && distanceToPortal <= threshold)
        { // if in hideout and near the portal
            await SyncInput.LClick(Ui.Camera.WorldToScreen(portal.ItemOnGround.Pos));
            if (leaderPE?.TpButton != null && GetTpConfirmation() != null) await SyncInput.PressKey(Keys.Escape);
        }
        else if (leaderPE?.TpButton != null)
        {
            await SyncInput.LClick(leaderPE.GetTpButtonPosition());

            if (leaderPE.TpButton != null)
            { // check if the tp confirmation is open
                var tpConfirmation = GetTpConfirmation();
                if (tpConfirmation != null) await SyncInput.LClick(tpConfirmation.GetClientRectCache.Center, 500);
            }
        }
        return true;
    }

    private static LabelOnGround GetBestPortalLabel()
    {
        try
        {
            var portalLabels =
                IngameUi.ItemsOnGroundLabelsVisible?.Where(
                        x => x.ItemOnGround.Metadata.ToLower().Contains("areatransition")
                            || x.ItemOnGround.Metadata.ToLower().Contains("portal")
                            || x.ItemOnGround.Metadata.ToLower().EndsWith("ultimatumentrance")
                        )
                    .OrderBy(x => Vector3.Distance(lastTargetPosition, x.ItemOnGround.Pos)).ToList();

            var random = new Random();

            return CurrentArea.IsHideout
                ? portalLabels?[random.Next(portalLabels.Count)]
                : portalLabels?.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static async SyncTask<bool> MoveToward(Vector3 targetPos)
    {
        var screenPos = Ui.Camera.WorldToScreen(targetPos);

        if (Settings.Additional.UseMouse.Value)
        {
            await SyncInput.LClick(screenPos, 20);
        }
        else
        {
            Input.SetCursorPos(screenPos);
            Input.MouseMove();
            await SyncInput.PressKey(Keys.T);
        }
        lastTargetPosition = targetPos;
        return true;
    }
}