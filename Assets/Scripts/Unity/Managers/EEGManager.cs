using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pfcore;

public class EEGManager : MonoBehaviorSingleton<EEGManager>
{
	public FPSPlayer player;
	public int port = 5005;
	public bool trainFromFile;
	public string filepath;
	public int minThreshold = 3;
	public int maxThreshold = 6;
	public int minStatusDuration = 10;
	public int maxStatusDuration = 40;
	public AudioClip trainingBeep;
	public AudioSource audioSrc;
	public TrainingPanel trainingPanel;

	public int StatusCount { get; private set; }

	public EyesStatus Status { get; private set; }

	private EEGReader reader;
	private EEGProcessor processor;

	private bool trained;

	private Trainer trainer;

	private CounterTimer testingTimer;
	private CounterTimer trainingTimer;
	private CounterTimer statusTimer;

	private bool training;
	private bool beepPlayed;
	private bool reading = false;
	private Vector3 previousPlayerPos;

	void Start ()
	{
		trainer = new Trainer (EEGProcessor.FEATURE_COUNT, ClassifierType.DecisionTree);
		if (trainFromFile) {
			EEGReader fileReader = new EEGFileReader (Application.dataPath + "/" + filepath, false);
			EEGProcessor fileProcessor = new EEGProcessor (fileReader);

			fileProcessor.Training = true;
			fileProcessor.Start ();
			while (!fileProcessor.Finished) {
				fileProcessor.Update ();
			}

			trainer.Train (fileProcessor.TrainingValues);
			trained = true;
			reading = true;
		}
		reader = new EEGOSCReader (port);
		processor = new EEGProcessor (reader);

		StatusCount = 0;
		Status = EyesStatus.NONE;

		processor.ProcessorCallback = OnFFT;
		testingTimer = new CounterTimer (5);
		audioSrc.clip = trainingBeep;
		audioSrc.loop = false;
		previousPlayerPos = player.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (training) {
			Cursor.visible = true;
			float remainingTime = statusTimer.Length - statusTimer.CurrentTime;
			float totalRemainingTime = trainingTimer.Length - trainingTimer.CurrentTime;
			trainingPanel.timer.text = ((int)Mathf.Min((remainingTime), totalRemainingTime)).ToString ();

			string minutes = Mathf.Floor(totalRemainingTime / 60).ToString("00");
			string seconds = (totalRemainingTime % 60).ToString("00");
			trainingPanel.totalTimer.text = minutes + ":" + seconds;
		
			if (!beepPlayed && remainingTime < 1.3f && !(totalRemainingTime < 1.3f)) {
				audioSrc.Play ();
				if (processor.Status == EyesStatus.OPEN) {
					StartCoroutine (PlayDelayed ());
					trainingPanel.actionText.text = "Cierra los ojos!";
				} else {
					trainingPanel.actionText.text = "Abre los ojos!";
				}
				trainingPanel.actionText.GetComponent<Blink> ().StartBlinking ();
				beepPlayed = true;
			}

			if (trainingTimer.Finished) {
				processor.Training = false;
				training = false;
				trainingPanel.gameObject.SetActive (false);
				EEGGameManager.Instance.Status = GameStatus.Playing;
				trained = true;
			} else if (statusTimer.Finished) {
				if (processor.Status == EyesStatus.OPEN) {
					processor.Status = EyesStatus.CLOSED;
					trainingPanel.statusText.text = "Manten los ojos cerrados";
				} else {
					processor.Status = EyesStatus.OPEN;
					trainingPanel.statusText.text = "Manten los ojos abiertos";
				}
				beepPlayed = false;
				statusTimer = new CounterTimer (Random.Range (minStatusDuration, maxStatusDuration));
			}

			statusTimer.Update (Time.deltaTime);
			trainingTimer.Update (Time.deltaTime);
		}

		if (reading && processor != null) {
			processor.Update ();

			// If the player moves, he stops hearing
			if (Vector3.Distance (player.transform.position, previousPlayerPos) > 0.005) {
				StatusCount = 0;
			}

			previousPlayerPos = player.transform.position;
		}

//		if (testingTimer.Finished) {
//			testingTimer.Reset ();
//			if (Status == EyesStatus.CLOSED) {
//				Status = EyesStatus.OPEN;
//			} else {
//				Status = EyesStatus.CLOSED;
//			}
//		}

		testingTimer.Update (Time.deltaTime);
	}

	public void StartReading() {
		if (!reading) {
			processor.Start ();
			reading = true;
		}
	}


	void OnApplicationQuit() {
		reading = false;
		processor.StopAndJoin ();
		processor = null;
	}

	public void StartTraining ()
	{
		trainingPanel.gameObject.SetActive (true);
		trainingPanel.Reset ();
	}

	public void StartTrainingClicked ()
	{
		int duration = (int)trainingPanel.durationSlider.value;
		trainingTimer = new CounterTimer (duration * 60);
		statusTimer = new CounterTimer (Random.Range (minStatusDuration, maxStatusDuration));
		processor.Status = EyesStatus.OPEN;
		training = true;
		StartReading ();
	}

	void OnFFT ()
	{
		Debug.Log (processor.TrainingValues.Count);
		if (trained) {
			if (processor.Status == EyesStatus.CLOSED) {
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

	IEnumerator PlayDelayed ()
	{
		yield return new WaitForSeconds (0.6f);	
		audioSrc.Play ();
	}
}
