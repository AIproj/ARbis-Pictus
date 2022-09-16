using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using HoloToolkit.Unity;
using System;
using HoloToolkit.Unity.SpatialMapping;

public class SpeechManager : Singleton<SpeechManager> { 
    // No documentation + massive changes to HoloToolkit = faster to implement our own...
    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    private bool allowVoiceCommands = true;
    [HideInInspector]
    public bool recordTime = false;
    // clickMode = true <-> click to place, clickMode = false <-> click to play sound
    [HideInInspector]
    public bool clickMode = true;

    [HideInInspector]
    public bool allowVoiceToPlace = true;

    private long timeIn;
    private long timeOut;

    public AudioSource[] soundfiles;
    private AudioSource modeSwitchFeedback, placedFeedback;


    protected override void Awake()
    {
        base.Awake();
        timeIn = 0;
        modeSwitchFeedback = soundfiles[0];
        placedFeedback = soundfiles[1];
    }

    // Use this for initialization
    void Start () {
        // Safety for the voice commands
        keywords.Add("Admin mode", () =>
        {
            modeSwitchFeedback.Play();
            timeOut = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            // append to log file IF log file exists
            if (recordTime)
            {
                UserStudyLogger.Instance.Record("User session", timeIn.ToString(), timeOut.ToString());
            }
            MySceneManager.Instance.control.SetActive(true);
            SpatialMappingManager.Instance.DrawVisualMeshes = true;
            allowVoiceCommands = true;
            clickMode = true;
            recordTime = false;
        });

        keywords.Add("User mode", () =>
        {
            modeSwitchFeedback.Play();
            timeIn = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            MySceneManager.Instance.control.SetActive(false);
            SpatialMappingManager.Instance.DrawVisualMeshes = false;
            allowVoiceCommands = false;
            clickMode = false;
            recordTime = true;
        });


        // Control panel
        keywords.Add("Show control panel", () =>
        {
            if (allowVoiceCommands) MySceneManager.Instance.control.SetActive(true);
        });

        keywords.Add("Hide control panel", () =>
        {
            if (allowVoiceCommands) MySceneManager.Instance.control.SetActive(false);
        });


        // Place objects with voice command
        for (int i = 0; i < MySceneManager.Instance.labelsStrings.Count; i++)
        {
            // Initialise current=i so that it doesn't get updated by ref to be imax for all labels
            int current = i;
            keywords.Add("Place " + MySceneManager.Instance.labelsStrings[current], () =>
            {
                if (allowVoiceCommands && allowVoiceToPlace)
                {
                    MySceneManager.Instance.RaycastLabelPosition(current);
                    placedFeedback.Play();
                }
            });
        }
        
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;

        keywordRecognizer.Start();
        
    }

    // Update is called once per frame
    void Update () {
        
	}

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        // if the keyword recognized is in our dictionary, call that Action.
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
}
