using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pfcore;

public enum GameStatus {
	Playing, Training, Paused, GameOver, PlayerWins
}

public class EEGGameManager : MonoBehaviorSingleton<EEGGameManager> {
	public FPSPlayer player;
	public GameObject bombsContainer;
	public float checkingInterval = 0.4f;

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

	private bool started;

	private GameStatus status;
	public GameStatus Status {
		get {
			return status;
		}

		set {
			switch (value) {
			case GameStatus.Playing:
				player.fpsController.enabled = true;
				player.enabled = true;
				Time.timeScale = 1;
				Cursor.visible = false;
				SoundManager.Instance.UnPauseAudio ();
				SoundManager.Instance.MuteAll (false);
				SoundManager.Instance.PauseMainSong (false);
				EEGUIManager.Instance.Pause (false);
				break;
			case GameStatus.Training:
				SoundManager.Instance.PauseAudio ();
				SoundManager.Instance.PauseMainSong (true);
				SoundManager.Instance.MuteAll (true);
				EEGUIManager.Instance.Pause (false);
				player.fpsController.enabled = false;
				player.enabled = false;
				EEGManager.Instance.StartTraining ();
				break;
			case GameStatus.Paused:
				SoundManager.Instance.PauseAudio ();
				SoundManager.Instance.MuteAll (true);
				player.fpsController.enabled = false;
				Time.timeScale = 0;
				EEGUIManager.Instance.Pause (true);
				break;
			case GameStatus.GameOver:
				Invoke ("GameOver", 1);
				break;
			case GameStatus.PlayerWins:
				SoundManager.Instance.PauseAudio ();
				SoundManager.Instance.MuteAll (true);
				player.fpsController.enabled = false;
				Time.timeScale = 0;
				EEGUIManager.Instance.PlayerWins ();
				break;
			}

			status = value;
		}
	}

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
		if (!started) {
			if (EEGManager.Instance.trainFromFile) {
				Status = GameStatus.Playing;
			} else {
				Status = GameStatus.Training;
			}
			started = true;
		}

		switch (status) {
		case GameStatus.Playing:
			if (Input.GetKeyDown (KeyCode.P)) {
				Status = GameStatus.Paused;
				return;
			}

			if (player.health <= 0) {
				Status = GameStatus.GameOver;
				return;
			}

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
					visibilityTimer = new CounterTimer (Mathf.Min(visibilityTime + EEGManager.Instance.minThreshold, maxVisibilityDuration));
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
			break;
		case GameStatus.Paused:
			if (Input.GetKeyDown (KeyCode.P)) {
				Status = GameStatus.Playing;
				return;
			}
			break;
		default:

			break;
			
		}
	}

	public void RemoveBomb(Bomb bomb) {
		bombs.Remove (bomb);
		activeBombs.Remove (bomb);
		RemoveOutlineObject (bomb.GetComponent<OutlineObject>());
	}

	public void RemoveOutlineObject(OutlineObject obj) {
		obj.Outline = false;
		if (activeObjects != null) {
			activeObjects.Remove (obj);
		}

		if (inactiveObjects != null) {
			inactiveObjects.Remove (obj);
		}
	}

	private void GameOver() {
		SoundManager.Instance.PauseAudio ();
		SoundManager.Instance.MuteAll (true);
		player.fpsController.enabled = false;
		Time.timeScale = 0;
		EEGUIManager.Instance.GameOver ();
	}
}
