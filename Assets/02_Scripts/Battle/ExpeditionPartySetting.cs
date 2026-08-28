using UnityEngine;
using System;

public class ExpeditionPartySetting
{
    private const int _maxSlots = 3;
    public static Action OnPartyChanged;

    public bool AddHunterToExpedition(string uniqueId)
    {
        string[] expParty = SaveManager.Instance.CurrentSaveData.ExpeditionPartyUids;
        string[] mainParty = SaveManager.Instance.CurrentSaveData.CurrentPartyCharacterUids;

        bool isInMainParty = false;
        for (int i = 0; i < mainParty.Length; i++)
        {
            if (mainParty[i] == uniqueId)
            {
                isInMainParty = true;
                break;
            }
        }

        if (isInMainParty)
        {
            Debug.LogWarning("현재 탑 메인 스쿼드에 편성된 헌터는 원정대에 보낼 수 없습니다!");
            return false;
        }

        bool isInExpParty = false;
        for (int i = 0; i < expParty.Length; i++)
        {
            if (expParty[i] == uniqueId)
            {
                isInExpParty = true;
                break;
            }
        }

        if (isInExpParty)
        {
            Debug.LogWarning("이미 원정대 스쿼드에 편성된 헌터입니다.");
            return false;
        }

        for (int i = 0; i < _maxSlots; i++)
        {
            if (string.IsNullOrEmpty(expParty[i]))
            {
                expParty[i] = uniqueId;
                return true;
            }
        }

        Debug.LogWarning("원정대 스쿼드가 가득 찼습니다.");
        return false;
    }

    public bool RemoveCharacterFromExpedition(string uniqueId)
    {
        string[] expParty = SaveManager.Instance.CurrentSaveData.ExpeditionPartyUids;
        for (int i = 0; i < _maxSlots; i++)
        {
            if (expParty[i] == uniqueId)
            {
                expParty[i] = "";
                return true;
            }
        }
        return false;
    }
}