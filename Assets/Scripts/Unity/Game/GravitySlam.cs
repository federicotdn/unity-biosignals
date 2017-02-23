using UnityEngine;

public class GravitySlam : EMGWeapon {

    public const uint MAX_CHARGE_COUNTERS = 16;
    public const uint LIFT_OBJECTS_START = 1;
    public const uint SLAM_OBJECTS_THRESHOLD = 2;

    public float baseStrength = 3000.0f;
    private PhysicsObject[] liftedObjects = null;

    private uint chargeCounter = 0;
    private uint chargeReached = 0;

    void Update() {
        if (chargeCounter == 0) {
            if (liftedObjects != null) {
                DoCleanup();
            }
        } else if (chargeCounter >= LIFT_OBJECTS_START && chargeCounter < MAX_CHARGE_COUNTERS) {
            if (liftedObjects == null) {
                liftedObjects = FindObjectsOfType<PhysicsObject>();
                foreach (PhysicsObject obj in liftedObjects) {
                    obj.body.useGravity = false;
                    obj.body.AddForce(new Vector3(0, 100, 0));
                    float rotation = Random.Range(0.0f, 1.0f) * 100;
                    obj.body.AddTorque(new Vector3(rotation, rotation, rotation));
                }
            }
        } else if (chargeCounter == MAX_CHARGE_COUNTERS) {
            SlamObjects();
        }
    }

    private void SlamObjects() {
        float factor = ((float)chargeReached / MAX_CHARGE_COUNTERS);
        foreach (PhysicsObject obj in liftedObjects) {
            obj.body.useGravity = true;
            obj.body.AddForce(new Vector3(0, -baseStrength * factor, 0));
        }
        DoCleanup();
    }
    
    private void DoCleanup() {
        foreach (PhysicsObject obj in liftedObjects) {
            obj.body.useGravity = true;
        }
        liftedObjects = null;
        chargeCounter = chargeReached = 0;
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

            if (chargeReached - chargeCounter >= SLAM_OBJECTS_THRESHOLD) {
                SlamObjects();
            }

        } else {
            chargeReached = 0;
        }
    }
}
