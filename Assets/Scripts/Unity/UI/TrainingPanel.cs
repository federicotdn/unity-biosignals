using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingPanel : MonoBehaviour {

	public Text instructions;
	public Text timer;
	public Text totalTimer;
	public Text statusText;
	public Text actionText;
	public Button trainButton;
	public Image totalTimerImage;
	public Slider durationSlider;
	public Text durationText;
	public Text durationValueText;

	// Use this for initialization
	void Start () {
		Reset ();
	}

	// Update is called once per frame
	void Update () {
		durationValueText.text = ((int)durationSlider.value).ToString();
	}

	public void Reset() {
		timer.gameObject.SetActive(false);
		statusText.gameObject.SetActive(false);
		actionText.gameObject.SetActive (false);
		totalTimer.gameObject.SetActive (false);
		totalTimerImage.gameObject.SetActive (false);
		durationSlider.gameObject.SetActive (true);
		durationText.gameObject.SetActive (true);
		durationValueText.gameObject.SetActive (true);
		instructions.gameObject.SetActive(true);
		trainButton.gameObject.SetActive(true);
	}

	public void StartTraining() {
		timer.gameObject.SetActive(true);
		statusText.gameObject.SetActive(true);
		actionText.gameObject.SetActive (true);
		totalTimerImage.gameObject.SetActive (true);
		totalTimer.gameObject.SetActive (true);
		durationSlider.gameObject.SetActive (false);
		durationText.gameObject.SetActive (false);
		durationValueText.gameObject.SetActive (false);
		instructions.gameObject.SetActive(false);
		trainButton.gameObject.SetActive(false);
	}
}
