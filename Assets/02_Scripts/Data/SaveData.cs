using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int CurrentStage = 1;
    public int MaxClearedStage = 0;

    public long Gold = 0;
    public uint Diamond = 0;
    public long Exp = 0;

    public List<string> OwnedEquipments = new List<string>();

    public int PlayerLevel = 1;
    //TODO: 가지고 있는 헌터들의 Id들과 구성한 헌터들의 스쿼드 저장하는 내용도 추가할 것

    public string OngoingExpeditionId = "";
    public string ExpeditionStartTime = "";
}