using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    public GameObject noteBlockPrefab;
    public Material perfect;
    public Material good;
    public Material ok;
    public Material miss;
    public float noteBlockSpawnDist;
    public float hitLeniencyMs;
    public float hitForwardOffsetMs;
    public bool debugColors;

    private AudioSource source;
    private GameplayUI gameplayUI;
    private MapManager mapManager;
    private TextAsset map;
    private float velocity;
    private float noteDelayMs;
    private bool autoplay;

    private struct Note
    {
        public int time;
        public int drum;

        public Note(int time, int drum)
        {
            this.time = time;
            this.drum = drum;
        }
    }

    public Material judgementToMat(char judgement)
    {
        Dictionary<char, Material> matDict = new Dictionary<char, Material>()
        {
            { 'm', miss },
            { 'o', ok },
            { 'g', good },
            { 'p', perfect },
        };

        return matDict[judgement];
    }

    private void Start()
    {
        source = GameObject.Find("OVRCameraRig").GetComponent<AudioSource>();
        gameplayUI = GameObject.Find("UI").GetComponent<GameplayUI>();
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();

        source.clip = mapManager.song;
        map = mapManager.diff;
        velocity = mapManager.velocity;
        noteDelayMs = mapManager.noteDelay;
        autoplay = mapManager.autoplay;

        List<Note> notes = new List<Note>();
        string text = map.text;

        // 14 = length of [HitObjects] + 2
        int hitObjectsStart = text.IndexOf("[HitObjects]") + 14;
        text = text.Substring(hitObjectsStart);

        string[] lines = text.Trim().Split('\n');

        foreach (string line in lines)
        {
            string[] lineSplit = line.Split(',');

            // Drum no. conversion algo from https://osu.ppy.sh/wiki/en/Client/File_formats/Osu_%28file_format%29
            Note newNote = new Note(Int32.Parse(lineSplit[2]), Int32.Parse(lineSplit[0]) * 4 / 512);
            notes.Add(newNote);
        }

        StartCoroutine(StartMusic(noteBlockSpawnDist / velocity));

        foreach (Note note in notes)
        {
            float timeToSpawn = (note.time + noteDelayMs) / 1000;

            StartCoroutine(SpawnNote(note.drum, timeToSpawn));

            if (autoplay)
            {
                StartCoroutine(AutoplayEnter(note.drum, timeToSpawn + (noteBlockSpawnDist / velocity)));
            }
        }
    }

    private IEnumerator AutoplayEnter(int drum, float time)
    {
        yield return new WaitForSeconds(time);

        GetDrumFromIndex(drum).GetComponent<Drum>().HitEnter();

        StartCoroutine(AutoplayExit(drum, (float) 50 / 1000));
    }

    private IEnumerator AutoplayExit(int drum, float time)
    {
        yield return new WaitForSeconds(time);

        GetDrumFromIndex(drum).GetComponent<Drum>().HitExit();
    }

    private IEnumerator SpawnNote(int drum, float time)
    {
        yield return new WaitForSeconds(time);

        Transform parent = transform.Find("Drum" + drum.ToString() + "Col");
        GameObject newNoteBlock = Instantiate(noteBlockPrefab, GetNewNoteBlockPos(drum), Quaternion.identity, parent);

        Rigidbody rb = newNoteBlock.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0, 0, -velocity);

        float timeToDrum = noteBlockSpawnDist / velocity;
        float timeToHitWindowOpen = timeToDrum - (hitLeniencyMs / 2 / 1000) - (hitForwardOffsetMs / 1000);

        StartCoroutine(HitWindowStartPhase(newNoteBlock, timeToHitWindowOpen, 'o', true));
    }

    private IEnumerator HitWindowStartPhase(GameObject obj, float time, char nextJudgement, bool beforeHalf)
    {
        // This is a very terrible awful confusing way to implement a hit window but it works
        // Better approach for future reference: track time since spawn in each note and use this value on hit to determine judgement
        yield return new WaitForSeconds(time);

        if (obj)
        {
            if (debugColors)
            {
                obj.GetComponent<Renderer>().material = judgementToMat(nextJudgement);
            }

            obj.GetComponent<NoteBlock>().judgementOnHit = nextJudgement;
            float timeToNextPhase = hitLeniencyMs / 1000 / 5;

            if (beforeHalf)
            {
                switch (nextJudgement)
                {
                    case 'o':
                        StartCoroutine(HitWindowStartPhase(obj, timeToNextPhase, 'g', true));
                        break;
                    case 'g':
                        StartCoroutine(HitWindowStartPhase(obj, timeToNextPhase, 'p', true));
                        break;
                    case 'p':
                        StartCoroutine(HitWindowStartPhase(obj, timeToNextPhase, 'g', false));
                        break;
                }
            }
            else
            {
                switch (nextJudgement)
                {
                    case 'g':
                        StartCoroutine(HitWindowStartPhase(obj, timeToNextPhase, 'o', false));
                        break;
                    case 'o':
                        StartCoroutine(HitWindowClose(obj, timeToNextPhase));
                        break;
                }
            }
        }
    }

    private IEnumerator DisplayResults(float time)
    {
        yield return new WaitForSeconds(time);

        gameplayUI.DisplayResults();
    }

    private IEnumerator HitWindowClose(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        if (obj)
        {
            obj.GetComponent<NoteBlock>().judgementOnHit = 'm';
            gameplayUI.IncrementHit('m');
            Destroy(obj);
        }
    }

    private IEnumerator StartMusic(float time)
    {
        yield return new WaitForSeconds(time);

        source.PlayOneShot(source.clip);

        StartCoroutine(DisplayResults(source.clip.length));
    }

    private GameObject GetDrumFromIndex(int drum)
    {
        return GameObject.Find("/Drums/Drum" + drum.ToString());
    }

    private Vector3 GetNewNoteBlockPos(int drum)
    {
        GameObject drumObject = GetDrumFromIndex(drum);
        Vector3 drumObjectPos = drumObject.transform.position;
        Vector3 newNoteBlockPos = new Vector3(drumObjectPos.x, drumObjectPos.y, drumObjectPos.z + noteBlockSpawnDist);

        return newNoteBlockPos;
    }
}
