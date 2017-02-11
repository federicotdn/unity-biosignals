using pfcore;
using UnityEngine;
using UnityEngine.UI;

public class EMGWidget : MonoBehaviour {

    public Text modeText;
    public Text sampleCountText;
    public Text setMuscleStateText;
    public Text predMuscleStateText;
    public Text ticksText;

    private int ticks = 0;

	void Start () {
		
	}
	
	void Update () {
		
	}

    public void WidgetCallback() {
        EMGProcessor proc = EMGManager.Instance.Processor;
        ticks++;

        modeText.text = "Mode: " + proc.CurrentMode;
        sampleCountText.text = "Tr. Sample Count: " + proc.TrainingDataLength;
        setMuscleStateText.text = "Set MuscleState: " + proc.CurrentMuscleState;
        predMuscleStateText.text = "Predicted MuscleState: " + proc.PredictedMuscleState;
        ticksText.text = "Ticks: " + ticks.ToString();
    }
}
