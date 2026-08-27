using System;
using TMPro;
using UnityEngine;

public class BossRaidList : MonoBehaviour
{
    [SerializeField] private TMP_Text _bossName;
    [SerializeField] private UiButton _bossListButton;

    private int _index;
    private Action<int> _onClickList;

    public void SetUp(BossData bossData, int index, Action<int> onClick)
    {
        _index = index;
        _onClickList = onClick;

        MonsterData monsterData = null;

        if (bossData != null && GameDataManager.Instance != null)
        {
            monsterData = GameDataManager.Instance.GetData<MonsterData>(bossData.MonsterId);
        }

        if (_bossName != null)
        {
            string bossName = monsterData != null ? monsterData.Name : "보스 정보 없음";
            int limitLevel = bossData != null ? bossData.LimitLevel : 0;
            _bossName.text = $"{bossName} (Lv.{limitLevel})";
        }

        BindButton();
    }

    private void OnEnable()
    {
        BindButton();
    }

    private void BindButton()
    {
        if (_bossListButton == null || _onClickList == null)
        {
            return;
        }

        _bossListButton.UnBindOnClickButtonEvent(OnClickBoss);
        _bossListButton.BindOnClickButtonEvent(OnClickBoss);
    }

    private void OnClickBoss()
    {
        _onClickList?.Invoke(_index);
    }
}