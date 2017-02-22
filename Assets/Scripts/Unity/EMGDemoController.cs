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

    public void Setup() {
        manager = EMGManager.Instance;
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
        if (Input.GetKeyUp(KeyCode.Z)) {
            foreach (PrefabSpawner spawner in FindObjectsOfType<PrefabSpawner>()) {
                spawner.Respawn();
            }
        }
    }
}
