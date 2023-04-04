using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

[Serializable]
public class MapInfo
{
    public AudioClip song;
    public Texture2D banner;
    public TextAsset[] difficulties;
    public Metadata metadata;
}

public struct Metadata
{
    public string title;
    public string artist;
    public int length;
    public float bpm;
    public float previewPoint;
}

public class LevelSelectUI : MonoBehaviour
{
    public GameObject mapButtonPrefab;
    public GameObject diffButtonPrefab;
    public MapInfo[] maps;

    private MapManager mapManager;
    private AudioSource source;

    private void Start()
    {
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        source = GameObject.Find("OculusInteractionSampleRig/OVRCameraRig").GetComponent<AudioSource>();

        SetSliderValue("DelaySlider", mapManager.noteDelay);
        SetSliderValue("VelocitySlider", mapManager.velocity);

        Transform mapButtonParent = transform.Find("Panel/Scroll View/Viewport/Content");

        int i = 0;
        foreach (MapInfo map in maps)
        {
            map.metadata = GetMetadata(map);

            GameObject newMapButton = AddButtonToList(mapButtonPrefab, mapButtonParent, i);
            Button newMapButtonComponent = newMapButton.GetComponent<Button>();

            Transform titleObj = newMapButton.transform.Find("TitleText");
            Transform artistObj = newMapButton.transform.Find("ArtistText");

            TMPro.TextMeshProUGUI titleTMPro = titleObj.GetComponent<TMPro.TextMeshProUGUI>();
            TMPro.TextMeshProUGUI artistTMPro = artistObj.GetComponent<TMPro.TextMeshProUGUI>();

            titleTMPro.text = map.metadata.title;
            artistTMPro.text = map.metadata.artist;

            newMapButtonComponent.onClick.AddListener(delegate { PopulateDetails(map); });

            i++;
        }

        mapButtonParent.GetChild(0).GetComponent<Button>().Select();
        PopulateDetails(maps[0]);
    }

    private Metadata GetMetadata(MapInfo map)
    {
        Metadata result = new Metadata();
        string text = map.difficulties[0].text;

        int titleStart = text.IndexOf("Title:") + 6;
        int titleEnd = text.IndexOf("\n", titleStart);
        result.title = text.Substring(titleStart, titleEnd - titleStart);

        int artistStart = text.IndexOf("Artist:") + 7;
        int artistEnd = text.IndexOf("\n", artistStart);
        result.artist = text.Substring(artistStart, artistEnd - artistStart);

        int previewPointStart = text.IndexOf("PreviewTime:") + 12;
        int previewPointEnd = text.IndexOf("\n", previewPointStart);
        result.previewPoint = float.Parse(text.Substring(previewPointStart, previewPointEnd - previewPointStart)) / 1000;

        result.length = (int) map.song.length;

        int firstTimingPointStart = text.IndexOf("[TimingPoints]") + 16;
        int firstTimingPointEnd = text.IndexOf("\n", firstTimingPointStart);
        string[] firstTimingPoint = text.Substring(firstTimingPointStart, firstTimingPointEnd - firstTimingPointStart).Split(',');

        // BPM algo from https://osu.ppy.sh/wiki/en/Client/File_formats/Osu_%28file_format%29
        result.bpm = 1 / float.Parse(firstTimingPoint[1]) * 1000 * 60;

        return result;
    }

