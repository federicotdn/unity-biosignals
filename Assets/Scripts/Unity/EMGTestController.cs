using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using pfcore;

public class EMGTestController : MonoBehaviour {

    public string COMPort = "COM5";
    public int maxQueueSize = 1000;

    EMGReader reader;
    EMGProcessor processor;

    private bool started = false;

	void Start () {
        reader = new EMGReader(COMPort, maxQueueSize);
        processor = new EMGProcessor(reader);
	}
	
    public void StartReading() {
        if (started) {
            Debug.Log("Already reading EMG data.");
            return;
        }

        processor.Start();
        started = true;

        Debug.Log("Now reading EMG data.");
    }

	void Update () {
		
	}
}
