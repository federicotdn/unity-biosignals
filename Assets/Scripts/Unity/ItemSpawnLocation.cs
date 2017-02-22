using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnLocation : MonoBehaviour {

	public PickableItemContainer item;
	public ItemType type;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (item != null && ((type == ItemType.AMMO && item.ammo.pickedUp) || (type == ItemType.HEALTH && item.health.pickedUp))) {
			SpO2GameManager.Instance.ItemPickedUp (item, type);
			item = null;
		} 
	}

	protected virtual void OnDrawGizmos() {

		if (type == ItemType.AMMO) {
			Gizmos.color = Color.yellow;
		} else {
			Gizmos.color = Color.red;
		}
		Gizmos.DrawWireSphere (transform.position, 1);
	}
}
