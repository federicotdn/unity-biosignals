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

	public int StatusCount { get; private set; }

	public EyesStatus Status { get; private set; }

	private EEGReader reader;
	private EEGProcessor processor;

	private bool trained;

	private Trainer trainer;

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
		processor.ProcessorCallback = OnFFT;
		Status = 0;
		Status = EyesStatus.NONE;		
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
				if (processor.Status == EyesStatus.OPEN) {
					StartCoroutine (PlayDelayed ());
					uiManager.actionText = "Cierra los ojos!";
				} else {
					uiManager.actionText = "Abre los ojos!";
				}
				uiManager.trainingPanel.actionText.GetComponent<Blink> ().StartBlinking ();
				beepPlayed = true;
			}

			if (trainingTimer.Finished) {
				processor.Training = false;
				training = false;
				uiManager.Training (false);
				EEGGameManager.Instance.Status = GameStatus.Playing;
				trained = true;
			} else if (statusTimer.Finished) {
				if (processor.Status == EyesStatus.OPEN) {
					processor.Status = EyesStatus.CLOSED;
					uiManager.statusText = "Manten los ojos cerrados";
				} else {
					processor.Status = EyesStatus.OPEN;
					uiManager.statusText = "Manten los ojos abiertos";
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

	void OnDestroy() {
		reading = false;
		if (processor != null) {
			processor.StopAndJoin ();
			processor = null;
		}
	}

	public void StartTraining ()
	{
		EEGUIManager.Instance.Training (true);
		beepPlayed = false;
	}

	public void StartTrainingClicked ()
	{
		int duration = (int)EEGUIManager.Instance.trainingPanel.durationSlider.value;
		trainingTimer = new CounterTimer (duration * 60);
		statusTimer = new CounterTimer (Random.Range (minStatusDuration, maxStatusDuration));
		processor.Status = EyesStatus.OPEN;
		processor.Training = true;
		training = true;
		StartReading ();
	}

	void OnFFT ()
	{
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
		SoundManager.Instance.PlayClip (trainingBeep);
	}
}
