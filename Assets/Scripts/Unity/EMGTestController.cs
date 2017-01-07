using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using pfcore;
using System.Numerics;
using System;
using System.IO;

public class EMGTestController : MonoBehaviour {

    public int readingsXAxisSize = 1000;

    public string SerialPort = "COM5";
    public int maxQueueSize = 1000;
    public bool useFile = false;
    public string filePath = "";

    public WMG_Axis_Graph readingsGraph;
    public WMG_Axis_Graph fftGraph;

    private WMG_Series readingsSeries;
    private WMG_Series fftSeries;

    private EMGReader reader;
    private EMGProcessor processor;

    private bool started = false;

    private long baseTime;

	void Start () {
        if (!useFile) {
            reader = new EMGReader(SerialPort, maxQueueSize);
        } else {
            Debug.Log("Setting up file mode.");

            FileStream stream;
            try {
                stream = File.OpenRead(Application.dataPath + "/../DataSets/EMG/" + filePath);
            } catch (Exception e) {
                Debug.Log("Unable to open file: " + e);
                return;
            }
             
            reader = new EMGReader(stream, maxQueueSize);
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

        fftSeries = fftGraph.addSeries();
        readingsSeries = readingsGraph.addSeries();
        readingsSeries.pointValues.SetList(new List<Vector2>());

        processor.FFTCallback = OnFFT;
        started = true;
    }

    void OnApplicationQuit() {
        StopReading();
    }

    public void StopReading() {
        if (!started) {
            Debug.Log("Not reading.");
            return;
        }

        processor.StopAndJoin();
        Debug.Log("STOPPED");
    }

    void OnFFT() {
        Debug.Log("on fft");

        List<Vector2> vals = new List<Vector2>(processor.FFTResults.Count);
        for (int i = 0; i < processor.FFTResults.Count; i++) {
            Vector2 val = new Vector2((float)(i * EMGProcessor.FREQ_STEP), (float)processor.FFTResults[i].Real);
            vals.Add(val);
        }

        fftSeries.pointValues.SetList(vals);

        List<Vector2> readings = new List<Vector2>(readingsSeries.pointValues.list);

        for (int i = 0; i < processor.Readings.Count; i++) {
            EMGPacket packet = processor.Readings[i];
            Vector2 val = new Vector2((float)(packet.timeStamp - baseTime) * 100, (float)packet.channels[0]);
            readings.Add(val);
        }

        int count = readings.Count;
        if (count > readingsXAxisSize) {
            readings.RemoveRange(0, count - readingsXAxisSize);
        }

        readingsSeries.pointValues.SetList(readings);
    }

	void Update () {
        if (!started) {
            return;
        }

        processor.Update();
    }
}
