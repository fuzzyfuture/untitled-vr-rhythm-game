using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drum : MonoBehaviour
{
    public Material white;
    public Material red;

    private AudioSource source;
    private IDictionary<string, KeyCode> drumObjectToKeyCode;
    private Transform noteCol;
    private GameplayUI GameplayUI;
    private NoteController noteController;
    private MapManager mapManager;

    public void HitEnter()
    {
        char judgement = 'm';

        source.PlayOneShot(source.clip);

        if (noteCol.childCount > 0)
        {
            GameObject nextNoteObj = noteCol.GetChild(0).gameObject;
            NoteBlock nextNote = nextNoteObj.GetComponent<NoteBlock>();
            judgement = nextNote.judgementOnHit;

            if (judgement != 'm')
            {
                GameplayUI.IncrementHit(judgement);
                Destroy(nextNoteObj);
            }
        }

        GetComponent<Renderer>().material = noteController.judgementToMat(judgement);
    }

    public void HitExit()
    {
        GetComponent<Renderer>().material = white;
    }

    private void Start()
    {
        drumObjectToKeyCode = new Dictionary<string, KeyCode>()
        {
            { "Drum0", KeyCode.D },
            { "Drum1", KeyCode.F },
            { "Drum2", KeyCode.J },
            { "Drum3", KeyCode.K },
        };

        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        source = GetComponent<AudioSource>();
        source.volume = mapManager.volume;
        noteController = GameObject.Find("NoteController").GetComponent<NoteController>();
        noteCol = GameObject.Find("/NoteController/" + name + "Col").transform;
        GameplayUI = GameObject.Find("UI").GetComponent<GameplayUI>();
    }

    private void Update()
    {
        // Keyboard input for timing debug
        KeyCode key = drumObjectToKeyCode[name];

        if (Input.GetKeyDown(key) && !noteController.autoplay)
        {
            HitEnter();
        }

        if (Input.GetKeyUp(key) && !noteController.autoplay)
        {
            HitExit();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Drumstick") && !noteController.autoplay)
        {
            HitEnter();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!noteController.autoplay)
        {
            HitExit();
        }
    }
}
