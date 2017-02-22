using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other) {
		if (other.GetComponent<FPSPlayer> () != null) {
			EEGGameManager.Instance.Status = GameStatus.PlayerWins;
		}
	}
}
