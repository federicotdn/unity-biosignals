using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Humanoid {

	public Transform mainTarget;
	public List<Transform> patrols;
	public NavMeshAgent Agent;
	public Animator animator;
	public List<AudioClip> GrowlClips;
	public List<AudioClip> HitClips;
	public float attackCooldown = 1;
	public float raycastInterval = 0.5f;
	public float maxViewingDistance = 30;
	public float stoppingDistance = 1;
	public ParticleSystem bulletImpactEffect;
	public ParticleSystem deadEffect;

	public AudioSource AudioSrc;

	private AudioClip hitClip;
	private AudioClip growlClip;
	private CounterTimer coolDownTimer;
	private bool attacking;
	private CounterTimer raycastTimer;
	private Transform currentTarget;
	private bool playerFound;
	private bool patrolMode;
	private int patrolIndex = 0;

	// Use this for initialization
	void Start () {
		growlClip = GrowlClips[Random.Range(0, GrowlClips.Count)];
		hitClip = HitClips[Random.Range(0, HitClips.Count)];
		AudioSrc.clip = growlClip;
		AudioSrc.loop = true;
		AudioSrc.Play ();
		raycastTimer = new CounterTimer (raycastInterval);
		patrolMode = patrols.Count > 1;
		if (patrolMode) {
			currentTarget = patrols[patrolIndex];
			Agent.stoppingDistance = 0;
		} else {
			currentTarget = mainTarget;
			Agent.stoppingDistance = stoppingDistance;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Health > 0) {
			if (patrolMode && !playerFound) {
				if (Agent.remainingDistance < 0.1) {
					patrolIndex++;
					patrolIndex %= patrols.Count;
					currentTarget = patrols[patrolIndex];
				}

				if (raycastTimer.Finished) {
					raycastTimer.Reset ();
					RaycastHit hit;
					Vector3 rayOrigin = transform.position;
					Vector3 dir = (mainTarget.position - transform.position);
					dir.y = 0.2f;
					float dot = Vector3.Dot(transform.forward, dir);
					if (dot >= 0 && Physics.Raycast (rayOrigin, dir, out hit, maxViewingDistance)) {
						FPSPlayer player = hit.collider.GetComponent<FPSPlayer> ();
						if (player != null) {
							playerFound = true;
							currentTarget = mainTarget;
							Agent.stoppingDistance = stoppingDistance;
							Debug.Log ("FOUND");
						}
					}
				}
			}
		

			Agent.destination = currentTarget.position;
			animator.SetFloat ("Speed", Agent.speed);

			if (currentTarget == mainTarget && !attacking && Agent.remainingDistance <= (Agent.stoppingDistance + 0.1)) {
				StartCoroutine (Attack ());
			}
		}
		raycastTimer.Update (Time.deltaTime);
	}

	public void Hit(float damage, RaycastHit hit) {
//		bloodEffect.transform.position = hit.point;
//		bloodEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
//		bloodEffect.Play ();
		Health = (int)Mathf.Max (0, Health - damage);
		if (Health <= 0) {
			animator.SetTrigger ("Die");
			AudioSrc.PlayOneShot (hitClip);
			Agent.Stop ();
			deadEffect.transform.position = hit.point;
			deadEffect.Play ();
			Destroy (gameObject, 3);
		}
	}

	private IEnumerator Attack() {
		attacking = true;
		animator.SetTrigger ("Attack");
		yield return new WaitForSeconds (attackCooldown);	
		attacking = false;
	}

}
