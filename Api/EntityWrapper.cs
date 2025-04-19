using System.Collections.Generic;
using System.Numerics;

using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;

namespace Copilot.Api;

public class EntityWrapper
{
    private Entity _entity;

    public EntityWrapper(Entity entity)
    {
        _entity = entity;
    }

    public Entity Entity => _entity;

    public string Metadata => _entity.Metadata;

    public EntityType EntityType => _entity.Type;

    public MonsterRarity Rarity => _entity.Rarity;

    public  bool IsAlive => _entity.IsAlive;
    public bool IsDead => _entity.IsDead;
    public bool IsHidden => _entity.IsHidden;

    public List<Buff> Buffs => _entity.Buffs;
    public Dictionary<GameStat, int> Stats => _entity.Stats;

    // Vitals stuff
    public Life Vitals => _entity.GetComponent<Life>();

    public float MaxHP => Vitals.MaxHP;
    public float CurrentHP => Vitals.CurHP;

    public float MaxMana => Vitals.MaxMana;
    public float CurrentMana => Vitals.CurMana;

    public float MaxES => Vitals.MaxES;
    public float CurrentES => Vitals.CurES;

    // Others

    public Vector3 Pos => _entity.Pos;

    public float DistanceTo(EntityWrapper e) => e.DistanceTo(e._entity);
    public float DistanceTo(Entity e) => Vector3.Distance(Pos, e.Pos);
}
