# Copilot
This plugin is a fork of the original [copilot](https://github.com/totalschaden/copilot), updated to work with PoE2.

Although this plugin originates from the original, it has been rewritten with changes to its logic handling.
Additionally, some features from the original have not been implemented and are not planned for future updates.

It also incorporates some base code from [FollowLad](https://github.com/AlphaCaster/FollowLad).

## Features
- [x] Follow the player
- [x] Pass through portals
- [x] Use blink to catch up (if the target gets too far away)
- [x] Close UIs (such as chests, crafting tables, etc.) if clicked accidentally
- [x] Auto-respawn (will attempt to respawn, unless another player is on the map)
- [x] Pick-up items
- [ ] Use skills

## Settings
The default settings are configured to suit **my** needs and may not work for you.
Please **do not** open an issue if the plugin isn't working due to your settings.

## Tips
- **DO NOT** use checkpoints.
- When the target passes through a portal, they should wait nearby so the bot can detect them upon loading.
- If you're moving too fast, it's your fault the bot isn't keeping up, not the plugin's.
- It's helpful to zoom out using the [WheresMyZoomAt](https://github.com/doubleespressobro/WheresMyZoomAt-PoE2) plugin. (But not too much)
- I recommend placing the Waypoint and Map Device close to each other.
- Since the bot picks up everything, it's best to use a [good loot filter](https://www.filterblade.xyz/?game=Poe2) for the pick-up items feature.

## Known bugs
- May experience issues in "ARENA".
- Please someone help me fix the snippet system... it doesn't allow me to use async/await, and I don't know how to fix it.

## Custom snippets

Example for a Curse bot.
```csharp
if (globals.Target == null) return "No target";
if (globals.Player.DistanceTo(globals.Target) > 1000) return "Too far from target"; 

var monster = Entities
  .NearbyMonsters(additionalFilters: e => e.IsDead)
  .FirstOrDefault();

if (monster == null) return "No monsters.";
if (globals.Player.DistanceTo(monster) > 1000) return "Too far";

SyncInput.MoveMouse(monster, 100);
SyncInput.KeyPress(Keys.Q);
return "Action completed!";
```

Example for a shock bot.
```csharp
var monster = Entities
  .NearbyMonsters(EntityRarity.AtLeastRare, 1000, additionalFilters: e => e.IsAlive)
  .FirstOrDefault();
if (monster == null) return "No monsters";

SyncInput.MoveMouse(monster);
SyncInput.KeyPress(Keys.Q);

// track the balls
var ball = Entities.ValidList
  .Where(e => e.IsDead && e.Metadata == "Metadata/Projectiles/BallLightningPlayer" && Vector3.Distance(e.Pos, monster.Pos) < 600)
  .OrderBy(e => Vector3.Distance(monster.Pos, e.Pos))
  .FirstOrDefault();

if (ball != null)
{
  SyncInput.MoveMouse(ball);
  SyncInput.KeyPress(Keys.W);
}
return "Action completed!";
```

## Donations

<div style="display: flex;align-items: center;justify-content: center;flex-direction: column;">
  <a href="https://www.paypal.com/donate/?hosted_button_id=NX4PVU9B2YFDU">
    <img src="./assets/donate_paypal.png" width="200">
  </a>
  <a href="https://revolut.me/curvu">
    <img src="./assets/donate_revolut.png" width="160">
  </a>
</div>