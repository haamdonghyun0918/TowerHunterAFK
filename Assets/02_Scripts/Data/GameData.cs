using JetBrains.Annotations;
using System;
using System.Collections.Generic;

[System.Serializable]
public class GameDataBase
{
    public string Id;
}

[System.Serializable]
public class CharacterData : GameDataBase
{
    public string Name;
    public string Description;
    public string SkillId;
    public string Rarity;
    public int StarLevel;
    public string BaseStatDataId;
    public int Exp;
    public string PrefabPath;
    public int MaxSkillCost;
    public int SkillCostRegenRate;
    public int AtkPerLevel;
    public int HpPerLevel;
    public int DefPerLevel;
    public string CharacterType;
    public int GachaWeight;
    public string IconPath;
    public string CircleIconPath;
    public EquipmentRange WeaponRange;
}

[System.Serializable]
public class BaseStatData : GameDataBase
{
    public int BaseAtk;
    public int BaseHp;
    public int BaseAtkSpeed;
    public int BaseDef;
}

[System.Serializable]
public class MonsterData : GameDataBase
{
    public string Name;
    public int BaseAtk;
    public int BaseHp;
    public int BaseAtkSpeed;
    public int BaseDef;
    public string PrefabPath;
    public bool IsBoss;
    public string SkillId;
    public string SkillType;
}

[System.Serializable]
public class ExpeditionData : GameDataBase
{
    public string ExpeditionName;
    public float DurationHours;
    public int LimitLevel;
    public long RewardGold;
    public string[] RewardEquipments;
}

[System.Serializable]
public class SkillData : GameDataBase
{
    public string Name;
    public int RequiredCost;
    public int SkillDamage;
    public string SkillType;
    public string PrefabPath;
    public int SkillDuration;
    public int MotionDuration;
}

public enum EquipmentTier
{
    None = 0,
    Normal = 1,
    Rare = 2,
    Epic = 3
}

public enum EquipmentRange
{
    None = 0,
    Melee = 1,
    Ranged = 2 
}

public enum EquipmentSlot
{
    None = 0,
    Weapon = 1,
    Armor = 2,
    Accessory = 3,
}

[System.Serializable]
public class EquipmentData: GameDataBase
{
    public string Name;
    public string Position;
    public EquipmentTier Tier;
    public int Rank;
    public EquipmentRange Range;
    public int BuffAtk;
    public int BuffHp;
    public int BuffAtkSpeed;
    public int BuffDef;
    public string IconAddress;
    public int Price;
}

[System.Serializable]
public class BossData: GameDataBase
{
    public string MonsterId;
    public int LimitLevel;
    public uint RewardDiamond;
    public GuildRank RewardRank;
}