using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineObject : MonoBehaviour {
	public float raycastInterval = 0.3f;

	private List<MaterialSwitcher> materialSwitchers;
	private CounterTimer raycastTimer;

	private bool outline;
	public bool Outline {
		get {
			return outline;
		}

		set {
			outline = value;
			if (outline) {
				foreach (MaterialSwitcher s in materialSwitchers) {
					s.SwitchToAlternative ();
				}
			} else {
				foreach (MaterialSwitcher s in materialSwitchers) {
					s.SwitchToMain ();
				}
			}
		}
	}
	// Use this for initialization
	void Start () {
		MaterialSwitcher[] switchers = GetComponentsInChildren<MaterialSwitcher> ();
		materialSwitchers = new List<MaterialSwitcher> (switchers);
		materialSwitchers.AddRange(GetComponents<MaterialSwitcher>());
		raycastTimer = new CounterTimer (raycastInterval);
	}
	
	// Update is called once per frame
	void Update () {
		if (outline && raycastTimer.Finished) {
			raycastTimer.Reset ();
			RaycastHit hit;
			Vector3 dir = ((EEGGameManager.Instance.player.transform.position + Vector3.up * 0.2f) - transform.position);
			if (Physics.Raycast (transform.position + (dir.normalized * 0.5f), dir, out hit, EEGGameManager.Instance.hearingDistance)) {
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer ("Wall")) {
					foreach (MaterialSwitcher s in materialSwitchers) {
						s.SwitchToAlternative ();
					}
				} else {
					foreach (MaterialSwitcher s in materialSwitchers) {
						s.SwitchToMain ();
					}
				}
			}
		}

		raycastTimer.Update (Time.deltaTime);
	}
}
