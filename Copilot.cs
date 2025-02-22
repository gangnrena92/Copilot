using System;
using System.Linq;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;

//TODO: setting for focus on picking or following (task method??)
//TODO: sometimes when changing world it just gets stuck (not sure if it's fixed)

namespace Copilot
{
    public class Copilot : BaseSettingsPlugin<CopilotSettings>
    {
        private Entity _followTarget;
        private DateTime _nextAllowedActionTime = DateTime.Now; // Cooldown timer
        private DateTime _nextAllowedBlinkTime = DateTime.Now;
        private Vector3 lastTargetPosition = Vector3.Zero;

        public override bool Initialise()
        {
            // Initialize plugin
            Name = "Copilot";
            lastTargetPosition = Vector3.Zero;
            return base.Initialise();
        }

        public override void Render()
        {
            if (!GameController.Window.IsForeground()) return;

            // Handle pause/unpause toggle
            if (Settings.TogglePauseHotkey.PressedOnce()) Settings.IsPaused.Value = !Settings.IsPaused.Value;

            // If paused, disabled, or not ready for the next action, do nothing
            if (!Settings.Enable.Value || Settings.IsPaused.Value || DateTime.Now < _nextAllowedActionTime || GameController.Player == null || !GameController.Player.IsAlive || GameController.IsLoading) return;

            try
            {
                // Check if there are any UI elements blocking the player
                var checkpoint = GameController.IngameState.UIRoot?.Children?[1]?.Children?[64];
                var market = GameController.IngameState.UIRoot?.Children?[1]?.Children?[27];
                var leftPanel = GameController.IngameState.IngameUi.OpenLeftPanel;
                var rightPanel = GameController.IngameState.IngameUi.OpenRightPanel;
                var worldMap = GameController.IngameState.IngameUi.WorldMap;
                var npcDialog = GameController.IngameState.IngameUi.NpcDialog;
                if ((checkpoint?.IsVisible != null && (bool)checkpoint?.IsVisible) ||
                    (leftPanel?.IsVisible != null && (bool)leftPanel?.IsVisible) ||
                    (rightPanel?.IsVisible != null && (bool)rightPanel?.IsVisible) ||
                    (worldMap?.IsVisible != null && (bool)worldMap?.IsVisible) ||
                    (npcDialog?.IsVisible != null && (bool)npcDialog?.IsVisible) ||
                    (market?.IsVisible != null && (bool)market?.IsVisible))
                {
                    Keyboard.KeyDown(Keys.Space);
                    Keyboard.KeyUp(Keys.Space);
                    _nextAllowedActionTime = DateTime.Now.AddMilliseconds(Settings.ActionCooldown.Value);
                    return;
                }

                // TODO: handle picking up items??

                FollowTarget();
            } 
            catch (Exception) { /* Handle exceptions silently */ }
        }

        public override void AreaChange(AreaInstance area)
        {
            base.AreaChange(area);
            lastTargetPosition = Vector3.Zero;
            _followTarget = null;
        }

        private void FollowTarget()
        {
            try
            {
                _followTarget = GetFollowingTarget();
                var leaderPE = GetLeaderPartyElement();
                var myPos = GameController.Player.Pos;

                if (_followTarget == null && !leaderPE.ZoneName.Equals(GameController?.Area?.CurrentArea?.DisplayName)) {
                    // distance between lastTarget and myPost, int rounded
                    var roundedDiff = (int) Math.Round(Vector3.Distance(lastTargetPosition, myPos));
                    if (lastTargetPosition != Vector3.Zero && roundedDiff <= 10) {
                        MoveToward(lastTargetPosition);
                        _nextAllowedActionTime = DateTime.Now.AddMilliseconds(1000);
                        return;
                    }

                    var portal = GetBestPortalLabel(leaderPE);
                    var distanceToPortal = portal != null ? Vector3.Distance(myPos, portal.ItemOnGround.Pos) : 501;
                    var isInTown = GameController.Area.CurrentArea.IsTown;

                    if (!isInTown && distanceToPortal <= 500) {
                        var screenPos = GameController.IngameState.Camera.WorldToScreen(portal.ItemOnGround.Pos);
                        var screenPoint = new Point((int)screenPos.X, (int)screenPos.Y);
                        Mouse.SetCursorPosition(screenPoint);
                        Thread.Sleep(300);
                        Mouse.LeftClick(screenPoint);
                        if (leaderPE?.TpButton != null && GetTpConfirmation() != null)
                        {
                            Keyboard.KeyDown(Keys.Space);
                            Keyboard.KeyUp(Keys.Space);
                        }
                    } else if (leaderPE?.TpButton != null) {
                        var screenPoint = GetTpButton(leaderPE);
                        Mouse.SetCursorPosition(screenPoint);
                        Thread.Sleep(300);
                        Mouse.LeftClick(screenPoint);

                        if (leaderPE?.TpButton != null)
                        { // check if the tp confirmation is open
                            var tpConfirmation = GetTpConfirmation();
                            if (tpConfirmation != null)
                            {
                                screenPoint = new Point((int)tpConfirmation.GetClientRectCache.Center.X, (int)tpConfirmation.GetClientRectCache.Center.Y);
                                Mouse.SetCursorPosition(screenPoint);
                                Thread.Sleep(300);
                                Mouse.LeftClick(screenPoint);
                            }
                        }
                    }
                    _nextAllowedActionTime = DateTime.Now.AddMilliseconds(1000);
                    return;
                }

                // Check distance to target
                if (_followTarget == null) return;
                var targetPos = _followTarget.Pos;
                if (lastTargetPosition == Vector3.Zero) lastTargetPosition = targetPos;
                var distanceToTarget = Vector3.Distance(myPos, targetPos);

                // If within the follow distance, skip actions
                if (distanceToTarget <= Settings.FollowDistance.Value || distanceToTarget <= Settings.IdleDistance.Value) return;

                // check if is possible to move to the target but the tp confirmation is open
                if (leaderPE?.TpButton != null && GetTpConfirmation() != null)
                {
                    Keyboard.KeyDown(Keys.Space);
                    Keyboard.KeyUp(Keys.Space);
                }

                // check if the distance of the target changed significantly from the last position
                if (distanceToTarget > 4000) {
                    var portal = GetBestPortalLabel(leaderPE);
                    if(portal != null) {
                        var screenPos = GameController.IngameState.Camera.WorldToScreen(portal.ItemOnGround.Pos);
                        var screenPoint = new Point((int)screenPos.X, (int)screenPos.Y);
                        Mouse.LeftClick(screenPoint);
                        _nextAllowedActionTime = DateTime.Now.AddMilliseconds(500);
                        return;
                    }
                } else if (distanceToTarget > Settings.BlinkRange.Value) {
                    // use blink if the distance is too far
                    if (Settings.UseBlink.Value && DateTime.Now > _nextAllowedBlinkTime)
                    {
                        Keyboard.KeyDown(Keys.Space);
                        Keyboard.KeyUp(Keys.Space);
                        _nextAllowedBlinkTime = DateTime.Now.AddMilliseconds(Settings.BlinkCooldown.Value);
                    }
                }

                // If outside the follow distance, move toward the target
                MoveToward(targetPos);

                // Set the cooldown for the next allowed action
                _nextAllowedActionTime = DateTime.Now.AddMilliseconds(Settings.ActionCooldown.Value);
            }
            catch (Exception) { /* Handle exceptions silently */ }
        }