    private void PopulateDetails(MapInfo map)
    {
        source.Stop();
        source.clip = map.song;
        source.time = map.metadata.previewPoint;
        source.Play();

        string parent = "Panel/DetailsPanel/";

        Transform titleObj = transform.Find(parent + "Mask/Banner/TitleText");
        Transform artistObj = transform.Find(parent + "Mask/Banner/ArtistText");
        Transform lengthObj = transform.Find(parent + "LengthText");
        Transform bpmObj = transform.Find(parent + "BPMText");

        TMPro.TextMeshProUGUI titleTMPro = titleObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI artistTMPro = artistObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI lengthTMPro = lengthObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI bpmTMPro = bpmObj.GetComponent<TMPro.TextMeshProUGUI>();

        titleTMPro.text = map.metadata.title;
        artistTMPro.text = map.metadata.artist;
        lengthTMPro.text = "Length: " + (map.metadata.length / 60).ToString() + ":" + (map.metadata.length % 60).ToString("00");
        bpmTMPro.text = "BPM: " + map.metadata.bpm.ToString();

        Transform bannerObj = transform.Find(parent + "Mask/Banner");
        RawImage bannerImage = bannerObj.GetComponent<RawImage>();
        bannerImage.texture = map.banner;

        Transform diffButtonParent = transform.Find("Panel/DetailsPanel/DifficultiesScroll/Viewport/Content");
        
        foreach (Transform child in diffButtonParent)
        {
            Destroy(child.gameObject);
        }

        int i = 0;
        foreach (TextAsset diff in map.difficulties)
        {
            string diffName = GetDiffName(diff);

            GameObject newDiffButton = AddButtonToList(diffButtonPrefab, diffButtonParent, i);
            Button newDiffButtonComponent = newDiffButton.GetComponent<Button>();

            Transform nameObj = newDiffButton.transform.Find("NameText");
            Transform NPSObj = newDiffButton.transform.Find("NPSText");
            TMPro.TextMeshProUGUI nameTMPro = nameObj.GetComponent<TMPro.TextMeshProUGUI>();
            TMPro.TextMeshProUGUI NPSTMPro = NPSObj.GetComponent<TMPro.TextMeshProUGUI>();
            nameTMPro.text = diffName;

            int notesStart = diff.text.IndexOf("[HitObjects]") + 14;
            string notes = diff.text.Substring(notesStart);
            string[] notesSplit = notes.Trim().Split('\n');
            int noteCount = notesSplit.Length;

            int firstNoteTime = Int32.Parse(notesSplit[0].Split(',')[2]);
            int lastNoteTime = Int32.Parse(notesSplit[notesSplit.Length - 1].Split(',')[2]);
            int lengthTrimmed = map.metadata.length - (firstNoteTime / 1000) - (map.metadata.length - (lastNoteTime / 1000));
            NPSTMPro.text = "NPS: " + (noteCount / lengthTrimmed).ToString();

            newDiffButtonComponent.onClick.AddListener(delegate { LoadMap(map, diff, diffName); });

            i++;
        }
    }

    private GameObject AddButtonToList(GameObject prefab, Transform parent, int i)
    {
        GameObject newButton = Instantiate(prefab, parent);
        RectTransform newButtonRect = newButton.GetComponent<RectTransform>();
        Button newButtonComponent = newButton.GetComponent<Button>();

        float newPositionY = newButtonRect.localPosition.y - (newButtonRect.rect.height * i);
        Vector3 newPosition = new Vector3(newButtonRect.localPosition.x, newPositionY, newButtonRect.localPosition.z);
        newButtonRect.localPosition = newPosition;

        return newButton;
    }

    private string GetDiffName(TextAsset diff)
    {
        string text = diff.text;

        int nameStart = text.IndexOf("Version:") + 8;
        int nameEnd = text.IndexOf("\n", nameStart);

        return text.Substring(nameStart, nameEnd -  nameStart);
    }

    private int GetSliderValue(string name)
    {
        return (int) GameObject.Find("UIOptions/Panel/" + name).GetComponent<Slider>().value;
    }

    private void SetSliderValue(string name, int value)
    {
        GameObject.Find("UIOptions/Panel/" + name).GetComponent<Slider>().value = value;
    }

    private bool GetToggleValue(string name)
    {
        return GameObject.Find("UIOptions/Panel/" + name).GetComponent<Toggle>().isOn;
    }

    private void LoadMap(MapInfo map, TextAsset diff, string diffName)
    {
        mapManager.song = map.song;
        mapManager.diff = diff;
        mapManager.banner = map.banner;
        mapManager.metadata = map.metadata;
        mapManager.diffName = diffName;
        mapManager.velocity = GetSliderValue("VelocitySlider");
        mapManager.noteDelay = GetSliderValue("DelaySlider");
        mapManager.autoplay = GetToggleValue("AutoplayToggle");

        SceneManager.LoadScene("Main");
    }
}
