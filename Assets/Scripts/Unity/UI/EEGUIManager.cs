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
	public Text statusCount;
	public Text status;
	public int trainingTimer;
	public float remainingTime;
	public string actionText;
	public string statusText;
	private string previousActionText;


	private string originalStatusText;

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
		originalStatusText = statusText;
	}

	// Update is called once per frame
	void Update () {
		trainingPanel.timer.text = trainingTimer.ToString();
		string minutes = Mathf.Floor(remainingTime / 60).ToString("00");
		string seconds = (remainingTime % 60).ToString("00");
		trainingPanel.totalTimer.text = minutes + ":" + seconds;
		trainingPanel.actionText.text = actionText;
		trainingPanel.statusText.text = statusText;

		statusCount.text = EEGManager.Instance.StatusCount.ToString();
		status.text = EEGManager.Instance.Status.ToString ();

		previousActionText = actionText;
	}

	public void Retry() {
		loadingPanel.SetActive (true);
		gameOverPanel.SetActive (false);
		playerWinsPanel.SetActive (false);
		pausePanel.SetActive (false);
		EEGGameManager.Instance.SetProcessor (EEGManager.Instance.Processor, true);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void Exit() {
		EEGGameManager.Instance.RemoveProcessor ();
		SceneManager.LoadScene("Scenes/MainMenu");
	}

	public void GameOver() {
		UnlockCursor ();
		gameOverPanel.SetActive (true);
	}

	public void PlayerWins() {
		UnlockCursor ();
		playerWinsPanel.SetActive (true);
	}

	public void Training(bool training) {
		trainingPanel.gameObject.SetActive (training);
		if (training) {
			statusText = originalStatusText;
			UnlockCursor ();
			trainingPanel.Reset ();
		}
	}

	public void Retrain() {
		EEGManager.Instance.Retrain ();
	}

	private void UnlockCursor() {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void Pause(bool pause) {
		pausePanel.SetActive (pause);
		if (pause) {
			UnlockCursor ();
		} else {
			Cursor.visible = false;
			EEGManager.Instance.minThreshold = minThresholdSlider.Value;
			EEGManager.Instance.maxThreshold = maxThresholdSlider.Value;
		}
	}

	void OnDestroy() {
		UnlockCursor ();
	}
}
