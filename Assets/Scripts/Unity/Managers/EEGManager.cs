using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pfcore;

public class EEGManager : MonoBehaviorSingleton<EEGManager> {

	public int port = 5005;
	public bool trainFromFile;
	public string filepath;
	public int minThreshold = 3;
	public int maxThreshold = 6;

	public int StatusCount { get; private set; }
	public EyesStatus Status { get; private set; }

	private EEGReader reader;
	private EEGProcessor processor;

	private bool trained;

	private EEGTrainer trainer;

	void Start () {
		trainer = new EEGTrainer ();
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
		}
		reader = new EEGOSCReader (port);
		processor = new EEGProcessor (reader);
		processor.Start ();

		StatusCount = 0;
		Status = EyesStatus.NONE;

		processor.ProcessorCallback = OnFFT;
	}
	
	// Update is called once per frame
	void Update () {
		processor.Update ();
		Status = EyesStatus.CLOSED;
	
	}

	void OnFFT() {
		if (trained) {
			List<EyesStatus> outputs = trainer.Predict (processor.TrainingValuesWindow);
			int aux = 0;
			foreach (EyesStatus status in outputs) {
				if (status == EyesStatus.CLOSED) {
					aux++;
				} else {
					aux--;
				}
			}
			StatusCount += aux;
			StatusCount = Mathf.Clamp (StatusCount, 0, maxThreshold);
			if (StatusCount >= minThreshold) {
				Status = EyesStatus.CLOSED;
			} else {
				Status = EyesStatus.OPEN;
			}
		}
	}
}
