using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pfcore;

public class ProcessorContainer : MonoBehaviour {
	public EEGProcessor processor;
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
