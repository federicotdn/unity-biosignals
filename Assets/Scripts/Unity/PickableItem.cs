using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {
	AMMO, HEALTH
}

public class PickableItem : MonoBehaviour {

	public float speed = 1;
	public float amplitude = 0.1f;
	public ItemType type;
	public AudioClip soundEffect;

	private float y0;
	private float x0;
	// Use this for initialization
	void Start () {
		y0 = transform.position.y;
		x0 = transform.position.x;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.position;
		pos.y = y0 + (amplitude * Mathf.Sin(speed * Time.time));
		pos.x = x0 + (amplitude/2 * Mathf.Cos(speed * Time.time));
		transform.position = pos;
	}

	public void Pickup() {
		EEGGameManager.Instance.RemoveOutlineObject(GetComponent<OutlineObject>());
		Destroy (gameObject);
	}
}
