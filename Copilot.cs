using System;
using System.Linq;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

using ExileCore2;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;

using Copilot.Utils;
using Copilot.Settings;
using Copilot.Classes;

// TODO: ghost follow
// - debug to see like a crosshair
// TODO: circle people
// TODO: pots
// TODO: fix AreaTransition
// TODO: better pickup filter make it able to use regex
// TODO: Threads - this will be a big one

namespace Copilot;
public class Copilot : BaseSettingsPlugin<CopilotSettings>
{
    public static Copilot Main;

    private LoggerPlus Log => new LoggerPlus("Core");

    private Entity _followTarget;
    private DateTime _nextAllowedActionTime = DateTime.Now; // Cooldown timer
    private DateTime _nextAllowedBlinkTime = DateTime.Now;
    private DateTime _nextAllowedShockTime = DateTime.Now;
    private Vector3 lastTargetPosition = Vector3.Zero;

    private Vector3 PlayerPos => GameController.Player.Pos;

    public override bool Initialise()
    {
        Main = this;
        Name = "Copilot";
        lastTargetPosition = Vector3.Zero;
        return base.Initialise();
    }

    public override void DrawSettings()
    {
        CopilotSettingsHandler.DrawCustomSettings();
        base.DrawSettings();
    }

    public override void AreaChange(AreaInstance area)
    {
        lastTargetPosition = Vector3.Zero;
        _followTarget = null;
        base.AreaChange(area);
    }

    public override void Render()
    {
        if (!GameController.Window.IsForeground()) return;

        // Handle pause/unpause toggle
        if (Settings.TogglePauseHotkey.PressedOnce()) Settings.IsFollowing.Value = !Settings.IsFollowing.Value;

        // If paused, disabled, or not ready for the next action, do nothing
        if (!Settings.Enable.Value || !Settings.IsFollowing.Value || DateTime.Now < _nextAllowedActionTime || GameController.Player == null || GameController.IsLoading) return;
        Log.Message($"Is currently following");

        try
        {
            if (Settings.Dumper.Enable.Value && Ui.CurrentArea.IsHideout && DumpEverything()) return;

            if (Ui.IsAnyUiOpen()) {
                Keyboard.KeyPress(Keys.Space);
                _nextAllowedActionTime = DateTime.Now.AddMilliseconds(Settings.ActionCooldown.Value);
                return;
            }

            var resurrectPanel = Ui.IngameUi.ResurrectPanel;
            if (resurrectPanel != null && resurrectPanel.IsVisible) {
                var inTown = resurrectPanel?.ResurrectInTown;
                var atCheckpoint = resurrectPanel?.ResurrectAtCheckpoint;
                var btn = atCheckpoint ?? inTown; // if inTown is null, use atCheckpoint
                if (btn != null && btn.IsVisible) {
                    var screenPoint = new Point((int)btn.GetClientRectCache.Center.X, (int)btn.GetClientRectCache.Center.Y);
                    Mouse.LeftClick(screenPoint, 300);
                }
                _nextAllowedActionTime = DateTime.Now.AddMilliseconds(1000);
                return;
            }

            FollowTarget();
        } 
        catch (Exception) { /* Handle exceptions silently */ }
    }

    private void FollowTarget()
    {
        try
        {
            _followTarget = GetFollowingTarget();
            var leaderPE = GetLeaderPartyElement();

            // If the target is not found, or the player is not in the same zone
            if (_followTarget == null) {
                if (!leaderPE.ZoneName.Equals(Ui.CurrentArea.DisplayName))
                    FollowUsingPortalOrTpButton(leaderPE);
                return;
            }
            if (Ui.CurrentArea.IsTown) return;

            var targetPos = _followTarget.Pos;
            if (lastTargetPosition == Vector3.Zero) lastTargetPosition = targetPos;
            var distanceToTarget = Vector3.Distance(PlayerPos, targetPos);

            //* Shock Bot
            if (Settings.ShockBot.Enable.Value && DateTime.Now > _nextAllowedShockTime && ShockBotCode()) return;

            //* Pickup
            if (Settings.Pickup.Enable.Value && distanceToTarget <= Settings.Pickup.RangeToIgnore.Value && PickUpItem()) return;

            // If within the follow distance, do nothing
            if (distanceToTarget <= Settings.FollowDistance.Value) return;

            // Quick check for "bugged" (shouldn't be open) confirmation tp
            if (leaderPE?.TpButton != null && Ui.GetTpConfirmation() != null) Keyboard.KeyPress(Keys.Escape);

            // check if there is areatransition in the area and boss
            // var thereIsBossNear = GameController.Entities.Any(e => e.Type == EntityType.Monster && e.IsAlive && e.Rarity == MonsterRarity.Unique && Vector3.Distance(myPos, e.Pos) < 2000);

            // check if the distance of the target changed significantly from the last position OR if there is a boss near and the distance is less than 2000
            if (distanceToTarget > 3000 /* || (thereIsBossNear && distanceToTarget < 2000) */) // TODO: fix this arena
            {
                var portal = GetBestPortalLabel();
                ClickLabel(portal, 300);
                _nextAllowedActionTime = DateTime.Now.AddMilliseconds(1000);
                return;
            }
            else if (Settings.Blink.Enable.Value && DateTime.Now > _nextAllowedBlinkTime && distanceToTarget > Settings.Blink.Range.Value)
            {
                MoveToward(targetPos);
                Thread.Sleep(50);
                Keyboard.KeyPress(Keys.Space);
                _nextAllowedBlinkTime = DateTime.Now.AddMilliseconds(Settings.Blink.Cooldown.Value);
            }
            else
            {
                MoveToward(targetPos);
            }

            // Set the cooldown for the next allowed action
            _nextAllowedActionTime = DateTime.Now.AddMilliseconds(Settings.ActionCooldown.Value);
        }
        catch (Exception) { /* Handle exceptions silently */ }
    }

