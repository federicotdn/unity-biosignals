using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    [HideInInspector]
    public Rigidbody body;

	void Start () {
        body = GetComponent<Rigidbody>();
	}
}
