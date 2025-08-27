using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

using ExileCore2.PoEMemory.Elements;
using ExileCore2.Shared;

using Copilot.Settings;
using Copilot.Utils;
using Copilot.Classes;
using Copilot.Api;

using static Copilot.Copilot;
using static Copilot.Api.Ui;

namespace Copilot.CoRoutines
{
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

        public static async Task<bool> Follow_Task()
        {
            while (true)
            {
                await Task.Delay(Settings.ActionCooldown);

                try
                {
                    if (_player == null || State.IsLoading || Ui.ResurrectPanel.IsVisible || DontFollow)
                        continue;

                    var leaderPE = GetLeaderPartyElement();

                    if (_target == null)
                    {
                        Log.Message("Target not found, trying to follow the leader...");
                        if (leaderPE != null)
                        {
                            if (!leaderPE.ZoneName.Equals(State.AreaName))
                                await FollowUsingPortalOrTpButton(leaderPE);
                            else
                                Main.TpTries = 0;
                        }
                        continue;
                    }

                    if (State.IsTown || (State.IsHideout && Settings.Tasks.IsDumperEnabled && Api.Inventory.Items.Count != 0))
                        continue;

                    var distanceToTarget = _player.DistanceTo(_target.Entity);

                    if (distanceToTarget <= Settings.FollowDistance)
                        continue;

                    if (lastTargetPosition == Vector3.Zero) lastTargetPosition = _target.Pos;

                    if (distanceToTarget > 3000 && !Main.RessurectedRecently)
                    {
                        var portal = GetBestPortalLabel();
                        if (portal == null) continue;
                        await SyncInput.LClick(portal.ItemOnGround, 300);
                    }
                    else
                    {
                        if (Main.RessurectedRecently)
                        {
                            if (distanceToTarget < 600)
                                Main.RessurectedRecently = false;
                            continue;
                        }

                        await MoveToward();
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Error in main loop: {e.Message}");
                    continue;
                }
            }
        }

        private static PartyElement GetLeaderPartyElement()
        {
            try
            {
                var partyElementList = IngameUi.PartyElement.Children?[0]?.Children;
                var leader = partyElementList?.FirstOrDefault(partyElement =>
                    partyElement?.Children?[0]?.Children?[0]?.Text?.ToLower() ==
                    Settings.TargetPlayerName.Value.ToLower());

                var leaderPartyElement = new PartyElement
                {
                    PlayerName = leader?.Children?[0]?.Children?[0]?.Text,
                    TpButton = leader?.Children?[4],
                    ZoneName = leader?.Children?[3]?.Text ?? State.AreaName
                };
                return leaderPartyElement;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<bool> FollowUsingPortalOrTpButton(PartyElement leaderPE)
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
                {
                    await SyncInput.LClick(portal.ItemOnGround, 1000);
                }
                else if (leaderPE?.TpButton != null)
                {
                    if (Main.TpTries++ > 3) return false;

                    await SyncInput.LClick(leaderPE.GetTpButtonPosition(), 10);

                    if (leaderPE.TpButton != null)
                    {
                        var tpConfirmation = GetTpConfirmation();
                        if (tpConfirmation != null)
                            await SyncInput.LClick(tpConfirmation.GetClientRectCache.Center, 500);
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
                var portalLabel =
                    IngameUi.ItemsOnGroundLabelsVisible?
                        .Where(x => validLabels.Any(label => x.ItemOnGround.Metadata.ToLower().Contains(label)))
                        .OrderBy(x => Vector3.Distance(lastTargetPosition, x.ItemOnGround.Pos))
                        .FirstOrDefault();
                return portalLabel;
            }
            catch (Exception e)
            {
                Log.Error("Error in GetBestPortalLabel: " + e.Message);
                return null;
            }
        }

        private static async Task<bool> MoveToward()
        {
            if (Settings.Additional.MovementMode == MovementMode.Mouse)
            {
                if (Settings.Additional.UseMouse)
                    await SyncInput.LClick(_target, 20);
                else
                {
                    await SyncInput.MoveMouse(_target, 10);
                    await SyncInput.PressKey(Settings.Additional.FollowKey);
                }
            }
            else if (Settings.Additional.MovementMode == MovementMode.WASD)
            {
                while (true)
                {
                    if (_player == null || _target == null)
                        break;

                    var playerPos = _player.Pos;
                    var targetPos = _target.Pos;

                    var direction = targetPos - playerPos;
                    var distance = direction.Length();

                    if (distance < 0.5f) break;

                    double angle = Math.Atan2(direction.Y, direction.X) * (180 / Math.PI);

                    Keys key1 = Keys.None;
                    Keys key2 = Keys.None;

                    if (angle >= -22.5 && angle < 22.5) key1 = Keys.D;
                    else if (angle >= 22.5 && angle < 67.5) { key1 = Keys.W; key2 = Keys.D; }
                    else if (angle >= 67.5 && angle < 112.5) key1 = Keys.W;
                    else if (angle >= 112.5 && angle < 157.5) { key1 = Keys.W; key2 = Keys.A; }
                    else if (angle >= 157.5 || angle < -157.5) key1 = Keys.A;
                    else if (angle >= -157.5 && angle < -112.5) { key1 = Keys.S; key2 = Keys.A; }
                    else if (angle >= -112.5 && angle < -67.5) key1 = Keys.S;
                    else if (angle >= -67.5 && angle < -22.5) { key1 = Keys.S; key2 = Keys.D; }

                    var keysToPress = new List<Keys>();
                    if (key1 != Keys.None) keysToPress.Add(key1);
                    if (key2 != Keys.None) keysToPress.Add(key2);

                    foreach (var key in keysToPress)
                        Input.KeyDown(key);

                    await Task.Delay(new Random().Next(Settings.Additional.RandomDelayMin, Settings.Additional.RandomDelayMax));

                    foreach (var key in keysToPress)
                        Input.KeyUp(key);
                }
            }

            lastTargetPosition = _target.Pos;
            return true;
        }
    }
}