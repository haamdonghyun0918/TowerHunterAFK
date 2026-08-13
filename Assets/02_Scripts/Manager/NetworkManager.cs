using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance {  get; private set; }

    public PlayerResourceService PlayerResourceService { get; private set; }

    public StageService StageService { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            PlayerResourceService = new PlayerResourceService();
            StageService = new StageService();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
