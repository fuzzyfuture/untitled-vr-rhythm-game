using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public AudioClip song;
    public TextAsset diff;
    public Texture2D banner;
    public Metadata metadata;
    public string diffName;
    public int noteDelay;
    public int velocity;
    public bool autoplay;

    public static MapManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
