using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomSlider : MonoBehaviour {

	public Text valueText;
	public Slider slider;

	public int Value {
		get {
			return (int)slider.value;
		}

		set {
			slider.value = value;
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		valueText.text = slider.value.ToString ();
	}
}
