using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSUI : MonoBehaviorSingleton<FPSUI> {

	public Text rounds;
	public Text BPMPText;
	public Text healthText;
	public FPSPlayer player;
	public Image bloodImage;

	// Use this for initialization
	void Start () {
		Color color = bloodImage.color;
		color.a = 0;
		bloodImage.color = color;
	}
	
	// Update is called once per frame
	void Update () {
		rounds.text = player.rounds.ToString();
		healthText.text = player.health.ToString ();
		if (BPMPText != null) {
			BPMPText.text = EKGManager.Instance.BPM.ToString ();
		}
	}

	public void Flash() {
		Color color = bloodImage.color;
		color.a = 0.5f;
		bloodImage.color = color;
		bloodImage.CrossFadeAlpha (1, 0.0001f, true);
		bloodImage.CrossFadeAlpha (0, 1.5f, false);

		healthText.GetComponent<Blink> ().StartBlinking ();
	}

}
