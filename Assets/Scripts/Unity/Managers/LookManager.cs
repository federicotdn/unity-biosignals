using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class LookManager : MonoBehaviorSingleton<LookManager>
{

	public float RecoilMagnitude;
	public float RecoilDuration = 0.05f;

	public float maxShakeDelta = 0.2f;

	public float ShakeMagnitude;

	private float dx;
	private float dy;

	private float shakeDx;
	private float shakeDy;

	public bool paused;

	private CounterTimer recoilTimer;

	// Use this for initialization
	void Start ()
	{
		recoilTimer = new CounterTimer (RecoilDuration);
		recoilTimer.Update (RecoilDuration);
	}
	
	// Update is called once per frame
	void Update ()
	{
		recoilTimer.Update (Time.deltaTime);
		if (recoilTimer.Finished) {
			dx = 0;
			dy = 0;
		}
	}

	public void Recoil ()
	{
		Vector3 rotationAmount = Random.insideUnitSphere * RecoilMagnitude;
		dx = rotationAmount.x;
		dy = rotationAmount.y;
		recoilTimer.Reset ();
	}

	public void Shake (float magnitude)
	{
		if (magnitude > 0.00001) {
//			float periodDivider = Mathf.Lerp (0.7f, 1, 1 - magnitude);
			shakeDx = Mathf.Sin (Time.timeSinceLevelLoad) * Time.deltaTime * ShakeMagnitude * magnitude * RandomSign(0.9f);
			shakeDy = Mathf.Sin (((Time.timeSinceLevelLoad)) + Mathf.PI / 2) * Time.deltaTime * ShakeMagnitude * magnitude * RandomSign(0.9f);
		
			Vector3 vec = Random.insideUnitCircle;
			shakeDx += vec.x * 0.05f * magnitude;
			shakeDy += vec.y * 0.05f * magnitude;
		}
	}

	private int  RandomSign(float threshold) {
		return Random.value < threshold ? 1 : -1;
	}


	public float GetX ()
	{
		return CrossPlatformInputManager.GetAxis ("Mouse X") + dx + shakeDx;
	}

	public float GetY ()
	{
		return CrossPlatformInputManager.GetAxis ("Mouse Y") + dy + shakeDy;
	}
		
}
