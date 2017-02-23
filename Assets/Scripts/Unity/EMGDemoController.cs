using pfcore;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class EMGDemoController : MonoBehaviour {

    public EMGWidget widget;
    public GameObject helpWidget;
    private EMGManager manager;
    private EMGProcessor processor;

    public FirstPersonController fpsController;
    public EMGTrainingController trainingController;
    public EnergySphereGun sphereGun;

    public bool enableFakeLoop = false;

    public void Setup() {
        manager = EMGManager.Instance;
        processor = manager.Processor;

        if (!enableFakeLoop) {
            processor.AddProcessorCallback(widget.WidgetCallback);
            processor.AddProcessorCallback(DemoCallback);
        } else {
            StartCoroutine(FakeEMGLoop());
        }
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

    IEnumerator FakeEMGLoop() {
        while (true) {
            yield return new WaitForSeconds(EMGManager.EMG_TICK_DURATION); // Simulate EMGProcessor delay
            if (Input.GetKey(KeyCode.E)) {
                sphereGun.MuscleTenseTick();
            } else {
                sphereGun.MuscleRelaxedTick();
            }
        }
    }

    void Update () {
        if (Input.GetKeyUp(KeyCode.Z)) {
            foreach (PrefabSpawner spawner in FindObjectsOfType<PrefabSpawner>()) {
                spawner.Respawn();
            }
        }

        if (Input.GetKeyUp(KeyCode.H)) {
            widget.gameObject.SetActive(!widget.gameObject.activeSelf);
            helpWidget.SetActive(!helpWidget.activeSelf);
        }

        if (Input.GetKeyUp(KeyCode.P)) {
            SceneManager.LoadScene("Scenes/MainMenu");
        }
    }
}
