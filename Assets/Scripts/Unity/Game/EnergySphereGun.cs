using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySphereGun : MonoBehaviour {

    public const uint MAX_CHARGE_COUNTERS = 12;
    public const uint SPAWN_SPHERE_START = 1;
    public const uint SPAWN_SPHERE_THRESHOLD = 2;

    public GameObject energySpherePrefab;
    public Collider playerCollider;
    public float baseStrength = 1000.0f;

    private uint chargeCounter = 0;
    private uint chargeReached = 0;
    private EnergySphere currentSphere = null;

    // Use this for initialization
    void Start() {
        StartCoroutine(FakeEMGLoop()); // Use Keys instead of EMG for demo
    }

    void Update() {
        Vector3 forward = transform.forward.normalized;

        if (currentSphere != null) {
            currentSphere.transform.position = transform.position + forward;
        }

        if (chargeCounter == 0) {
            if (currentSphere != null) {
                Destroy(currentSphere.gameObject);
                currentSphere = null;
            }
        } else if (chargeCounter >= SPAWN_SPHERE_START && chargeCounter < MAX_CHARGE_COUNTERS) {
            if (currentSphere == null) {
                GameObject ball = Instantiate(energySpherePrefab);
                Physics.IgnoreCollision(ball.GetComponent<Collider>(), playerCollider);

                ball.transform.position = transform.position + forward;
                currentSphere = ball.GetComponent<EnergySphere>();
            }

            currentSphere.scaleFactor = ((float)chargeReached / MAX_CHARGE_COUNTERS) * EnergySphere.MAX_SCALE_FACTOR;
            currentSphere.UpdateSize();

        } else if (chargeCounter == MAX_CHARGE_COUNTERS) {
            LaunchCurrentSphere();
        }
    }

    private void LaunchCurrentSphere() {
        Vector3 forward = transform.forward.normalized;

        currentSphere.transform.position = transform.position + (forward * 0.5f);
        currentSphere.transform.rotation = transform.rotation;

        currentSphere.GetComponent<Rigidbody>().AddForce(forward * baseStrength);
        currentSphere.EnableAutoDestroy();

        chargeCounter = chargeReached = 0;
        currentSphere = null;
    }

    IEnumerator FakeEMGLoop() {
        while (true) {
            yield return new WaitForSeconds(EMGManager.EMG_TICK_DURATION); // Simulate EMGProcessor delay
            if (Input.GetKey(KeyCode.E)) {
                MuscleTenseTick();
            } else {
                MuscleRelaxedTick();
            }
        }
    }

    public void MuscleTenseTick() {
        Debug.Log("MuscleTenseTick");

        if (chargeCounter < MAX_CHARGE_COUNTERS) {
            chargeCounter++;
            chargeReached = chargeCounter;
        }

        Debug.Log("ChargeCounter: " + chargeCounter + " reached: " + chargeReached);
    }

    public void MuscleRelaxedTick() {
        Debug.Log("MuscleRelaxedTick");

        if (chargeCounter > 0) {
            chargeCounter--;

            if (chargeReached - chargeCounter >= SPAWN_SPHERE_THRESHOLD) {
                LaunchCurrentSphere();
            }

        } else {
            chargeReached = 0;
        }

        Debug.Log("ChargeCounter: " + chargeCounter + " reached: " + chargeReached);
    }
}
