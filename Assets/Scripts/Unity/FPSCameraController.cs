using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class FPSCameraController : MonoBehaviour {

	public Camera FPSCamera;
	private CounterTimer fireShakeTimer;
	public float fireShakeDuration;
	public float RecoilMagnitude = 3;

	// Use this for initialization
	void Start () {
		fireShakeTimer = new CounterTimer(fireShakeDuration);
		fireShakeTimer.Update(fireShakeDuration);
	}
	
	// Update is called once per frame
	void Update () {
		CrossPlatformInputManager.SetAxis ("Mouse X", 0);
	}
		

}
