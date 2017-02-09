﻿using System.Collections;
using UnityEngine;

public class EnergySphere : MonoBehaviour {

    public GameObject explosionPrefab;

    public Transform particleTransform;
    public SphereCollider sphereCollider;

    public const float MAX_LIFE_TIME = 4.0f;
    public const float START_RADIUS = 0.3f;

    private bool exploded = false;

    private float scaleFactor = 1.0f;
    public const float MAX_SCALE_FACTOR = 2.0f;
    private float lastScale;

    private float elapsedTime;
    private float targetScale;
    private float transitionDuration;

	void Start () {
        lastScale = scaleFactor;
        sphereCollider.radius = START_RADIUS;
    }

    public void EnableAutoDestroy() {
        StartCoroutine(WaitAndExplode(MAX_LIFE_TIME));
    }

    public void SetScale(float scale, float transitionDurationSec) {
        elapsedTime = 0;
        targetScale = scale;
        lastScale = scaleFactor;
        transitionDuration = transitionDurationSec;
    }

    public void SetScale(float newScale) {
        scaleFactor = newScale;
        sphereCollider.radius = START_RADIUS * scaleFactor;
        particleTransform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }

    void Update() {
        if (Mathf.Approximately(scaleFactor, targetScale)) {
            return;
        }

        elapsedTime += Time.deltaTime;

        if (elapsedTime <= transitionDuration) {
            float newScale = Mathf.Lerp(lastScale, targetScale, elapsedTime / transitionDuration);
            SetScale(newScale);
        }
    }

    IEnumerator WaitAndExplode(float seconds) {
        yield return new WaitForSeconds(seconds);
        Explode();
    }

    private void Explode() {
        if (exploded) {
            return;
        }

        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = particleTransform.localScale;
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