using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pfcore;

public class EEGManager : MonoBehaviorSingleton<EEGManager>
{
	public FPSPlayer player;
	public int port = 5005;
	public string filepath;
	public int minThreshold = 3;
	public int maxThreshold = 6;
	public int minStatusDuration = 10;
	public int maxStatusDuration = 40;
	public AudioClip trainingBeep;

	public int StatusCount { get; private set; }

	public EyesStatus Status { get; private set; }

	private EEGReader reader;
	private EEGProcessor processor;
	public EEGProcessor Processor {
		get {
			return processor;
		}

		set {
			if (processor != null) {
				processor.StopAndJoin ();
			}
			if (value != null) {
				processor = value;
				processor.ProcessorCallback = OnFFT;
				Status = 0;
				Status = EyesStatus.NONE;	
			}
		}
	}

	private bool trained;
	public bool Trained {
		get {
			return trained;
		}

		set {
			trained = value;
			if (value) {
				training = false;
				reading = true;
				Status = EyesStatus.NONE;	
			} else {
				training = false;
			}
		}
	}

	private CounterTimer trainingTimer;
	private CounterTimer statusTimer;

	private bool training;
	private bool beepPlayed;
	private bool reading = false;
	private Vector3 previousPlayerPos;

	void Start ()
	{
		previousPlayerPos = player.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (training) {
			float remainingTime = statusTimer.Length - statusTimer.CurrentTime;
			float totalRemainingTime = trainingTimer.Length - trainingTimer.CurrentTime;

			EEGUIManager uiManager = EEGUIManager.Instance;
			uiManager.trainingTimer = ((int)Mathf.Min ((remainingTime), totalRemainingTime));
			uiManager.remainingTime = totalRemainingTime;
		
			if (!beepPlayed && remainingTime < 1.3f && !(totalRemainingTime < 1.3f)) {
				SoundManager.Instance.PlayClip (trainingBeep);
				if (Processor.Status == EyesStatus.OPEN) {
					StartCoroutine (PlayDelayed ());
					uiManager.actionText = "Cierra los ojos!";
				} else {
					uiManager.actionText = "Abre los ojos!";
				}
				uiManager.trainingPanel.actionText.GetComponent<Blink> ().StartBlinking ();
				beepPlayed = true;
			}

			if (trainingTimer.Finished) {
				Processor.Training = false;
				uiManager.Training (false);
				EEGGameManager.Instance.Status = GameStatus.Playing;
				Trained = true;
			} else if (statusTimer.Finished) {
				if (Processor.Status == EyesStatus.OPEN) {
					Processor.Status = EyesStatus.CLOSED;
					uiManager.statusText = "Manten los ojos cerrados";
				} else {
					Processor.Status = EyesStatus.OPEN;
					uiManager.statusText = "Manten los ojos abiertos";
				}
				beepPlayed = false;
				statusTimer = new CounterTimer (Random.Range (minStatusDuration, maxStatusDuration));
			}

			statusTimer.Update (Time.deltaTime);
			trainingTimer.Update (Time.deltaTime);
		}

		if (reading && Processor != null) {
			Processor.Update ();

			// If the player moves, he stops hearing
			if (Vector3.Distance (player.transform.position, previousPlayerPos) > 0.005) {
				StatusCount = 0;
			}

			previousPlayerPos = player.transform.position;
		}
	}

	public void StartReading() {
		if (!reading) {
			Processor.Start ();
			reading = true;
		}
	}

	public void StopTraining() {
		if (processor != null) {
			processor.Training = false;
		}
		training = false;
		Trained = true;
		EEGGameManager.Instance.Status = GameStatus.Playing;
	}


	void OnApplicationQuit() {
		reading = false;
		if (processor != null) {
			Processor.StopAndJoin ();
			Processor = null;
		}
	}

	void OnDestroy() {
		Time.timeScale = 1;
	}

	public void StartTrainingMode ()
	{
		EEGUIManager.Instance.Training (true);
		beepPlayed = false;
	}

	public void StartTraining (int duration, int port)
	{
		if (this.port != port || processor == null) {
			this.port = port;
			if (processor != null) {
				processor.StopAndJoin ();
				processor = null;
			}
			reader = new EEGOSCReader (port);
			Processor = new EEGProcessor (reader);
			EEGGameManager.Instance.SetProcessor (processor, false);
			reading = false;
		} else {
			Processor.Reset ();
		}

		trainingTimer = new CounterTimer (duration);
		statusTimer = new CounterTimer (Random.Range (minStatusDuration, maxStatusDuration));
		Processor.Status = EyesStatus.OPEN;
		Processor.Training = true;
		training = true;
		StartReading ();
	}

	void OnFFT ()
	{
		if (Trained) {
			if (Processor.Status == EyesStatus.CLOSED) {
				StatusCount++;
			} else {
				StatusCount--;
			}

			StatusCount = Mathf.Clamp (StatusCount, 0, maxThreshold);
			if (StatusCount >= minThreshold) {
				Status = EyesStatus.CLOSED;
			} else {
				Status = EyesStatus.OPEN;
			}
		}
	}

	public void Retrain() {
		Trained = false;
		EEGGameManager.Instance.Status = GameStatus.Training;
	}


	IEnumerator PlayDelayed ()
	{
		yield return new WaitForSeconds (0.6f);	
		SoundManager.Instance.PlayClip (trainingBeep);
	}
}
