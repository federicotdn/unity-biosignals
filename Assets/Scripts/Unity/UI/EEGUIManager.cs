using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using pfcore;

public class EEGUIManager : MonoBehaviorSingleton<EEGUIManager> {

	public GameObject loadingPanel;
	public TrainingPanel trainingPanel;
	public GameObject gameOverPanel;
	public GameObject playerWinsPanel;
	public GameObject pausePanel;
	public CustomSlider minThresholdSlider;
	public CustomSlider maxThresholdSlider;
	public int trainingTimer;
	public float remainingTime;
	public string actionText;
	public string statusText;
	private string previousActionText;

	// Use this for initialization
	void Start () {
		loadingPanel.SetActive (false);
		gameOverPanel.SetActive (false);
		playerWinsPanel.SetActive (false);
		pausePanel.SetActive (false);
		trainingPanel.gameObject.SetActive (false);
		minThresholdSlider.Value = EEGManager.Instance.minThreshold;
		maxThresholdSlider.Value = EEGManager.Instance.maxThreshold;
		statusText = trainingPanel.statusText.text;
	}

	// Update is called once per frame
	void Update () {
		trainingPanel.timer.text = trainingTimer.ToString();
		string minutes = Mathf.Floor(remainingTime / 60).ToString("00");
		string seconds = (remainingTime % 60).ToString("00");
		trainingPanel.totalTimer.text = minutes + ":" + seconds;
		trainingPanel.actionText.text = actionText;
		trainingPanel.statusText.text = statusText;

		previousActionText = actionText;
	}

	public void Retry() {
		loadingPanel.SetActive (true);
		gameOverPanel.SetActive (false);
		playerWinsPanel.SetActive (false);
		pausePanel.SetActive (false);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void Exit() {
		loadingPanel.SetActive (true);
		gameOverPanel.SetActive (true);
		SceneManager.LoadScene(0);
	}

	public void GameOver() {
		Cursor.visible = true;
		gameOverPanel.SetActive (true);
	}

	public void PlayerWins() {
		Cursor.visible = true;
		playerWinsPanel.SetActive (true);
	}

	public void Training(bool training) {
		trainingPanel.gameObject.SetActive (training);
		if (training) {
			Cursor.visible = true;
			trainingPanel.Reset ();
		}
	}

	public void Pause(bool pause) {
		pausePanel.SetActive (pause);
		if (pause) {
			Cursor.visible = true;
		} else {
			Cursor.visible = false;
			EEGManager.Instance.minThreshold = minThresholdSlider.Value;
			EEGManager.Instance.maxThreshold = maxThresholdSlider.Value;
		}
	}
}
