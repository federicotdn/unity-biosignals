using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EEGGameManager : MonoBehaviorSingleton<EEGGameManager> {
	public FPSPlayer player;
	public GameObject bombsContainer;

	private List<Bomb> bombs;

	public float bombHearingDistance = 10;

	private Bomb activeBomb;
	// Use this for initialization
	void Start () {
		Bomb[] a = bombsContainer.GetComponentsInChildren<Bomb> ();
		bombs = new List<Bomb>(a);
	}
	
	// Update is called once per frame
	void Update () {
		float minDistance = 0;
		if (activeBomb != null) {
			float distance = Vector3.Distance (player.transform.position, activeBomb.transform.position);
			minDistance = distance;
			if (distance > bombHearingDistance) {
				activeBomb.StopPlaying ();
				activeBomb = null;
			}
		}

		if (EEGManager.Instance.Status == pfcore.EyesStatus.CLOSED) {
			foreach (Bomb bomb in bombs) {
				float distance = Vector3.Distance (player.transform.position, bomb.transform.position);
				if (distance <= bombHearingDistance) {
					if (activeBomb == null || distance < minDistance) {
						if (activeBomb != null) {
							activeBomb.StopPlaying ();
						}
						minDistance = distance;
						activeBomb = bomb;
					}
				}

			}

			if (activeBomb != null) {
				activeBomb.PlayBeepSound ();
			}
		} else if (activeBomb != null) {
			activeBomb.StopPlaying ();
			activeBomb = null;
		}
	}

	public void RemoveBomb(Bomb bomb) {
		bombs.Remove (bomb);
		if (bomb == activeBomb) {
			activeBomb = null;
		}
	}
}
