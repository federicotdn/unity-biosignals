using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSUI : MonoBehaviour {

	public Text Rounds;
	public Text BPMPText;
	public FPSPlayer player;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Rounds.text = player.rounds.ToString();
		BPMPText.text = EKGManager.Instance.BPM.ToString();
	}
}
