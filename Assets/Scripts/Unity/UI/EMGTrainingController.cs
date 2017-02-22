using pfcore;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EMGTrainingController : MonoBehaviour {
    public Button startButton;
    public Text sliderText;
    private Text buttonText;
    public Slider timeSlider;
    public Text instructions;
    public AudioSource audioSource;

    public EMGDemoController demoController;
    private EMGManager manager;
    private EMGProcessor processor = null;

    private bool skipMode = false;
    private const float stateDuration = 10.0f;

    void Start() {
        manager = EMGManager.Instance;
        OnSliderValueChanged();

        buttonText = startButton.GetComponentInChildren<Text>();
    }

    void Update() {
        skipMode = Input.GetKey(KeyCode.K);
    }

    public void OnButtonClicked() {
        if (processor != null) {
            OnTrainingFinished();
            return;
        }

        startButton.interactable = false;
        timeSlider.interactable = false;
        manager.Setup();
        processor = manager.Processor;
        manager.StartReading();

        if (skipMode) {
            demoController.enableFakeLoop = true;
            OnTrainingFinished();
            return;
        }

        StartCoroutine(TrainingCoroutine());
    }

    public void OnSliderValueChanged() {
        sliderText.text = timeSlider.value.ToString();
        sliderText.text += " minuto" + ((timeSlider.value > 1) ? "s" : "");
    }

    public IEnumerator TrainingCoroutine() {
        float remaining = timeSlider.value * 60;
        
        for (int i = 1; i < 4; i++) {
            instructions.text = "Comenzando en " + (4 - i) + "...";
            yield return new WaitForSeconds(1);
        }

        instructions.text = "Mantenga su brazo y mano relajados.";

        processor.CurrentMuscleState = MuscleState.RELAXED;
        processor.ChangeMode(EMGProcessor.Mode.TRAINING);

        while (remaining > 0) {
            for (int i = 0; i < stateDuration; i++) {
                buttonText.text = (stateDuration - i).ToString() + "...";
                yield return new WaitForSeconds(1);
            }

            remaining -= stateDuration;
            audioSource.Play();

            if (processor.CurrentMuscleState == MuscleState.RELAXED) {
                processor.CurrentMuscleState = MuscleState.TENSE;
                instructions.text = "Haga fuerza con su brazo y mano.";
            } else {
                processor.CurrentMuscleState = MuscleState.RELAXED;
                instructions.text = "Mantenga su brazo y mano relajados.";
            }
        }

        processor.ChangeMode(EMGProcessor.Mode.PREDICTING);

        startButton.interactable = true;
        buttonText.text = "Terminar";
        instructions.text = "Entrenamiento completado. Presione el botón de Terminar para continuar.";
    }

    private void OnTrainingFinished() {
        demoController.widget.gameObject.SetActive(true);
        demoController.fpsController.enabled = true;
        demoController.Setup();

        gameObject.SetActive(false);
    }
}