    private void FollowUsingPortalOrTpButton(PartyElement leaderPE)
    {
        try
        {
            var portal = GetBestPortalLabel();
            const int threshold = 1000;
            var distanceToPortal = portal != null ? Vector3.Distance(PlayerPos, portal.ItemOnGround.Pos) : threshold + 1;
            if (
                (Ui.CurrentArea.IsHideout ||
                    (Ui.CurrentArea.Name.Equals("The Temple of Chaos") && leaderPE.ZoneName.Equals("The Trial of Chaos")
                )) && distanceToPortal <= threshold)
            { // if in hideout and near the portal
                var screenPos = Ui.Camera.WorldToScreen(portal.ItemOnGround.Pos);
                var screenPoint = new Point((int)screenPos.X, (int)screenPos.Y);
                Mouse.LeftClick(screenPoint, 500);
                if (leaderPE?.TpButton != null && Ui.GetTpConfirmation() != null) Keyboard.KeyPress(Keys.Escape);
            }
            else if (leaderPE?.TpButton != null)
            {
                // var screenPoint = GetTpButton(leaderPE);
                var screenPoint = Ui.AdjustPosition(leaderPE.GetTpButtonPosition(), "TopLeft");
                Mouse.LeftClick(screenPoint, 100);

                if (leaderPE.TpButton != null)
                { // check if the tp confirmation is open
                    var tpConfirmation = Ui.GetTpConfirmation();
                    if (tpConfirmation != null)
                    {
                        screenPoint = new Point((int)tpConfirmation.GetClientRectCache.Center.X, (int)tpConfirmation.GetClientRectCache.Center.Y);
                        Mouse.LeftClick(screenPoint, 100);
                    }
                }
            }
            _nextAllowedActionTime = DateTime.Now.AddMilliseconds(500);
        }
        catch (Exception) { /* Handle exceptions silently */ }
    }

    private Vector2 WindowOffset => GameController.Window.GetWindowRectangleTimeCache.TopLeft;
    private bool DumpEverything()
    {
        var inventoryItems = Ui.InventoryList;
        if (inventoryItems.Count == 0) return false; // should continue
        Log.Message($"Dumping {inventoryItems.Count} items to the guild stash");

        var stash = Ui.IngameUi.ItemsOnGroundLabelsVisible
            .Where(e => e.ItemOnGround.Metadata.ToLower().Contains("guildstash"))
            .FirstOrDefault();

        if (stash == null) {
            Log.Error("No guild stash found in the area.");
            return false; // should continue
        }

        ClickLabel(stash, 1000);

        // TODO: go to the correct tab

        Log.Message($"Inventory items count: {inventoryItems.Count}");
        foreach (var item in inventoryItems)
        {
            Keyboard.KeyDown(Keys.ControlKey);
            var pos = item.GetClientRect().Center + WindowOffset;
            Mouse.LeftClick(new Point((int)pos.X, (int)pos.Y), 20);
            Thread.Sleep(Settings.Dumper.ClickDelay.Value);
            Keyboard.KeyUp(Keys.ControlKey);
        }

        return true; // shouldn't continue
    }

