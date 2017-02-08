using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour {

    public float delaySeconds = 1.0f;

	void Start () {
        StartCoroutine(WaitAndDestroy());
	}

    private IEnumerator WaitAndDestroy() {
        yield return new WaitForSeconds(delaySeconds);
        Destroy(gameObject);
    }
}
