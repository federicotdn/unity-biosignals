using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySphereGun : MonoBehaviour {

    public GameObject energySpherePrefab;
    public float baseStrength = 1000.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(KeyCode.E)) {
            GameObject ball = Instantiate(energySpherePrefab);
            Vector3 forward = transform.forward.normalized;

            ball.transform.position = transform.position + (forward * 0.5f);
            ball.transform.rotation = transform.rotation;

            ball.GetComponent<Rigidbody>().AddForce(forward * baseStrength);

            Debug.Log(forward);
        }
	}
}
