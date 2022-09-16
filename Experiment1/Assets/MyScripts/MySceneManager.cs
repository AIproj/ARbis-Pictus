using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MySceneManager : Singleton<MySceneManager> {
    // Loads scene gameobjects for use by other scripts
    [HideInInspector]
    public List<GameObject> wordSets = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> labels = new List<GameObject>();
    [HideInInspector]
    public List<string> labelsStrings = new List<string>();

    [Tooltip("How much the label is supposed to be shifted up after placement")]
    public float yShift;

    [HideInInspector]
    public Dictionary<string, System.Action> commands = new Dictionary<string, System.Action>();

    // Keep track of when the cursor should be a speaker
    [HideInInspector]
    public bool cursorIsSpeaker = false;

    // Keep track of the GameObject looked at
    [HideInInspector]
    public string currentLabel = null;
    [HideInInspector]
    public string lastLabel = null;
    [HideInInspector]
    public string userID = "";

    [HideInInspector]
    public GameObject control;

    [HideInInspector]
    public InputField userIDInputField;

    protected override void Awake()
    {
        base.Awake();

        // Will probably need to change the names once the experimental procedure is determined
        wordSets.Add(GameObject.Find("A1"));
        wordSets.Add(GameObject.Find("A2"));
        wordSets.Add(GameObject.Find("A3"));
        wordSets.Add(GameObject.Find("B1"));
        wordSets.Add(GameObject.Find("B2"));
        wordSets.Add(GameObject.Find("B3"));
        
        // Fill labels and labelsStrings with every word both as a gameobject and as a string
        foreach (GameObject wordSet in wordSets)
        {
            foreach (Transform childObject in wordSet.transform)
            {
                labels.Add(childObject.gameObject);
                labelsStrings.Add(childObject.gameObject.name);
            }
        }
        control = GameObject.Find("Control");
        userIDInputField = GameObject.Find("UserID").GetComponent<InputField>();

        // Enable word sets
        commands.Add("ToggleA1", () =>
        {
            wordSets[0].SetActive(!wordSets[0].activeInHierarchy);
        });

        commands.Add("ToggleA2", () =>
        {
            wordSets[1].SetActive(!wordSets[1].activeInHierarchy);
        });

        commands.Add("ToggleA3", () =>
        {
            wordSets[2].SetActive(!wordSets[2].activeInHierarchy);
        });

        commands.Add("ToggleB1", () =>
        {
            wordSets[3].SetActive(!wordSets[3].activeInHierarchy);
        });

        commands.Add("ToggleB2", () =>
        {
            wordSets[4].SetActive(!wordSets[4].activeInHierarchy);
        });

        commands.Add("ToggleB3", () =>
        {
            wordSets[5].SetActive(!wordSets[5].activeInHierarchy);
        });
        
    }

    // Use this for initialization
    void Start () {
        for (int i = 0; i < wordSets.Count; i++)
        {
            wordSets[i].SetActive(false);
        }
        control.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SaveUserID()
    {
        Debug.Log(userIDInputField.text);
        Debug.Log(userID);
        if (userIDInputField.text != null && userIDInputField.text != "Enter UserID:")
        {
            userID = userIDInputField.text;

            UserStudyLogger.Instance.CreateLogFile(userID);

            // TODO: Feed back when log file is created
        }
    }

    private void ToggleChildrenActive(GameObject parentObject)
    {
        foreach (Transform childObject in parentObject.transform)
        {
            if (childObject.gameObject == parentObject) continue;
            if (childObject.parent.gameObject == parentObject) continue;
            childObject.gameObject.SetActive(!childObject.gameObject.activeInHierarchy);
        }
    }


    IEnumerator RaycastGameobjectPosition(GameObject myGameObject, string myGameObjectAnchorName)
    {
        WorldAnchorManager.Instance.RemoveAnchor(myGameObject);
        // Wait for anchor to be removed before doing anything else
        yield return new WaitForEndOfFrame();
        // Do a raycast into the world that will only hit the Spatial Mapping mesh.
        Vector3 headPosition = Camera.main.transform.position;
        Vector3 gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo, 30.0f, SpatialMappingManager.Instance.LayerMask))
        {
            // Rotate this object to face the user.
            Quaternion toQuat = Camera.main.transform.localRotation;
            toQuat.x = 0;
            toQuat.z = 0;

            // Move this object to where the raycast
            // hit the Spatial Mapping mesh.
            // Here is where you might consider adding intelligence
            // to how the object is placed.  For example, consider
            // placing based on the bottom of the object's
            // collider so it sits properly on surfaces.
            myGameObject.transform.position = hitInfo.point + new Vector3(0, yShift, 0);
            myGameObject.transform.rotation = toQuat;
            
        }
        
        WorldAnchorManager.Instance.AttachAnchor(myGameObject, myGameObjectAnchorName);
        
    }

    public void RaycastLabelPosition(int i)
    {
        try
        {
            StartCoroutine(RaycastGameobjectPosition(labels[i], labelsStrings[i]));
        }
        catch
        {
            Debug.Log("Error accessing labels or performing the raycast");
        }
    }

}
