using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

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

    public async UniTask<T> LoadAsset<T>(string address) where T : UnityEngine.Object
    {
        if (_handles.TryGetValue(address, out AsyncOperationHandle handle))
        {
            return handle.Result as T;
        }

        AsyncOperationHandle<T> loadHandle = Addressables.LoadAssetAsync<T>(address);

        try
        {
            T result = await loadHandle.ToUniTask();
            _handles[address] = loadHandle;
            return result;
        }

        catch (Exception e)
        {
            Debug.LogError($"[ResourceManager] 에셋 로드 실패: {address} / Error: {e.Message}");

            if (loadHandle.IsValid())
            {
                Addressables.Release(loadHandle);
            }

            return null;
        }
    }

    public async UniTask<GameObject> Instantiate(string address, Transform parent = null, bool isSetToZeroPos = false)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, parent);

        try
        {
            GameObject instance = await handle.ToUniTask();

            if (isSetToZeroPos)
            {
                instance.transform.localPosition = Vector3.zero;
            }

            return instance;
        }

        catch (Exception e)
        {
            Debug.LogError($"[ResourceManager] 프리팹 생성 실패: {address} / Error: {e.Message}");

            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            return null;
        }
    }

    public void Release(string address)
    {
        if (_handles.TryGetValue(address, out AsyncOperationHandle handle))
        {
            Addressables.Release(handle);
            _handles.Remove(address);
            Debug.Log($"[ResourceManager] 에셋 메모리 해제 완료: {address}");
        }
    }
}