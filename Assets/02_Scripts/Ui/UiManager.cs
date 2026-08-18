using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [SerializeField] Canvas Canvas_GameCanvas;
    public static UiManager Instance { get; private set; }

    private Dictionary<Type, UiBase> _createdUiDic = new Dictionary<Type, UiBase>();
    private HashSet<Type> _openUiDic = new HashSet<Type>();

    private readonly HashSet<Type> _openUiTypes = new HashSet<Type>();

    private readonly HashSet<Type> _creatingUiTypes = new HashSet<Type>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public async UniTask<T> OpenUi<T>() where T : UiBase
    {
        Type uiType = typeof(T);

        if(_createdUiDic.ContainsKey(uiType) == false)
        {
            await CreateUi<T>();
        }

        if(_createdUiDic.TryGetValue(uiType, out UiBase createdUi)==false)
        {
            Debug.LogError($"[UiManager]: {uiType.Name} UI를 열 수 없습니다.");
            return null;
        }


        T openedUi = _createdUiDic[uiType] as T;

        if (_openUiDic.Contains(uiType) == false)
        {
            openedUi.gameObject.SetActive(true);
            openedUi.transform.SetAsLastSibling(); // 추가로 수정할 부분이 있으면 수정해도 됨
            _openUiDic.Add(uiType);
        }

        return openedUi;
    }

    public void CloseUi<T>() where T : UiBase
    {
        Type uiType = typeof(T);

        if (_openUiDic.Contains(uiType))
        {
            var openedUi = _createdUiDic[uiType];
            openedUi.gameObject.SetActive(false);
            _openUiDic.Remove(uiType);
        }
    }

    private async UniTask CreateUi<T>() where T : UiBase
    {
        Type uiType = typeof(T);
        string address = uiType.Name;

        if(Canvas_GameCanvas == null)
        {
            Debug.LogError($"[UiManager]: Canvas_GameCanvas가 연결되지 않았습니다.");
            return;
        }

        if(ResourceManager.Instance == null)
        {
            Debug.LogError("[UiManager]: ResourceManager.Instance가 없습니다");
            return;
        }

        GameObject gObj = await ResourceManager.Instance.Instantiate(address, Canvas_GameCanvas.transform);

        if (gObj == null)
        {
            Debug.LogError($"[UiManager] ResourceManager가 {address}를 생성하지 못했습니다!");
            return;
        }

        var uiBase = gObj.GetComponent<T>();

        if (uiBase != null)
        {
            _createdUiDic.Add(uiType, uiBase);
        }

        else
        {
            Debug.LogError($"[UiManager] {address} 프리팹에 {uiType.Name} 스크립트가 없습니다!");
        }
    }
}