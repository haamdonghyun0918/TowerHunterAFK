using System.Collections.Generic;
public class BossRaidViewModel : ViewModelBase
{
    private readonly BossRaidModel _bossModel;

    public BossRaidViewModel(BossRaidModel bossModel)
    {
        _bossModel = bossModel;
    }

    public BossData SelectedBoss
    {
        get => _bossModel.SelectedBoss;

        private set
        {
            if(_bossModel.SelectedBoss == value)
            {
                return;
            }

            _bossModel.SelectedBoss = value;
            OnPropertyChanged(nameof(SelectedBoss));
            OnPropertyChanged(nameof(CanStart));
        }
    }

    public bool IsRaidInProgress
    {
        get => _bossModel.IsRaidInProgress;

        private set
        {
            if(_bossModel.IsRaidInProgress == value)
            {
                return;
            }

            _bossModel.IsRaidInProgress = value;
            OnPropertyChanged(nameof(IsRaidInProgress));
            OnPropertyChanged(nameof(CanStart));
        }
    }

    public IReadOnlyList<string> PartyUids => _bossModel.PartyUids;

    public int PartyCount
    {
        get
        {
            int count = 0;
            for(int i = 0; i< BossRaidModel.MaxPartySize; i++)
            {
                if(string.IsNullOrEmpty(_bossModel.PartyUids[i]) == false)
                {
                    count++;
                }
            }
            return count;
        }
    }

    public bool IsPartyComplete => PartyCount == BossRaidModel.MaxPartySize;

    public bool CanStart => SelectedBoss != null && IsPartyComplete && IsRaidInProgress == false;

    public bool SelectBoss(BossData bossData)
    {
        if(bossData == null || IsRaidInProgress)
        {
            return false;
        }

        SelectedBoss = bossData;
        return true;
    }

    public bool TryAddHunter(string uniqueId)
    {

        if(string.IsNullOrEmpty(uniqueId) || IsRaidInProgress)
        {
            return false;
        }

        for(int i = 0; i < BossRaidModel.MaxPartySize;  ++i)
        {
            if (_bossModel.PartyUids[i] == uniqueId)
            {
                return false; 
            }
        }

        for(int i = 0; i < BossRaidModel.MaxPartySize; i++)
        {
            if (string.IsNullOrEmpty(_bossModel.PartyUids[i]))
            {
                _bossModel.PartyUids[i] = uniqueId;
                NotifyPartyChanged();
                return true;
            }
        }

        return false;
    }

    public bool TryRemoveHunter(string uniqueId)
    {
        if (string.IsNullOrEmpty(uniqueId) || IsRaidInProgress)
        {
            return false;
        }

        for (int i = 0; i < BossRaidModel.MaxPartySize; i++)
        {
            if (_bossModel.PartyUids[i] == uniqueId)
            {
                _bossModel.PartyUids[i] = "";
                NotifyPartyChanged();
                return true;
            }
        }

        return false;
    }

    public void ReplaceParty(IReadOnlyList<string> partyUids)
    {
        if(IsRaidInProgress)
        {
            return;
        }
        HashSet<string> usedUids = new HashSet<string>();
        bool isChanged = false;

        for (int i = 0; i < BossRaidModel.MaxPartySize; i++)
        {
            string newUid = "";

            if (partyUids != null && i < partyUids.Count)
            {
                string candidateUid = partyUids[i];

                if (string.IsNullOrEmpty(candidateUid) == false && usedUids.Add(candidateUid))
                {
                    newUid = candidateUid;
                }
            }

            if (_bossModel.PartyUids[i] != newUid)
            {
                _bossModel.PartyUids[i] = newUid;
                isChanged = true;
            }
        }

        if (isChanged)
        {
            NotifyPartyChanged();
        }
    }

    public string[] CopyPartyUids()
    {
        string[] copiedParty = new string[BossRaidModel.MaxPartySize];

        for (int i = 0; i < BossRaidModel.MaxPartySize; i++)
        {
            copiedParty[i] = _bossModel.PartyUids[i];
        }

        return copiedParty;
    }

    public bool TryStartRaid()
    {
        if (CanStart == false)
        {
            return false;
        }

        IsRaidInProgress = true;
        return true;
    }

    public bool CompleteRaid()
    {
        if (IsRaidInProgress == false)
        {
            return false;
        }

        IsRaidInProgress = false;
        return true;
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(SelectedBoss));
        OnPropertyChanged(nameof(IsRaidInProgress));
        NotifyPartyChanged();
    }

    private void NotifyPartyChanged()
    {
        OnPropertyChanged(nameof(PartyUids));
        OnPropertyChanged(nameof(PartyCount));
        OnPropertyChanged(nameof(IsPartyComplete));
        OnPropertyChanged(nameof(CanStart));
    }

}
