# Copilot
A plugin forked from the original [copilot](https://github.com/totalschaden/copilot), now updated to work with PoE2.

While this plugin shares its roots with the original, much of the logic has been rewritten.
Some features from the original version have been intentionally left out and are not planned for future updates.

## Features ✨
- [x] Fully coroutine-based for significantly better performance
- [x] Follow the player
- [x] Passes through portals
- [x] Uses blink to catch up when the target gets too far
- [x] Closes UIs (probably not needed if you use the Walk with Key)
- [x] Auto-respawn (tries to respawn unless another player is on the map)
- [x] Pick-up items
- [x] Dump items into the guild stash (optional)
- [x] Use skills (using your own custom snippets)

> ⚠️ I'm currently pursuing my Master's degree, so I don’t always have a ton of time to work on new features.<br>
> That said, I do try to fix bugs quickly — usually within a day of being reported.<br>
> Thanks in advance for your patience and understanding!  
>  
> 💡 Still, if you have a feature request, feel free to [open an issue](https://github.com/Curvu/Copilot/issues) and tag it as a suggestion — just know it might take a bit!

## Settings ⚙️
The default settings are configured to suit **my** needs and may not work for you.
Please **do not** open issues if the plugin doesn't work due to your custom configuration.

## Tips 💡
- **DO NOT** use checkpoints.
- When the target passes through a portal, they should wait nearby so the bot can detect them upon loading.
- If you're moving too fast, it's your fault the bot isn't keeping up, not the plugin's.
- It's helpful to zoom out using the [WheresMyZoomAt](https://github.com/doubleespressobro/WheresMyZoomAt-PoE2) plugin. (But not too much)
- Since the bot picks up everything, it's best to use a [good loot filter](https://www.filterblade.xyz/?game=Poe2) for the pick-up items feature.

## Known bugs 🐞
- May experience issues in "ARENA".
- The snippet system doesn’t support `async/await`, and I haven’t figured out how to fix that yet — any help is appreciated.
- I usually push updates within a day of a bug report, so please don’t hesitate to [report issues](https://github.com/Curvu/Copilot/issues)!

## Custom snippets 🧩

### Example for a Curse bot
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

### Example for a shock bot
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

## Donations 🙏

If you enjoy the plugin and want to support development, consider donating!
Your support helps keep the project alive and lets me spend more time improving it.

I’m grateful for any amount. Thank you!

<div style="display: flex;align-items: center;justify-content: center;flex-direction: column;">
  <a href="https://www.paypal.com/donate/?hosted_button_id=NX4PVU9B2YFDU">
    <img src="./assets/donate_paypal.png" width="200">
  </a>
  <a href="https://revolut.me/curvu">
    <img src="./assets/donate_revolut.png" width="160">
  </a>
</div>