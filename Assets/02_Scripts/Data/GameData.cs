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
}

[System.Serializable]
public class BaseStatData : GameDataBase
{
    public int BaseAtk;
    public int BaseHp;
    public int BaseMp;
    public int BaseAtkSpeed;
    public int BaseDefense;
}

[System.Serializable]
public class MonsterData : GameDataBase
{
    public string Name;
    public int BaseAtk;
    public int BaseHp;
    public int BaseAtkSpeed;
    public int BaseDefense;
}

[System.Serializable]
public class ExpeditionData : GameDataBase
{
    public string ExpeditionName;
    public float DurationHours;
    public int LimitLevel;
    public long RewardGold;
    public string[] RewardItems;
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
}