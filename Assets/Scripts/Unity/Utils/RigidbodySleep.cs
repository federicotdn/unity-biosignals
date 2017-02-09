using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodySleep : MonoBehaviour {
	void Start () {
        GetComponent<Rigidbody>().Sleep();
	}
}
