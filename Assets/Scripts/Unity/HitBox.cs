﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour {

	public int Damage;
	public Zombie Enemy;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Hit(RaycastHit hit) {
		Enemy.Hit (Damage, hit, true);
	}
}
