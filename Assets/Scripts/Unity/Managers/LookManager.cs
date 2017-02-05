using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class LookManager : MonoBehaviorSingleton<LookManager> {

	public float RecoilMagnitude;
	public float RecoilDuration = 0.05f;

	public float MaxShakeVel = 1;
	public float ShakeMagnitude;
	public float ShakeDuration = 0.05f;

	private float dx;
	private float dy;

	private float shakeDx;
	private float shakeDy;

	private float ax;
	private float vx;

	private float ay;
	private float vy;


	private CounterTimer recoilTimer;
	private CounterTimer shakeXTimer;
	private CounterTimer shakeYTimer;


	// Use this for initialization
	void Start () {
		recoilTimer = new CounterTimer(RecoilDuration);
		recoilTimer.Update(RecoilDuration);


		Vector3 rotationAmount = Random.insideUnitSphere;
		shakeXTimer = new CounterTimer(rotationAmount.x);
		shakeXTimer.Update(rotationAmount.x);

		shakeYTimer = new CounterTimer(rotationAmount.y);
		shakeYTimer.Update(rotationAmount.y);
	}
	
	// Update is called once per frame
	void Update () {
		recoilTimer.Update(Time.deltaTime);
		shakeXTimer.Update(Time.deltaTime);
		shakeYTimer.Update(Time.deltaTime);
		if (recoilTimer.Finished) {
			dx = 0;
			dy = 0;
		}

//		float magnitude = Mathf.Max((EKGManager.Instance.BPM - 60), 0) / 30.0f;
//		float periodDivider = Mathf.Lerp (0.9f, 1, 1 - magnitude);
//		shakeDx = Mathf.Sin (Time.timeSinceLevelLoad * periodDivider) * Time.deltaTime * ShakeMagnitude * magnitude;
//		shakeDy = Mathf.Sin (((Time.timeSinceLevelLoad) * periodDivider) + Mathf.PI) * Time.deltaTime * ShakeMagnitude * magnitude;

//		if (!shakeXTimer.Finished) {
//			vx += ax * Time.deltaTime;
//			if (Mathf.Abs(vx) > MaxShakeVel) {
//				vx = -vx;
//			}
//			shakeDx = vx * Time.deltaTime;
//		}
//
//		if (!shakeYTimer.Finished) {
//			vy += ay * Time.deltaTime;
//			if (Mathf.Abs(vy) > MaxShakeVel) {
//				vy = -vy;
//			}
//			shakeDy = vy * Time.deltaTime;
//
//		}

//		Shake ();
	}

	public void Recoil() {
		Vector3 rotationAmount = Random.insideUnitSphere * RecoilMagnitude;
		dx = rotationAmount.x;
		dy = rotationAmount.y;
		recoilTimer.Reset ();
	}

	void Shake() {
		Vector3 aux = Random.insideUnitSphere * 0.5f;
		int bpm = EKGManager.Instance.BPM;
		float magnitude = (100 - bpm) / 30.0f;
		if (shakeXTimer.Finished) {
			shakeXTimer = new CounterTimer (aux.x);
			ax = Mathf.Abs(Random.insideUnitSphere.x * ShakeMagnitude * magnitude) * - Mathf.Sign(ax);
		}

		if (shakeYTimer.Finished) {
			shakeYTimer = new CounterTimer (aux.y);
			ay = Mathf.Abs(Random.insideUnitSphere.y * ShakeMagnitude * magnitude) * - Mathf.Sign(ay);
		}
	}

	public float GetX() {
		return CrossPlatformInputManager.GetAxis ("Mouse X") + dx + shakeDx;
	}

	public float GetY() {
		return CrossPlatformInputManager.GetAxis("Mouse Y") + dy + shakeDy;
	}
		
}
