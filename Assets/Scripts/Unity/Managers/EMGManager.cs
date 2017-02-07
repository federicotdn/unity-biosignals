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

    public const float EMG_TICK_DURATION = EMGProcessor.FFT_SAMPLE_SIZE / EMGPacket.SAMPLE_RATE;

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
	
    public void StartReading() {
        if (reading) {
            Debug.LogError("EMGManager: already reading.");
            return;
        }

        if (processor == null) {
            Setup();
        }

        processor.Start();

        if (reader.HasError) {
            Debug.LogError("Error occured when starting reader.");
            processor.StopAndJoin();
            processor = null;
            return;
        }

        reading = true;
    }

    public void StopReading() {
        if (!reading) {
            Debug.LogError("EMGManager: not reading.");
        }

        processor.StopAndJoin();
        processor = null;
        reading = false;
    }
}
