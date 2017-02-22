using pfcore;
using System;
using System.IO;
using UnityEngine;

public class EMGManager : MonoBehaviorSingleton<EMGManager> {

    public string portName;
    public int maxQueueSize = 1000;
    public bool useFile = false;

    [Header("File path, relative to Application.dataPath")]
    public string filePath = "";

    private EMGProcessor processor = null;
    public EMGProcessor Processor {
        get {
            return processor;
        }
    }

    private EMGReader reader = null;
    private bool reading = false;

    public const float EMG_TICK_DURATION = (float)EMGProcessor.FFT_SAMPLE_SIZE / EMGPacket.SAMPLE_RATE;

	public void Setup() {
        if (!useFile) {
            reader = new EMGSerialReader(portName, maxQueueSize);
        } else {
            FileStream stream;
            try {
                stream = File.OpenRead(Application.dataPath + "/" + filePath);
            } catch (Exception e) {
                Debug.LogError("EMGManager: Unable to read file: " + e);
                return;
            }

            EMGFileReader fileReader = new EMGFileReader(stream, maxQueueSize);
            fileReader.EnableFileLoop();
            reader = fileReader;
        }

        processor = new EMGProcessor(reader);
    }

    void Update() {
        if (reading && processor != null) {
            processor.Update();
        }
    }

    public void StartReading() {
        if (reading) {
            Debug.Log("EMGManager: already reading.");
            return;
        }

        if (processor == null) {
            Setup();
        }

        processor.Start();

        if (reader.HasError) {
            Debug.Log("Error occured when starting reader.");
            processor.StopAndJoin();
            processor = null;
            return;
        }

        reading = true;
    }

    protected override void Destroy() {
        base.Destroy();
        StopReading();
    }

    public void StopReading() {
        if (!reading) {
            Debug.Log("EMGManager: not reading.");
            return;
        }

        processor.StopAndJoin();
        processor = null;
        reading = false;
    }
}
