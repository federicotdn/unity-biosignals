using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pfcore;

public class EEGManager : MonoBehaviorSingleton<EEGManager> {

	public int port = 5005;
	public bool trainFromFile;
	public string filepath;
	public int threshold = 5;

	public int StatusCount { get; private set; }
	public EyesStatus Status { get; private set; }

	private EEGReader reader;
	private EEGProcessor processor;

	private bool trained;

	private EEGTrainer trainer;

	void Start () {
		trainer = new EEGTrainer ();
		if (trainFromFile) {
			Debug.Log (Application.dataPath + "/" + filepath);
			EEGReader fileReader = new EEGFileReader (Application.dataPath + "/" + filepath, false);
			EEGProcessor fileProcessor = new EEGProcessor (fileReader);

			fileProcessor.Training = true;
			fileReader.Start ();
//			fileProcessor.Start ();
//			while (!fileProcessor.Finished) {
//				fileProcessor.Update ();
//			}

			trainer.Train (fileProcessor.TrainingValues);
			trained = true;
			Debug.Log ("Finished Taraining");
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
			StatusCount = Mathf.Clamp (StatusCount, 0, 15);
			if (StatusCount >= threshold) {
				Status = EyesStatus.CLOSED;
			} else {
				Status = EyesStatus.OPEN;
			}
		}
	}
}
