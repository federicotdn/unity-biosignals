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

	// Use this for initialization
	void Start () {
		loadingPanel.SetActive (false);
		gameOverPanel.SetActive (false);
		playerWinsPanel.SetActive (false);
		pausePanel.SetActive (false);
		portInput.text = SpO2Manager.Instance.portName;
		minBPMSlider.Value = SpO2Manager.Instance.minBPM;
		SpO2Manager.Instance.maxBPM = SpO2Manager.Instance.minBPM;
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
        Time.timeScale = 1;
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

	public void Pause(bool pause) {
		pausePanel.SetActive (pause);
		if (pause) {
			Cursor.visible = true;
		} else {
			Cursor.visible = false;
			SpO2Manager.Instance.minBPM = minBPMSlider.Value;
			SpO2Manager.Instance.maxBPM = maxBPMSlider.Value;
		}
	}

	public void Reconnect() {
		SpO2Manager.Instance.Reconnect (portInput.text);
	}
}
