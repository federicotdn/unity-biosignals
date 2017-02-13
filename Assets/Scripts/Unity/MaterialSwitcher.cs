using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour {
	public Material mainMaterial;
	public Material secondaryMaterial;

	private Renderer renderer;
	private Material currentMaterial;
	// Use this for initialization
	void Start () {
		renderer = GetComponent<Renderer> ();
		currentMaterial = mainMaterial;
		renderer.material = mainMaterial;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SwitchToMain() {
		currentMaterial = mainMaterial;
		renderer.material = currentMaterial;
	}

	public void SwitchToAlternative() {
		currentMaterial = secondaryMaterial;
		renderer.material = currentMaterial;
	}
}