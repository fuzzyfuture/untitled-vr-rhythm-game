using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    public GameObject resultsPrefab;
    public float okAcc;
    public int okScore;
    public float goodAcc;
    public int goodScore;
    public float perfectAcc;
    public int perfectScore;
    public int comboNerf;

    private NoteController noteController;
    private MapManager mapManager;
    private int oks;
    private int goods;
    private int perfects;
    private int misses;
    private int combo;
    private int score;
    private float acc;

    public void IncrementHit(char judgement)
    {
        if (judgement != 'm')
        {
            combo++;
        }
        else
        {
            combo = 0;
        }

        int comboWithNerf = (combo / comboNerf) + 1;

        switch (judgement)
        {
            case 'o':
                oks++;
                score += okScore * comboWithNerf;
                break;
            case 'g':
                goods++;
                score += goodScore * comboWithNerf;
                break;
            case 'p':
                perfects++;
                score += perfectScore * comboWithNerf;
                break;
            case 'm':
                misses++;
                break;
        }

        float hitTotal = oks + goods + perfects + misses;
        float accTotal = oks * okAcc + goods * goodAcc + perfects * perfectAcc;
        acc = accTotal / hitTotal * 100f;

        UpdateText();
    }

    public void DisplayResults()
    {
        Destroy(GameObject.Find("Gameplay"));

        GameObject results = Instantiate(resultsPrefab, transform);
        results.name = "Results";

        string grade = "F";

        if (acc == 100)
        {
            grade = "S+";
        }
        else if (acc >= 99)
        {
            grade = "S";
        }
        else if (acc >= 90)
        {
            grade = "A";
        }
        else if (acc >= 80)
        {
            grade = "B";
        }
        else if (acc >= 70)
        {
            grade = "C";
        }
        else if (acc >= 50)
        {
            grade = "D";
        }

        string parent = "Results/Panel/";

        Transform gradeObj = transform.Find(parent + "GradeText");
        Transform scoreObj = transform.Find(parent + "ScoreText");
        Transform accObj = transform.Find(parent + "AccText");
        Transform judgementsObj = transform.Find(parent + "JudgementsText");
        Transform difficultyObj = transform.Find(parent + "DifficultyText");
        Transform titleObj = transform.Find(parent + "Mask/Banner/TitleText");
        Transform artistObj = transform.Find(parent + "Mask/Banner/ArtistText");
        Transform bannerObj = transform.Find(parent + "Mask/Banner");

        TMPro.TextMeshProUGUI gradeTMPro = gradeObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI scoreTMPro = scoreObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI accTMPro = accObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI judgementsTMPro = judgementsObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI difficultyTMPro = difficultyObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI titleTMPro = titleObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI artistTMPro = artistObj.GetComponent<TMPro.TextMeshProUGUI>();
        RawImage bannerImage = bannerObj.GetComponent<RawImage>();

        gradeTMPro.text = grade;
        scoreTMPro.text = score.ToString();
        accTMPro.text = acc.ToString("0.00") + "%";
        judgementsTMPro.text = perfects.ToString() + "\n" + goods.ToString() + "\n" + oks.ToString() + "\n" + misses.ToString();
        difficultyTMPro.text = "Difficulty: " + mapManager.diffName;
        titleTMPro.text = mapManager.metadata.title;
        artistTMPro.text = mapManager.metadata.artist;
        bannerImage.texture = mapManager.banner;
    }

    private void Start()
    {
        noteController = GameObject.Find("NoteController").GetComponent<NoteController>();
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();

        oks = 0;
        goods = 0;
        perfects = 0;
        misses = 0;
        combo = 0;
        score = 0;
        acc = 0;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.B) || Input.GetKeyDown(KeyCode.B))
        {
            noteController.StopAllCoroutines();
            SceneManager.LoadScene("LevelSelect");
        }
    }

    private void UpdateText()
    {
        string parentLeft = "Gameplay/CanvasLeft/Panel/";
        string parentCenter = "Gameplay/CanvasCenter/";

        Transform scoreObj = transform.Find(parentLeft + "ScoreText");
        Transform accObj = transform.Find(parentLeft + "AccText");
        Transform comboObj = transform.Find(parentCenter + "ComboText");

        TMPro.TextMeshProUGUI scoreTMPro = scoreObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI accTMPro = accObj.GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI comboTMPro = comboObj.GetComponent<TMPro.TextMeshProUGUI>();

        scoreTMPro.text = "Score: " + score.ToString();
        accTMPro.text = "Acc: " + acc.ToString("0.00") + "%";
        comboTMPro.text = combo.ToString() + "x";
    }
}
