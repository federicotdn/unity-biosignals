using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SpO2UIManager : MonoBehaviorSingleton<SpO2UIManager> {

	public GameObject loadingPanel;
	public GameObject gameOverPanel;
	public GameObject playerWinsPanel;
	public GameObject pausePanel;
	public InputField portInput;
	public CustomSlider minBPMSlider;
	public CustomSlider maxBPMSlider;
	public CustomSlider shakeSlider;

	// Use this for initialization
	void Start () {
		loadingPanel.SetActive (false);
		gameOverPanel.SetActive (false);
		playerWinsPanel.SetActive (false);
		pausePanel.SetActive (false);
		portInput.text = SpO2Manager.Instance.portName;
		minBPMSlider.Value = SpO2Manager.Instance.minBPM;
		maxBPMSlider.Value = SpO2Manager.Instance.maxBPM;
		shakeSlider.Value = LookManager.Instance.ShakeMagnitude;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Retry() {
		loadingPanel.SetActive (true);
		gameOverPanel.SetActive (false);
		playerWinsPanel.SetActive (false);
		pausePanel.SetActive (false);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void Exit() {
		UnlockCursor ();
        Time.timeScale = 1;
		SceneManager.LoadScene("Scenes/MainMenu");
	}

	public void GameOver() {
		UnlockCursor ();
		gameOverPanel.SetActive (true);
	}

	private void UnlockCursor() {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void PlayerWins() {
		UnlockCursor ();
		playerWinsPanel.SetActive (true);
	}

	public void Pause(bool pause) {
		pausePanel.SetActive (pause);
		if (pause) {
			UnlockCursor ();
			minBPMSlider.Value = SpO2Manager.Instance.minBPM;
			maxBPMSlider.Value = SpO2Manager.Instance.maxBPM;
			shakeSlider.Value = LookManager.Instance.ShakeMagnitude;
		} else {
			Cursor.visible = false;
			SpO2Manager.Instance.minBPM = (int)minBPMSlider.Value;
			SpO2Manager.Instance.maxBPM = (int)maxBPMSlider.Value;
			LookManager.Instance.ShakeMagnitude = shakeSlider.Value;
		}
	}

	public void Reconnect() {
		SpO2Manager.Instance.Reconnect (portInput.text);
	}

	void OnDestroy() {
		UnlockCursor ();
	}
}
