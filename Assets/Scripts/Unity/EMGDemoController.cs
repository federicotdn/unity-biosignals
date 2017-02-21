using pfcore;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class EMGDemoController : MonoBehaviour {

    public EMGWidget widget;
    private EMGManager manager;
    private EMGProcessor processor;

    public FirstPersonController fpsController;
    public EMGTrainingController trainingController;
    public EnergySphereGun sphereGun;

    void Start () {
        manager = EMGManager.Instance;
        manager.Setup();
        processor = manager.Processor;

        processor.AddProcessorCallback(widget.WidgetCallback);
        processor.AddProcessorCallback(DemoCallback);
    }
	
    public void DemoCallback() {
        if (processor.CurrentMode != EMGProcessor.Mode.PREDICTING) {
            return;
        }

        if (processor.PredictedMuscleState == MuscleState.RELAXED) {
            sphereGun.MuscleRelaxedTick();
        } else if (processor.PredictedMuscleState == MuscleState.TENSE) {
            sphereGun.MuscleTenseTick();
        }
    }

	void Update () {
		if (Input.GetKeyUp(KeyCode.Return)) {
            manager.StartReading();
            Debug.Log("Started reading.");
        }

        if (Input.GetKeyUp(KeyCode.I)) {
            processor.ChangeMode(EMGProcessor.Mode.IDLE);
        } else if (Input.GetKeyUp(KeyCode.T)) {
            if (processor.CurrentMuscleState != MuscleState.NONE) {
                processor.ChangeMode(EMGProcessor.Mode.TRAINING);
            } else {
                Debug.Log("Unable to start Training mode: no MuscleState set yet.");
            }
        } else if (Input.GetKeyUp(KeyCode.P)) {
            processor.ChangeMode(EMGProcessor.Mode.PREDICTING);
        }

        if (Input.GetKeyUp(KeyCode.UpArrow)) {
            processor.CurrentMuscleState = MuscleState.TENSE;
        } else if (Input.GetKeyUp(KeyCode.DownArrow)) {
            processor.CurrentMuscleState = MuscleState.RELAXED;
        }

        if (Input.GetKeyUp(KeyCode.Z)) {
            foreach (PrefabSpawner spawner in FindObjectsOfType<PrefabSpawner>()) {
                spawner.Respawn();
            }
        }
    }
}
