using UnityEngine;

public class EnergySphereGun : EMGWeapon {

    public const uint MAX_CHARGE_COUNTERS = 12;
    public const uint SPAWN_SPHERE_START = 1;
    public const uint SPAWN_SPHERE_THRESHOLD = 2;

    public GameObject energySpherePrefab;
    public Collider playerCollider;
    public float baseStrength = 1000.0f;

    private uint chargeCounter = 0;
    private uint chargeReached = 0;
    private EnergySphere currentSphere = null;

    void Update() {
        if (currentSphere != null) {
            Vector3 forward = transform.forward.normalized;
            currentSphere.transform.position = transform.position + forward;
        }

        if (chargeCounter == 0) {
            if (currentSphere != null) {
                Destroy(currentSphere.gameObject);
                currentSphere = null;
            }
        } else if (chargeCounter >= SPAWN_SPHERE_START && chargeCounter < MAX_CHARGE_COUNTERS) {
            float newScale = GetMultiplier();

            if (currentSphere == null) {
                CreateSphere();
                currentSphere.SetScale(newScale);
            } else {
                currentSphere.SetScale(newScale, EMGManager.EMG_TICK_DURATION);
            }
        } else if (chargeCounter == MAX_CHARGE_COUNTERS) {
            LaunchCurrentSphere();
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            chargeCounter = chargeReached = 1;
            CreateSphere();
            currentSphere.SetScale(0.1f);
            LaunchCurrentSphere();
        }
    }

    private float GetMultiplier() {
        return ((float)chargeReached / MAX_CHARGE_COUNTERS) * EnergySphere.MAX_SCALE_FACTOR;
    }

    private void CreateSphere() {
        Vector3 forward = transform.forward.normalized;
        GameObject ball = Instantiate(energySpherePrefab);
        Physics.IgnoreCollision(ball.GetComponent<Collider>(), playerCollider);

        ball.transform.position = transform.position + forward;
        currentSphere = ball.GetComponent<EnergySphere>();
    }

    private void LaunchCurrentSphere() {
        Vector3 forward = transform.forward.normalized;

        currentSphere.transform.position = transform.position + (forward * 0.5f);
        currentSphere.transform.rotation = transform.rotation;

        currentSphere.GetComponent<Rigidbody>().AddForce(forward * baseStrength * GetMultiplier());
        currentSphere.EnableAutoDestroy();

        chargeCounter = chargeReached = 0;
        currentSphere = null;
    }

    public override void MuscleTenseTick() {
        if (chargeCounter < MAX_CHARGE_COUNTERS) {
            chargeCounter++;
            chargeReached = chargeCounter;
        }
    }

    public override void MuscleRelaxedTick() {
        if (chargeCounter > 0) {
            chargeCounter--;

            if (chargeReached - chargeCounter >= SPAWN_SPHERE_THRESHOLD) {
                LaunchCurrentSphere();
            }

        } else {
            chargeReached = 0;
        }
    }
}
