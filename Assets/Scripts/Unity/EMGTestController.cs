using System.Collections.Generic;
using UnityEngine;

using pfcore;
using System;
using System.IO;
using UnityEngine.UI;
using System.Numerics;
using UnityEngine.SceneManagement;

public class EMGTestController : MonoBehaviour {

    public int readingsXAxisSize = 1000;

    public string SerialPort = "COM5";
    public int maxQueueSize = 1000;
    public bool useFile = false;
    public string filePath = "";

    public WMG_Axis_Graph readingsGraph;
    public WMG_Axis_Graph fftGraph;
    public WMG_Axis_Graph postFFTGraph;

    public Text modeLabel;
    public Text meanLabel;
    public Text predictionLabel;
    public Text trainingInfoLabel;

    public WMG_Series readingsSeries;
    public WMG_Series fftSeries;
    public WMG_Series postFFTSeries;

    public Button startWriteButton;
    public Button stopWriteButton;

    private EMGReader reader;
    private EMGProcessor processor;

    private bool started = false;
    private bool recording = false;
    private FileStream outFileStream = null;

    private long baseTime;

    void Awake() {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }
    void OnDestroy() {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    void OnSceneChanged(Scene a, Scene b) {
        /* Empty */
    }

    void Start () {
        if (!useFile) {
            reader = new EMGSerialReader(SerialPort, maxQueueSize);
        } else {
            Debug.Log("Setting up file mode.");

            FileStream stream;
            try {
                stream = File.OpenRead(Application.dataPath + "/../DataSets/EMG/" + filePath);
            } catch (Exception e) {
                Debug.Log("Unable to open file: " + e);
                return;
            }
             
            EMGFileReader fileReader = new EMGFileReader(stream, maxQueueSize);
            fileReader.EnableFileLoop();
            reader = fileReader;
        }

        processor = new EMGProcessor(reader);
	}
	
    public void StartReading() {
        if (started) {
            Debug.Log("Already reading EMG data.");
            return;
        }

        processor.Start();
        baseTime = DateTime.Now.Ticks;

        Debug.Log("Now reading EMG data.");

        processor.ProcessorCallback = OnProcessed;
        started = true;
    }

    void OnApplicationQuit() {
        StopReading();

        if (outFileStream != null) {
            outFileStream.Close();
        }
    }

    public void StopReading() {
        if (!started) {
            Debug.Log("Not reading.");
            return;
        }

        processor.StopAndJoin();
        Debug.Log("STOPPED");
    }

    private void OnProcessed() {
        //RAW FFT

        List<Complex> fftResults = processor.FFTResults;

        List<Vector2> vals = new List<Vector2>(fftResults.Count);
        for (int i = 0; i < fftResults.Count; i++) {
            Vector2 val = new Vector2((float)(i * EMGProcessor.FREQ_STEP), (float)fftResults[i].Magnitude);
            vals.Add(val);
        }

        fftSeries.pointValues.SetList(vals);

        //Averaged FFT
        int bins = 16;
        int binSize = (fftResults.Count / 2) / bins; // Use only second half of FFT results
        int startIndex = fftResults.Count / 2;
        List<Vector2> postFFTVals = new List<Vector2>(bins);
        for (int i = 0; i < bins; i++) {
            Complex avg = Complex.Zero;
            for (int j = 0; j < binSize; j++) {
                int valueIdx = startIndex + (i * binSize) + j;
                avg += fftResults[valueIdx];
            }
            avg /= binSize;

            postFFTVals.Add(new Vector2(i, (float)avg.Magnitude));
        }

        postFFTSeries.pointValues.SetList(postFFTVals);  

        //Readings sliding history
        List<Vector2> readings = new List<Vector2>(readingsSeries.pointValues.list);

        for (int i = 0; i < processor.Readings.Count; i++) {
            EMGReading reading = processor.Readings[i];
            Vector2 val = new Vector2((float)(reading.timeStamp - baseTime) * 100, reading.value);
            readings.Add(val);
        }

        int count = readings.Count;
        if (count > readingsXAxisSize) {
            readings.RemoveRange(0, count - readingsXAxisSize);
        }

        readingsSeries.pointValues.SetList(readings);

    }

    void Update() {
        if (!started) {
            return;
        }

        if (Input.GetKeyUp(KeyCode.D)) {
            processor.ChangeMode(EMGProcessor.Mode.DETRENDING);
        } else if (Input.GetKeyUp(KeyCode.I)) {
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

        modeLabel.text = processor.CurrentMode.ToString();
        meanLabel.text = processor.Mean.ToString();
        predictionLabel.text = "Prediction: " + processor.PredictedMuscleState.ToString();
        trainingInfoLabel.text = "Count: " + processor.TrainingDataLength.ToString() + "\nMuscleState: " + processor.CurrentMuscleState.ToString();

        processor.Update();
    }

    public void YAxisAutoSizeEnabled(bool enabled) {
        fftGraph.yAxis.MaxAutoGrow = enabled;
        fftGraph.yAxis.MaxAutoShrink = enabled;
        fftGraph.yAxis.MinAutoGrow = enabled;
        fftGraph.yAxis.MinAutoShrink = enabled;

        readingsGraph.yAxis.MaxAutoGrow = enabled;
        readingsGraph.yAxis.MaxAutoShrink = enabled;
        readingsGraph.yAxis.MinAutoGrow = enabled;
        readingsGraph.yAxis.MinAutoShrink = enabled;

        postFFTGraph.yAxis.MaxAutoGrow = enabled;
        postFFTGraph.yAxis.MaxAutoShrink = enabled;
        postFFTGraph.yAxis.MinAutoGrow = enabled;
        postFFTGraph.yAxis.MinAutoShrink = enabled;
    }

    public void StartRecording() {
        if (recording) {
            return;
        }

        startWriteButton.interactable = false;
        stopWriteButton.interactable = true;

        string filename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".emg";
        string filepath = Application.dataPath + "/../DataSets/EMG/";

        outFileStream = File.OpenWrite(filepath + filename);
        Debug.Log("Writing file to: " + filepath + filename);

        recording = true;
        processor.OutFileStream = outFileStream;
        processor.ChangeMode(EMGProcessor.Mode.WRITING);
    }

    public void StopRecording() {
        if (!recording) {
            return;
        }

        startWriteButton.interactable = true;
        stopWriteButton.interactable = false;

        Debug.Log("Session recorded.");

        processor.ChangeMode(EMGProcessor.Mode.IDLE);
        outFileStream.Close();
        outFileStream = null;
        recording = false;
    }
}
