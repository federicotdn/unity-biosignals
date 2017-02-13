using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Blink : MonoBehaviour {

	public MaskableGraphic imageToToggle;

	public float interval = 0.1f;
	public bool currentState = true;
	public bool defaultState = true;
	public float duration = 1.5f;
	private bool blinking = false;

	private CounterTimer durationTimer;

	void Start()
	{
		imageToToggle.enabled = defaultState;
		durationTimer = new CounterTimer (duration);
		durationTimer.Update (duration);
	}

	void Update() {
		if (durationTimer.Finished && blinking) {
			blinking = false;
			CancelInvoke ();
		}

		durationTimer.Update (Time.deltaTime);
	}

	public void StartBlinking()
	{
		durationTimer.Reset ();
		if (blinking)
			return;

		if (imageToToggle !=null)
		{
			blinking = true;
			InvokeRepeating("ToggleState", 0, interval);
		}
	}


	public void ToggleState()
	{
		imageToToggle.enabled = !imageToToggle.enabled;
	}
}