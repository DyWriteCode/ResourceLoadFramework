using UnityEngine;

public class GameStart : MonoBehaviour
{
    public AudioSource m_Audio;
    private AudioClip clip;

    private void Awake()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
    }

    private void Start()
    {
        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/senlin.mp3", OnFinish, LoadResPriority.RES_MIDDLE);
    }

    public void OnFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null, object param4 = null, object param5 = null)
    {
        clip = obj as AudioClip;
        m_Audio.clip = clip;
        m_Audio.Play();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            m_Audio.Stop();
            ResourceManager.Instance.ReleaseResouce(clip, true);
        }
    }
}
