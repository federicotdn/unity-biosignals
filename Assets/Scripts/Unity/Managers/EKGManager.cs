using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pfcore;

public class EKGManager : MonoBehaviorSingleton<EKGManager> {

	public string portName = "/dev/tty.SLAB_USBtoUART";
	public int maxQueueSize; 

	private bool accelerate;

	private SPO2Processor processor;

	public int BPM {
		get {
			float aux = 1;

			if (accelerate) {
				aux = 1.3f;
			}
			return (int)(processor.GetBPM () * aux);
		}
	}

	// Use this for initialization
	void Start () {
		SPO2Reader reader = new SPO2Reader (portName, maxQueueSize);
		processor = new SPO2Processor (reader);
		processor.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		processor.Update ();

		accelerate = Input.GetKey (KeyCode.K);
	}

	void OnApplicationQuit() {
		processor.StopAndJoin ();
	}
}
