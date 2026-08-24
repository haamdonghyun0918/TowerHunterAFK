using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class PlayerInfo : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _textRank;
    [SerializeField] private TextMeshProUGUI _textLevel;
    [SerializeField] private Image _imageGuild;

    private void OnEnable()
    {
        UpdatePlayerInfo().Forget();
    }

    public async UniTaskVoid UpdatePlayerInfo()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
        {
            Debug.LogError("[PlayerInfo]: SaveManager가 존재하지 않거나 저장 데이터가 존재하지 않습니다.");
            return;
        }

        var saveData = SaveManager.Instance.CurrentSaveData;

        if (_textLevel != null)
        {
            _textLevel.text = $"LV. {saveData.PlayerLevel}";
        }

        if (_textRank != null)
        {
            string rank = saveData.GuildRank;
            _textRank.text = rank;

            switch (rank)
            {
                case "F":
                    _textRank.color = new Color32(145, 150, 153, 255);
                    break;
                case "E":
                    _textRank.color = new Color32(84, 130, 179, 255);
                    break;
                case "D":
                    _textRank.color = new Color32(45, 135, 110, 255);
                    break;
                case "C":
                    _textRank.color = new Color32(43, 115, 235, 255);
                    break;
                case "B":
                    _textRank.color = new Color32(150, 60, 215, 255);
                    break;
                case "A":
                    _textRank.color = new Color32(195, 45, 45, 255);
                    break;
                case "S":
                    _textRank.color = new Color32(255, 195, 0, 255);
                    break;
                default:
                    _textRank.color = Color.white; // 기본 색상
                    break;
            }
        }

        if (_imageGuild != null && ResourceManager.Instance != null)
        {
            string imageKey = $"GuildRank_{saveData.GuildRank}";
            Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(imageKey);

            if (loadedSprite != null)
            {
                _imageGuild.sprite = loadedSprite;
            }

            else
            {
                Debug.LogWarning($"[PlayerInfo] 어드레서블에서 이미지를 찾을 수 없습니다: {imageKey}");
            }
        }
    }
}