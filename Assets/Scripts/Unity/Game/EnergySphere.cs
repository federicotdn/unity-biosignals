using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySphere : MonoBehaviour {

    public Transform particleTransform;
    public SphereCollider sphereCollider;

    public const float MAX_LIFE_TIME = 4.0f;
    public const float START_RADIUS = 0.3f;

    private bool exploded = false;

    public float scaleFactor = 1.0f;
    public const float MAX_SCALE_FACTOR = 2.0f;
    private float lastScale;

	void Start () {
        lastScale = scaleFactor;
        sphereCollider.radius = START_RADIUS;
    }

    public void EnableAutoDestroy() {
        StartCoroutine(WaitAndExplode(MAX_LIFE_TIME));
    }

    void Update() {
        UpdateSize();
    }

    public void UpdateSize() {
        if (!Mathf.Approximately(scaleFactor, lastScale)) {
            sphereCollider.radius = START_RADIUS * scaleFactor;
            particleTransform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            lastScale = scaleFactor;
        }
    }

    IEnumerator WaitAndExplode(float seconds) {
        yield return new WaitForSeconds(seconds);
      //  Explode();
    }

    private void Explode() {
        if (exploded) {
            return;
        }

        exploded = true;
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.GetComponent<EnergySphere>() != null) {
            return;
        }

        Explode();
        Debug.Log("collision");
    }
}
