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
        temp = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);
    }

    //public void OnFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null, object param4 = null, object param5 = null)
    //{
    //    clip = obj as AudioClip;
    //    m_Audio.clip = clip;
    //    m_Audio.Play();
    //}


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
            temp = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);
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
