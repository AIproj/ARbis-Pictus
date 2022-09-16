using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassUserID : MonoBehaviour {
    private Button button;

    void Awake()
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

    void TaskOnClick()
    {
        if (button.IsInteractable())
        {
            MySceneManager.Instance.SaveUserID();
            if (MySceneManager.Instance.userID == "" || MySceneManager.Instance.userID == "Enter UserID:")
            {
                GetComponent<Image>().color = Color.red;
            }
            else
            {
                GetComponent<Image>().color = Color.green;
                button.interactable = false;
            }
        }
        
    }
}
