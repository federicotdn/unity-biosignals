using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Humanoid {

	public FPSPlayer player;
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
	public int damage = 20;
	public float hearingDistance = 4;

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
		base.OnStart ();
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
			currentTarget = player.transform;
			Agent.stoppingDistance = stoppingDistance;
		}
		animator.applyRootMotion = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (health > 0) {
			Vector3 dir = (player.transform.position - transform.position);

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
					dir.y = 0.2f;
					float dot = Vector3.Dot(transform.forward, dir);
					if (dot >= 0 && Physics.Raycast (rayOrigin, dir, out hit, maxViewingDistance)) {
						FPSPlayer player = hit.collider.GetComponent<FPSPlayer> ();
						if (player != null) {
							PlayerFound ();
						}
					}
				}
			}

			Agent.destination = currentTarget.position;
			animator.SetFloat ("Speed", Agent.speed);

			if (!attacking && Vector3.Distance(player.transform.position, transform.position) <= (stoppingDistance * 1.2f)) {
				StartCoroutine (Attack ());
			}

			if (!playerFound && Vector3.Distance(transform.position, player.transform.position) <= hearingDistance) { 
				PlayerFound ();
			}
		}
		raycastTimer.Update (Time.deltaTime);
	}

	private void PlayerFound() {
		playerFound = true;
		currentTarget = player.transform;
		Agent.stoppingDistance = stoppingDistance;
	}

	public override void Hit(int damage, RaycastHit hit, bool hitPresent) {
//		bloodEffect.transform.position = hit.point;
//		bloodEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
//		bloodEffect.Play ();
		health -= damage;
		if (health <= 0) {
			EEGGameManager.Instance.RemoveOutlineObject (GetComponent<OutlineObject>());
			animator.SetTrigger ("Die");
			AudioSrc.PlayOneShot (hitClip);
			if (hitPresent) {
				deadEffect.transform.position = hit.point;
				deadEffect.Play ();
			}
			Agent.enabled = false;
			AudioSrc.enabled = false;
		} else {
			Agent.Move(-hit.normal * 0.4f);
		}
	}

	private IEnumerator Attack() {
		attacking = true;
		animator.SetTrigger ("Attack");
		player.Hit (damage, default(RaycastHit), false);
		yield return new WaitForSeconds (attackCooldown);	
		attacking = false;

	}

}
