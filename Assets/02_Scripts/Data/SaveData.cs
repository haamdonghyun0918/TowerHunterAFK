using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int CurrentStage = 1;
    public long Gold = 0;
    public List<string> InventoryItems = new List<string>();
}