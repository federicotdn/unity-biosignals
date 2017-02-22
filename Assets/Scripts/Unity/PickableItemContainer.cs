using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableItemContainer : MonoBehaviour, PoolableObject<PickableItemContainer> {
	public PickableItem ammo;
	public PickableItem health;


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	}

	public void OnRetrieve(ExtendablePool<PickableItemContainer> pool) {
		ammo.gameObject.SetActive (false);
		health.gameObject.SetActive (false);
	}

	public void OnReturn() {
		ammo.pickedUp = false;
		health.pickedUp = false;
		ammo.gameObject.SetActive (false);
		health.gameObject.SetActive (false);
	}
}
