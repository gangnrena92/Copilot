namespace Copilot.Api;

public enum EntityRarity
{
  Normal = 1 << 0,
  Magic = 1 << 1,
  Rare = 1 << 2,
  Unique = 1 << 3,
  Any = Normal | Magic | Rare | Unique,
  AtLeastRare = Rare | Unique
}