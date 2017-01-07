using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossHair : MonoBehaviour {

	public Image left;
	public Image right;
	public Image top;
	public Image bottom;
	public float minSeparation = 20;
	public float maxSeparation = 50;
	public Camera camera;

	// Use this for initialization
	void Start () {
		left.rectTransform.localPosition =  new Vector3(-minSeparation, 0, 0);
		right.rectTransform.localPosition = new Vector3(minSeparation, 0, 0);
		top.rectTransform.localPosition = new Vector3(0, minSeparation, 0);
		bottom.rectTransform.localPosition = new Vector3(0, -minSeparation, 0);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
