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
    public string Skill;
    public string Rarity;
    public int StarLevel;
    public string BaseStatDataId;
    public int Exp;
    public string PrefabPath;
}

[System.Serializable]
public class BaseStatData : GameDataBase
{
    public int BaseAtk;
    public int BaseHp;
    public int BaseMp;
    public int BaseAtkSpeed;
}

[System.Serializable]
public class MonsterData : GameDataBase
{
    public int BaseAtk;
    public int BaseHp;
    public int BaseAtkSpeed;
}
