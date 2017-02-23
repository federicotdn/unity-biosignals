using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomSlider : MonoBehaviour {

	public Text valueText;
	public Slider slider;
	public bool halfIncrements = false;

	public float Value {
		get {
			if (halfIncrements) {
				return slider.value / 2.0f;
			}
			return slider.value;
		}

		set {
			if (halfIncrements) {
				slider.value = value * 2;
			} else {
				slider.value = value;
			}
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (halfIncrements) {
			valueText.text = (slider.value / 2.0f).ToString ();
		} else {
			valueText.text = slider.value.ToString ();
		}
	}
}
