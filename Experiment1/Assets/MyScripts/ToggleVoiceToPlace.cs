using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleVoiceToPlace : MonoBehaviour {
    private bool isEnabled = true;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}
    
    private void TaskOnClick()
    {
        isEnabled = !isEnabled;
        SpeechManager.Instance.allowVoiceToPlace = !SpeechManager.Instance.allowVoiceToPlace;
        if (isEnabled)
        {
            GetComponent<Image>().color = Color.green;
        }
        else
        {
            MySceneManager.Instance.cursorIsSpeaker = false;
            GetComponent<Image>().color = Color.red;
        }
    }

}
