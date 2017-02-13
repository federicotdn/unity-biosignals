using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pfcore;

public class EEGGameManager : MonoBehaviorSingleton<EEGGameManager> {
	public FPSPlayer player;
	public GameObject bombsContainer;
	public float checkingInterval = 0.2f;

	private HashSet<Bomb> bombs;

	public float hearingDistance = 20;
	public float maxVisibilityDuration = 7;


	private HashSet<Bomb> activeBombs;
	private HashSet<OutlineObject> activeObjects;
	private HashSet<OutlineObject> inactiveObjects;

	private float visibilityTime;
	private CounterTimer visibilityTimer;

	private EyesStatus previousStatus;
	private bool visibilityOn;
	private CounterTimer checkingTimer;
	// Use this for initialization
	void Start () {
		Bomb[] a = bombsContainer.GetComponentsInChildren<Bomb> ();
		bombs = new HashSet<Bomb>(a);
		activeObjects = new HashSet<OutlineObject> ();
		inactiveObjects = new HashSet<OutlineObject> (FindObjectsOfType<OutlineObject>());
		activeBombs = new HashSet<Bomb> ();
		previousStatus = EyesStatus.NONE;
		checkingTimer = new CounterTimer (checkingInterval);
	}
	
	// Update is called once per frame
	void Update () {
		EyesStatus status = EEGManager.Instance.Status;
		if (status == EyesStatus.CLOSED) {
			if (previousStatus == EyesStatus.OPEN) {
				visibilityOn = false;
				foreach (OutlineObject visibleObject in activeObjects) {
					visibleObject.Outline = false;
				}
				inactiveObjects.UnionWith (activeObjects);
				activeObjects.Clear ();
			}

			visibilityTime += Time.deltaTime;
			visibilityTime = Mathf.Min(visibilityTime, maxVisibilityDuration);

			if (checkingTimer.Finished) {
				checkingTimer.Reset ();
				foreach (Bomb bomb in bombs) {
					float distance = Vector3.Distance (player.transform.position, bomb.transform.position);
					if (distance <= hearingDistance) {
						if (!activeBombs.Contains (bomb)) {
							bomb.PlayBeepSound ();
							activeBombs.Add (bomb);
						}
					} else {
						if (activeBombs.Contains (bomb)) {
							activeBombs.Remove (bomb);
							bomb.StopPlaying ();
						}
					}
				}

				foreach (OutlineObject inactiveObject in inactiveObjects) {
					float distance = Vector3.Distance (player.transform.position, inactiveObject.transform.position);
					if (distance <= hearingDistance) {
						activeObjects.Add (inactiveObject);
					}
				}

				inactiveObjects.RemoveWhere (x => activeObjects.Contains (x));
			}
		} else {
			foreach (Bomb bomb in activeBombs) {
				bomb.StopPlaying ();
			}
			activeBombs.Clear ();

			if (previousStatus == EyesStatus.CLOSED) {
				visibilityTimer = new CounterTimer (visibilityTime);
				foreach (OutlineObject activeObject in activeObjects) {
					activeObject.Outline = true;
				}
				visibilityOn = true;
			} else {
				if (visibilityOn && visibilityTimer.Finished) {
					visibilityOn = false;
					foreach (OutlineObject activeObject in activeObjects) {
						activeObject.Outline = false;
					}
					inactiveObjects.UnionWith (activeObjects);
					activeObjects.Clear ();
				}
			}

			if (visibilityOn) {
				visibilityTimer.Update (Time.deltaTime);
			}

			visibilityTime = 0;
		}

		previousStatus = status;
		checkingTimer.Update (Time.deltaTime);
	}

	public void RemoveBomb(Bomb bomb) {
		bombs.Remove (bomb);
		activeBombs.Remove (bomb);
		RemoveOutlineObject (bomb.GetComponent<OutlineObject>());
	}

	public void RemoveOutlineObject(OutlineObject obj) {
		activeObjects.Remove (obj);
		inactiveObjects.Remove (obj);
	}
}