    private bool ShockBotCode()
    {
        var monster = Ui.EntityList
            .Where(e => e.Type == EntityType.Monster && e.IsAlive && (e.Rarity == MonsterRarity.Rare || e.Rarity == MonsterRarity.Unique))
            .OrderBy(e => Vector3.Distance(PlayerPos, e.Pos))
            .FirstOrDefault();
        if (monster != null)
        {
            var distanceToMonster = Vector3.Distance(PlayerPos, monster.Pos);
            if (distanceToMonster <= Settings.ShockBot.Range)
            {
                var screenPos = Ui.Camera.WorldToScreen(monster.Pos);
                var screenPoint = new Point((int)screenPos.X, (int)screenPos.Y);
                Mouse.SetCursorPosition(screenPoint);
                Thread.Sleep(100);

                Keyboard.KeyPress(Settings.ShockBot.BallLightningKey.Value);

                // start tracking the balls
                var ball = Ui.EntityList
                    .Where(e => e.IsDead && e.Metadata == "Metadata/Projectiles/BallLightningPlayer")
                    .OrderBy(e => Vector3.Distance(monster.Pos, e.Pos))
                    .FirstOrDefault();

                if (ball != null && Vector3.Distance(monster.Pos, ball.Pos) <= Settings.ShockBot.RangeToUseLightningWarp.Value)
                {
                    var ballScreenPos = Ui.Camera.WorldToScreen(ball.Pos);
                    var ballScreenPoint = new Point((int)ballScreenPos.X, (int)ballScreenPos.Y);
                    Mouse.SetCursorPosition(ballScreenPoint);
                    Thread.Sleep(100);
                    Keyboard.KeyPress(Settings.ShockBot.LightningWarpKey.Value);
                }

                _nextAllowedActionTime = DateTime.Now.AddMilliseconds(Settings.ActionCooldown.Value);
                _nextAllowedShockTime = DateTime.Now.AddMilliseconds(Settings.ShockBot.ActionCooldown.Value);
                return true;
            }
        }
        _nextAllowedShockTime = DateTime.Now.AddMilliseconds(Settings.ShockBot.ActionCooldown.Value);
        return false;
    }

    private bool PickUpItem()
    {
        var pos = Settings.Pickup.UseTargetPosition.Value ? _followTarget.Pos : PlayerPos;
        try
        {
            var items = Ui.IngameUi.ItemsOnGroundLabelsVisible;
            if (items != null)
            {
                var filteredItems = Settings.Pickup.Filter.Value.Split(',');
                var item = items?
                    .OrderBy(x => Vector3.Distance(pos, x.ItemOnGround.Pos))
                    .FirstOrDefault(x => filteredItems.Any(y => x.Label.Text != null && x.Label.Text.Contains(y)));
                if (item == null) return false;

                var distanceToItem = Vector3.Distance(pos, item.ItemOnGround.Pos);
                if (distanceToItem <= Settings.Pickup.Range.Value)
                {
                    ClickLabel(item, 50);
                    _nextAllowedActionTime = DateTime.Now.AddMilliseconds(Settings.ActionCooldown.Value);
                    return true;
                }
            }
        }
        catch (Exception) { /* Handle exceptions silently */ }
        return false;
    }

    private Entity GetFollowingTarget()
    {
        try
        {
            var leaderName = Settings.TargetPlayerName.Value.ToLower();
            var target = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Player].FirstOrDefault(x => string.Equals(x.GetComponent<Player>()?.PlayerName.ToLower(), leaderName, StringComparison.OrdinalIgnoreCase));
            return target;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private PartyElement GetLeaderPartyElement()
    {
        try
        {
            var partyElementList = Ui.IngameUi.PartyElement.Children?[0]?.Children;
            var leader = partyElementList?.FirstOrDefault(partyElement => partyElement?.Children?[0]?.Children?[0]?.Text?.ToLower() == Settings.TargetPlayerName.Value.ToLower());
            var leaderPartyElement = new PartyElement
            {
                PlayerName = leader.Children?[0]?.Children?[0]?.Text,
                TpButton = leader?.Children?[4],
                ZoneName = leader.Children[3].Text ?? Ui.CurrentArea.DisplayName
            };
            return leaderPartyElement;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private LabelOnGround GetBestPortalLabel()
    {
        try
        {
            var portalLabels =
                Ui.IngameUi.ItemsOnGroundLabelsVisible?.Where(
                        x => x.ItemOnGround.Metadata.ToLower().Contains("areatransition")
                            || x.ItemOnGround.Metadata.ToLower().Contains("portal")
                            || x.ItemOnGround.Metadata.ToLower().EndsWith("ultimatumentrance")
                        )
                    .OrderBy(x => Vector3.Distance(lastTargetPosition, x.ItemOnGround.Pos)).ToList();

            var random = new Random();

            return Ui.CurrentArea?.IsHideout != null && Ui.CurrentArea.IsHideout
                ? portalLabels?[random.Next(portalLabels.Count)]
                : portalLabels?.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private void ClickLabel(LabelOnGround label, int delay = 300)
    {
        try
        {
            var screenPos = Ui.Camera.WorldToScreen(label.ItemOnGround.Pos);
            var screenPoint = new Point((int)screenPos.X, (int)screenPos.Y);
            Mouse.LeftClick(screenPoint, delay);
        }
        catch (Exception) { /* Handle exceptions silently */ }
    }

    private void MoveToward(Vector3 targetPos)
    {
        try
        {
            var screenPos = Ui.Camera.WorldToScreen(targetPos);
            var screenPoint = new Point((int)screenPos.X, (int)screenPos.Y);

            Mouse.SetCursorPosition(screenPoint);
            if (Settings.Additional.UseMouse.Value)
                Mouse.LeftClick(screenPoint, 20);
            else
                Keyboard.KeyPress(Keys.T);
            lastTargetPosition = targetPos;
        }
        catch (Exception) { /* Handle exceptions silently */ }
    }
}
