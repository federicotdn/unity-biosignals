using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pfcore;

public class SpO2Manager : MonoBehaviorSingleton<SpO2Manager> {

	public string portName = "/dev/tty.SLAB_USBtoUART";
	public int maxQueueSize; 
	public int minBPM = 60;
	public int maxBPM = 100;

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

		float magnitude = Mathf.Max((BPM - minBPM), 0) / ((float)maxBPM - minBPM);
		LookManager.Instance.Shake (magnitude);
	}

	void OnApplicationQuit() {
		processor.StopAndJoin ();
	}

	public void Reconnect(string portName) {
		this.portName = portName;
		processor.StopAndJoin ();
		SPO2Reader reader = new SPO2Reader (portName, maxQueueSize);
		processor = new SPO2Processor (reader);
		processor.Start ();
	}
}
