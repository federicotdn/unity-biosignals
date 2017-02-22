using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSUI : MonoBehaviorSingleton<FPSUI> {

	public Text rounds;
	public Text BPMPText;
	public Text healthText;
	public Image healthImage;
	public Image roundsImage;
	public FPSPlayer player;
	public Image bloodImage;
	public Text waveLabel;

	// Use this for initialization

	private int previousHealth;
	private int previousRounds;
	private int previousWave;
	void Start () {
		Color color = bloodImage.color;
		color.a = 0;
		bloodImage.color = color;
		previousHealth = -1;
		previousRounds = -1;
	}
	
	// Update is called once per frame
	void Update () {
		rounds.text = player.rounds  + " | " + player.remainingRounds;
		healthText.text = player.health.ToString ();
		if (BPMPText != null) {
			BPMPText.text = SpO2Manager.Instance.BPM.ToString ();
		}

		if (previousHealth != -1 && player.health != previousHealth) {
			healthImage.GetComponent<Blink> ().StartBlinking ();

			if (player.health < previousHealth) {
				Flash ();
			}
		}

		if ((Input.GetKeyDown (player.reloadKey) || Input.GetMouseButton(0)) && (player.remainingRounds + player.rounds) == 0) {
			BlinkRounds ();
		}

		if (previousRounds != -1 && player.remainingRounds > previousRounds) {
			BlinkRounds ();
		}

		previousHealth = player.health;
		previousRounds = player.remainingRounds;

		if (waveLabel != null && SpO2GameManager.IsInitialized()) {
			waveLabel.text = SpO2GameManager.Instance.wave.ToString ();
			if (previousWave != SpO2GameManager.Instance.wave) {
				waveLabel.GetComponent<Blink> ().StartBlinking ();
			}
			previousWave = SpO2GameManager.Instance.wave;
		}
	}

	private void Flash() {
		Color color = bloodImage.color;
		color.a = 0.5f;
		bloodImage.color = color;
		bloodImage.CrossFadeAlpha (1, 0.0001f, true);
		bloodImage.CrossFadeAlpha (0, 1.5f, false);
	}

	private void BlinkRounds() {
		roundsImage.GetComponent<Blink> ().StartBlinking ();
	}

}
