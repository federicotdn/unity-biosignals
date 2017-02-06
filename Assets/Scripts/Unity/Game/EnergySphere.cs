using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySphere : MonoBehaviour {

    public float maxLifeTime = 4.0f;
    public float explosionTime = 0.1f;

    private bool exploding = false;

	void Start () {
        StartCoroutine(WaitAndExplode(maxLifeTime));
    }

    IEnumerator WaitAndExplode(float seconds) {
        yield return new WaitForSeconds(seconds);
        Explode();
    }

    IEnumerator WaitAndDestroy(float seconds) {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }

    private void Explode() {
        if (exploding) {
            return;
        }

        exploding = true;


        StartCoroutine(WaitAndDestroy(explosionTime));
    }

    void OnCollisionEnter(Collision collision) {
        Explode();
    }
}
