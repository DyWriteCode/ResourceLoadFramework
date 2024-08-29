using UnityEngine;

public class GameStart : MonoBehaviour
{
    private GameObject temp;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
    }

    private void Start()
    {
        //ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/menusound.mp3", OnFinish, LoadResPriority.RES_MIDDLE);
        //ResourceManager.Instance.PreloadResource("Assets/GameData/Sounds/menusound.mp3");
        //ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);
        //ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/Attack.prefab", OnFinish, LoadResPriority.RES_HIGHEST, true);
        ObjectManager.Instance.PreLoadGameObject("Assets/GameData/Prefabs/Attack.prefab", 100);
    }

    public void OnFinish(string path, Object obj, object m_param1 = null, object m_param2 = null, object m_param3 = null, object m_param4 = null, object m_param5 = null)
    {
        temp = obj as GameObject;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            //m_Audio.Stop();
            //ResourceManager.Instance.ClearCache();
            //ResourceManager.Instance.ReleaseResouce(clip, true);
            //clip = null;
            //clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
            //m_Audio.clip = clip;
            //m_Audio.Play();
            ObjectManager.Instance.ReleaseObject(temp);
            temp = null;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/Attack.prefab", OnFinish, LoadResPriority.RES_HIGHEST, true);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            //m_Audio.Stop();
            //ResourceManager.Instance.ClearCache();
            //ResourceManager.Instance.ReleaseResouce(clip, true);
            //clip = null;
            ObjectManager.Instance.ReleaseObject(temp, 0, true);
            temp = null;
        }
    }
}
