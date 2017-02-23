using UnityEngine;

public abstract class EMGWeapon : MonoBehaviour {
    public abstract void MuscleTenseTick();
    public abstract void MuscleRelaxedTick();
}
