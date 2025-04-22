using System;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using System.Threading.Tasks;

using ExileCore2.PoEMemory.Elements;
using ExileCore2.Shared;

using static Copilot.Copilot;
using static Copilot.Api.Ui;
using Copilot.Api;
using Copilot.Utils;
using Copilot.Settings;
using Copilot.Classes;

namespace Copilot.CoRoutines;

internal class FollowCoRoutine
{
    private static CopilotSettings Settings => Main.Settings;

    private static LoggerPlus Log = new LoggerPlus("FollowCoRoutine");

    private static Vector3 lastTargetPosition = Vector3.Zero;

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
            await Task.Delay(Settings.ActionCooldown);

            // If paused, disabled, or not ready for the next action, do nothing
            if (_player == null || State.IsLoading || Ui.ResurrectPanel.IsVisible) continue;

            var leaderPE = GetLeaderPartyElement();

            // If the target is not found, or the player is not in the same zone
            if (_target == null)
            {
                Log.Message("Target not found, trying to follow the leader...");
                if (leaderPE != null && !leaderPE.ZoneName.Equals(State.AreaName))
                    await FollowUsingPortalOrTpButton(leaderPE);
                continue;
            }

            if (State.IsTown || (State.IsHideout && Settings.Tasks.IsDumperEnabled && Api.Inventory.Items.Count != 0)) continue;
            var distanceToTarget = _player.DistanceTo(_target.Entity);

            // If within the follow distance, do nothing
            if (distanceToTarget <= Settings.FollowDistance) continue;

            if (lastTargetPosition == Vector3.Zero) lastTargetPosition = _target.Pos;

            // check if the distance of the target changed significantly from the last position OR if there is a boss near and the distance is less than 2000
            if (distanceToTarget > 3000)
            {
                var portal = GetBestPortalLabel();
                if (portal == null) continue;
                await SyncInput.LClick(portal.ItemOnGround, 300);
            }
            else
            {
                await MoveToward();
                Main.AllowBlinkTask = true;
            }
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
                ZoneName = leader.Children[3].Text ?? State.AreaName
            };
            return leaderPartyElement;
        }
        catch
        {
            return null;
        }
    }

    private static async SyncTask<bool> FollowUsingPortalOrTpButton(PartyElement leaderPE)
    {
        var allowedToUsePortalAreas = new[] {
            "The Temple of Chaos",
            "The Trial of Chaos",
            "The Halani Gates"
        };

        try
        {
            var portal = GetBestPortalLabel();
            const int threshold = 1000;
            var distanceToPortal = portal != null ? _player.DistanceTo(portal.ItemOnGround) : threshold + 1;
            if ((State.IsHideout || allowedToUsePortalAreas.Contains(State.AreaName)) && distanceToPortal <= threshold)
            { // if in hideout and near the portal
                await SyncInput.LClick(portal.ItemOnGround, 10);
                if (leaderPE?.TpButton != null && GetTpConfirmation() != null) await SyncInput.PressKey(Keys.Escape);
            }
            else if (leaderPE?.TpButton != null)
            {
                await SyncInput.LClick(leaderPE.GetTpButtonPosition(), 10);

                if (leaderPE.TpButton != null)
                { // check if the tp confirmation is open
                    var tpConfirmation = GetTpConfirmation();
                    if (tpConfirmation != null) await SyncInput.LClick(tpConfirmation.GetClientRectCache.Center, 500);
                }
                await Task.Delay(1000);
            }
            else
            {
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Error in FollowUsingPortalOrTpButton: {e.Message}");
            return false;
        }
    }

    private static LabelOnGround GetBestPortalLabel()
    {
        var validLabels = new[] { "portal", "areatransition", "ultimatumentrance", "bosstransition" };
        try
        {
            var portalLabels =
                IngameUi.ItemsOnGroundLabelsVisible?
                    .Where(x => validLabels.Any(label => x.ItemOnGround.Metadata.ToLower().Contains(label)))
                    .OrderBy(x => Vector3.Distance(lastTargetPosition, x.ItemOnGround.Pos)).ToList();

            var random = new Random();

            return State.IsHideout
                ? portalLabels?[random.Next(portalLabels.Count)]
                : portalLabels?.FirstOrDefault();
        }
        catch (Exception e)
        {
            Log.Error("Error in GetBestPortalLabel: " + e.Message);
            return null;
        }
    }

    private static async SyncTask<bool> MoveToward()
    {
        if (Settings.Additional.UseMouse)
        {
            await SyncInput.LClick(_target, 20);
        }
        else
        {
            await SyncInput.MoveMouse(_target, 10);
            await SyncInput.PressKey(Keys.T);
        }
        lastTargetPosition = _target.Pos;
        return true;
    }
}