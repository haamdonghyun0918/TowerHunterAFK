using System;
using System.Collections.Generic;

[Serializable]
public class EquipmentSaveData
{
    public string UniqueId;
    public string BaseId;
    public int EnhanceLevel;
}

[Serializable]
public class CharacterSaveData
{
    public string UniqueId;
    public string BaseId;
    public string Rarity = "";
    public int Rank;
    public long Exp;
    public string EquippedWeaponUid = "";
    public string EquippedArmorUid = "";
    public string EquippedAccessoryUid = "";
}

[Serializable]
public class SaveData
{
    public string GuildRank = "F";

    public int CurrentStage = 1;
    public int MaxClearedStage = 0;

    public int PlayerLevel = 1;
    public long Exp = 0;
    public long Gold = 0;
    public uint Diamond = 0;
    public long MagicStone = 0;

    public List<CharacterSaveData> OwnedCharacters = new List<CharacterSaveData>();
    public List<EquipmentSaveData> OwnedEquipments = new List<EquipmentSaveData>();

    public uint RecentCharacterUid = 0;
    public uint RecentEquipmentUid = 0;

    public string[] CurrentPartyCharacterUids = new string[3] { "", "", "" };
    public string[] ExpeditionPartyUids = new string[3] { "", "", "" };
    public string[] BossRaidPartyUids = new string[5] { "", "", "", "", "" };

    public string OngoingExpeditionId = "";
    public string ExpeditionStartTime = "";
}