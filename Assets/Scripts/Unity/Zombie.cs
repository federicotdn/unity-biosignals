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
	public float attackCooldown = 1;

	public AudioSource AudioSrc;

	private AudioClip hitClip;
	private AudioClip growlClip;
	private CounterTimer coolDownTimer;
	private bool attacking;

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

			Debug.Log (Agent.remainingDistance);

			if (!attacking && Agent.remainingDistance <= (Agent.stoppingDistance + 0.1)) {
				StartCoroutine (Attack ());
			}
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

	private IEnumerator Attack() {
		attacking = true;
		animator.SetTrigger ("Attack");
		yield return new WaitForSeconds (attackCooldown);	
		Debug.Log ("Attack");
		attacking = false;
	}

}
