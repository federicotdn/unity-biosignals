using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Humanoid : MonoBehaviour {

	public int maxHealth;
	public int health {
		get {
			return _health;
		}

		set {
			_health = (int)Mathf.Max (value, 0);
		}

	}

	private int _health;

	// Use this for initialization
	void Start () {
		_health = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
	}

	protected void OnStart() {
		_health = maxHealth;
	}

	public abstract void Hit(int damage, RaycastHit hit, bool hitPresent);

}
