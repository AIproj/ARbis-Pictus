using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour {

    public AudioSource wordSpoken;

    // Use this for initialization
    void Awake()
    {
        wordSpoken = GetComponent<AudioSource>();
    }
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void OnMouseEnter()
    {
        wordSpoken.Stop();
        wordSpoken.Play();
    }
}