        private Entity GetFollowingTarget()
        {
            try
            {
                var leaderName = Settings.TargetPlayerName.Value.ToLower();
                return GameController.Entities
                    .Where(e => e.Type == ExileCore2.Shared.Enums.EntityType.Player)
                    .FirstOrDefault(e => e.GetComponent<Player>().PlayerName.ToLower() == leaderName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private PartyElementWindow GetLeaderPartyElement()
        {
            try
            {
                var partyElementList = GameController?.IngameState?.IngameUi?.PartyElement.Children?[0]?.Children;
                var leader = partyElementList?.FirstOrDefault(partyElement => partyElement?.Children?[0]?.Children?[0]?.Text?.ToLower() == Settings.TargetPlayerName.Value.ToLower());
                var leaderPartyElement = new PartyElementWindow
                {
                    PlayerName = leader.Children?[0]?.Children?[0]?.Text,
                    TpButton = leader?.Children?[leader.ChildCount == 4 ? 3 : 2],
                    ZoneName = leader?.Children?.Count == 4 ? leader.Children[2].Text : GameController.Area.CurrentArea.DisplayName
                };
                LogMessage("Leader party element: " + leaderPartyElement);
                return leaderPartyElement;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private LabelOnGround GetBestPortalLabel(PartyElementWindow leaderPE)
        {
            try
            {
                var currentZoneName = GameController.Area.CurrentArea.DisplayName;
                // if (leaderPE.ZoneName.Equals(currentZoneName)) return null;
                var portalLabels =
                    GameController?.Game?.IngameState?.IngameUi?.ItemsOnGroundLabelsVisible?.Where(x => x.ItemOnGround.Metadata.ToLower().Contains("areatransition") || x.ItemOnGround.Metadata.ToLower().Contains("portal"))
                        .OrderBy(x => Vector3.Distance(lastTargetPosition, x.ItemOnGround.Pos)).ToList();

                var random = new Random();

                return GameController?.Area?.CurrentArea?.IsHideout != null && (bool) GameController?.Area?.CurrentArea?.IsHideout
                    ? portalLabels?[random.Next(portalLabels.Count)]
                    : portalLabels?.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private Point GetTpButton(PartyElementWindow leaderPE)
        {
            try
            {
                var windowOffset = GameController.Window.GetWindowRectangle().TopLeft;
                var elemCenter = (Vector2) leaderPE?.TpButton?.GetClientRectCache.Center;
                var finalPos = new Point((int) (elemCenter.X + windowOffset.X), (int) (elemCenter.Y + windowOffset.Y));

                return finalPos;
            }
            catch
            {
                return Point.Empty;
            }
        }

        private Element GetTpConfirmation()
        {
            try
            {
                var ui = GameController?.IngameState?.IngameUi?.PopUpWindow.Children[0].Children[0];

                if (ui.Children[0].Text.Equals("Are you sure you want to teleport to this player's location?"))
                    return ui.Children[3].Children[0];

                return null;
            }
            catch
            {
                return null;
            }
        }

        private void MoveToward(Vector3 targetPos)
        {
            try
            {
                var screenPos = GameController.IngameState.Camera.WorldToScreen(targetPos);
                var screenPoint = new Point((int)screenPos.X, (int)screenPos.Y);

                Mouse.SetCursorPosition(screenPoint);
                Mouse.LeftClick(screenPoint);
                lastTargetPosition = targetPos;
            }
            catch (Exception) { /* Handle exceptions silently */ }
        }
    }
}

public class PartyElementWindow
{
    public string PlayerName { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public Element TpButton { get; set; } = new Element();

    public override string ToString()
    {
        return $"{PlayerName}, current zone: {ZoneName}";
    }
}