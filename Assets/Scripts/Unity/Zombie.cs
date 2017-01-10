using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour {

	public Transform Target;
	public NavMeshAgent Agent;
	public Animator animator;
	public float Health = 100;
	public List<AudioClip> GrowlClips;
	public List<AudioClip> HitClips;

	public AudioSource AudioSrc;

	private AudioClip hitClip;
	private AudioClip growlClip;

	// Use this for initialization
	void Start () {
		growlClip = GrowlClips[Random.Range(0, GrowlClips.Count)];
		hitClip = HitClips[Random.Range(0, HitClips.Count)];
		AudioSrc.clip = growlClip;
		AudioSrc.loop = true;
		AudioSrc.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Health > 0) {
			Agent.destination = Target.position;
			animator.SetFloat ("Speed", Agent.speed);
		}
	}

	public void Hit(float damage) {
		Health = Mathf.Max (0, Health - damage);
		if (Health <= 0) {
			animator.SetTrigger ("Die");
			AudioSrc.PlayOneShot (hitClip);
			Agent.Stop ();
		}
	}

}
