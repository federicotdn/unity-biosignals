using System;
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
	public CustomSlider durationSlider;
	public InputField portInput;
	public Text portLabel;


	// Use this for initialization
	void Start () {
		Reset ();
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void Reset() {
		timer.gameObject.SetActive(false);
		statusText.gameObject.SetActive(false);
		actionText.gameObject.SetActive (false);
		totalTimer.gameObject.SetActive (false);
		totalTimerImage.gameObject.SetActive (false);
		durationSlider.gameObject.SetActive (true);
		portInput.gameObject.SetActive(true);
		instructions.gameObject.SetActive(true);
		trainButton.gameObject.SetActive(true);
		portLabel.gameObject.SetActive (true);
		portInput.text = EEGManager.Instance.port.ToString();
	}

	public void StartTraining() {
		timer.gameObject.SetActive(true);
		statusText.gameObject.SetActive(true);
		actionText.gameObject.SetActive (true);
		totalTimerImage.gameObject.SetActive (true);
		totalTimer.gameObject.SetActive (true);
		durationSlider.gameObject.SetActive (false);
		instructions.gameObject.SetActive(false);
		trainButton.gameObject.SetActive(false);
		portInput.gameObject.SetActive(false);
		portLabel.gameObject.SetActive (false);

		int port;
		try {
			port = Int32.Parse(portInput.text);
		} catch (FormatException e) {
			port = EEGManager.Instance.port;
		}
		EEGManager.Instance.StartTraining (durationSlider.Value * 60, port);
	}
}
