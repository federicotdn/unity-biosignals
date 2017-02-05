using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EEGStatusController : MonoBehaviour {

	public Text statusCount;
	public Text status;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		statusCount.text = EEGManager.Instance.StatusCount.ToString();
		status.text = EEGManager.Instance.Status.ToString ();
	}
}
