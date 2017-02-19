using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using pfcore;
using System.Numerics;
using System;
using UnityEngine.UI;

public class EEGTestController : MonoBehaviour {

	public int port = 5005;

	public WMG_Axis_Graph fftGraph;
	public WMG_Axis_Graph tp9Graph;
	public WMG_Axis_Graph af7Graph;
	public WMG_Axis_Graph af8Graph;
	public WMG_Axis_Graph tp10Graph;

	public WMG_Series fftSeries;
	public WMG_Series tp9Series;
	public WMG_Series af7Series;
	public WMG_Series af8Series;
	public WMG_Series tp10Series;

	public int readingsXAxisSize = 100;

	public bool useFile;
	public string filepath;

	private EEGReader reader;
	private EEGProcessor processor;
	private bool started;
	private long baseTime;


	// Use this for initialization
	void Start () {
		if (useFile) {
			reader = new EEGFileReader (filepath, false);
		} else {
			reader = new EEGOSCReader (port);
		}
		processor = new EEGProcessor (reader);
	}

	public void StartReading() {
		if (started) {
			Debug.Log("Already reading EEG data.");
			return;
		}

		processor.Start();
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
		AddFFTToSeries (processor.FFTResults, fftSeries);
		AddFFTToSeries (processor.TP9FFT, tp9Series);
		AddFFTToSeries (processor.AF7FFT, af7Series);
		AddFFTToSeries (processor.AF8FFT, af8Series);
		AddFFTToSeries (processor.TP10FFT, tp10Series);
	}

	private void AddFFTToSeries(List<Complex> fft, WMG_Series series) {
		List<Vector2> vals = new List<Vector2>(fft.Count);

		for (int i = 0; i < 56; i++) {
			Vector2 val = new Vector2((float)(i * EEGProcessor.FREQ_STEP), Mathf.Log10((float)fft[i].Magnitude));
			vals.Add(val);
		}

		series.pointValues.SetList(vals);
	}

	public void YAxisAutoSizeEnabled(bool enabled) {
		YAxisAutoSizeEnabled (fftGraph, enabled);
		YAxisAutoSizeEnabled (tp9Graph, enabled);
		YAxisAutoSizeEnabled (af7Graph, enabled);
		YAxisAutoSizeEnabled (af8Graph, enabled);
		YAxisAutoSizeEnabled (tp10Graph, enabled);
	}

	private void YAxisAutoSizeEnabled(WMG_Axis_Graph graph, bool enabled) {
		graph.yAxis.MaxAutoGrow = enabled;
		graph.yAxis.MaxAutoShrink = enabled;
		graph.yAxis.MinAutoGrow = enabled;
		graph.yAxis.MinAutoShrink = enabled;
	}
}
