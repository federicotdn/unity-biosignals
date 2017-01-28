using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using pfcore;
using System.Numerics;
using System;
using UnityEngine.UI;

public class EEGTestController : MonoBehaviour {

	public int port = 5005;

	public WMG_Axis_Graph alphaGraph;
	public WMG_Axis_Graph fftGraph;

	public WMG_Series alphaSeries;
	public WMG_Series fftSeries;
	public int readingsXAxisSize = 100;

	private EEGReader reader;
	private EEGProcessor processor;
	private bool started;
	private long baseTime;


	// Use this for initialization
	void Start () {
		reader = new EEGReader (port);
		processor = new EEGProcessor (reader, true);
	}

	public void StartReading() {
		if (started) {
			Debug.Log("Already reading EEG data.");
			return;
		}

		processor.Start();
		processor.Training = true;
		baseTime = DateTime.Now.Ticks;

		Debug.Log("Now reading EEG data.");

		processor.ProcessorCallback = OnFFT;
		started = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (!started) {
			return;
		}

		processor.Update ();
	}

	void OnFFT() {
		List<Vector2> vals = new List<Vector2>(processor.FFTResults.Count);

		for (int i = 0; i < 56; i++) {
			Vector2 val = new Vector2((float)(i * EEGProcessor.FREQ_STEP), Mathf.Log10((float)processor.FFTResults[i].Magnitude));
			vals.Add(val);
		}

		fftSeries.pointValues.SetList(vals);

		List<Vector2> readings = new List<Vector2>(alphaSeries.pointValues.list);

		for (int i = 0; i < processor.AlphaReadings.Count; i++) {
			EEGReading reading = processor.AlphaReadings[i];
			Vector2 val = new Vector2((float)(reading.timeStamp - baseTime) * 100, reading.value);
			readings.Add(val);
		}

		int count = readings.Count;
		if (count > readingsXAxisSize) {
			readings.RemoveRange(0, count - readingsXAxisSize);
		}

		alphaSeries.pointValues.SetList(readings);

	}

	public void YAxisAutoSizeEnabled(bool enabled) {
		fftGraph.yAxis.MaxAutoGrow = enabled;
		fftGraph.yAxis.MaxAutoShrink = enabled;
		fftGraph.yAxis.MinAutoGrow = enabled;
		fftGraph.yAxis.MinAutoShrink = enabled;

		alphaGraph.yAxis.MaxAutoGrow = enabled;
		alphaGraph.yAxis.MaxAutoShrink = enabled;
		alphaGraph.yAxis.MinAutoGrow = enabled;
		alphaGraph.yAxis.MinAutoShrink = enabled;
	}
}
