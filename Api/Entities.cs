using System;
using System.Collections.Generic;
using System.Linq;

using ExileCore2;
using ExileCore2.Shared.Enums;
using ExileCore2.PoEMemory.MemoryObjects;

using static Copilot.Copilot;

namespace Copilot.Api;

public static class Entities
{
    private static GameController GameController => Main.GameController;

    public static EntityListWrapper List => GameController.EntityListWrapper;

    public static List<Entity> ValidList => List.OnlyValidEntities;

    public static List<Entity> ListByType(EntityType type) => List.ValidEntitiesByType[type];

    public static List<Entity> NearbyMonsters(EntityRarity rarity = EntityRarity.Any, int range = int.MaxValue, EntityWrapper entity = null, params Func<Entity, bool>[] additionalFilters)
    {
        entity ??= _player;
        return List.ValidEntitiesByType.TryGetValue(EntityType.Monster, out var monsters)
            ? monsters
                .Where(e => (rarity & ToEntityRarity(e.Rarity)) != 0 && entity.DistanceTo(e) <= range && additionalFilters.All(filter => filter(e)))
                .OrderBy(e => entity.DistanceTo(e))
                .ToList()
            : new List<Entity>();
    }

    private static EntityRarity ToEntityRarity(MonsterRarity rarity) => rarity switch
    {
        MonsterRarity.White => EntityRarity.Normal,
        MonsterRarity.Magic => EntityRarity.Magic,
        MonsterRarity.Rare => EntityRarity.Rare,
        MonsterRarity.Unique => EntityRarity.Unique,
        _ => 0
    };
}