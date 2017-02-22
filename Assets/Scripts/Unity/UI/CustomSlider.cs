using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomSlider : MonoBehaviour {

	public Text valueText;
	public Slider slider;

	public int value {
		get {
			return (int)slider.value;
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
