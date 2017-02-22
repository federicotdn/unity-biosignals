using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Humanoid,  PoolableObject<Zombie> {

	public FPSPlayer player;
	public List<Transform> patrols;
	public NavMeshAgent Agent;
	public Animator animator;
	public List<AudioClip> deathClips;
	public List<AudioClip> GrowlClips;
	public float attackCooldown = 1;
	public float raycastInterval = 1;
	public float maxViewingDistance = 20;
	public float stoppingDistance = 1;
	public ParticleSystem deadEffect;
	public int damage = 20;
	public float hearingDistance = 10;
	public bool outlined = false;
	public float growlInterval = 0.5f;

	public AudioSource AudioSrc;

	private CounterTimer growlTimer;
	private AudioClip deathClip;
	private AudioClip growlClip;
	private CounterTimer coolDownTimer;
	private bool attacking;
	private CounterTimer raycastTimer;
	private Transform currentTarget;
	private bool playerFound;
	private bool patrolMode;
	private int patrolIndex = 0;
	private List<Collider> colliders;

	// Use this for initialization
	void Start () {
		base.OnStart ();

		if (!outlined) {
			Destroy(GetComponent<OutlineObject>());
		}

		growlClip = GrowlClips[Random.Range(0, GrowlClips.Count)];
		deathClip = deathClips[Random.Range(0, deathClips.Count)];

		growlTimer = new CounterTimer (growlInterval + Random.value);

		raycastTimer = new CounterTimer (raycastInterval);
		patrolMode = patrols.Count > 1;
		if (patrolMode) {
			currentTarget = patrols[patrolIndex];
			Agent.stoppingDistance = 0;
		} else if (player != null)  {
			currentTarget = player.transform;
			playerFound = true;
			Agent.stoppingDistance = stoppingDistance;
		}

		colliders = new List<Collider> (GetComponentsInChildren<Collider>());
	}
	
	// Update is called once per frame
	void Update () {
		if (health > 0 && currentTarget != null) {
			Vector3 dir = (player.transform.position - transform.position);

			if (patrolMode && !playerFound) {
				if (Agent.remainingDistance < 0.1) {
					patrolIndex++;
					patrolIndex %= patrols.Count;
					currentTarget = patrols [patrolIndex];
				}

				if (raycastTimer.Finished) {
					raycastTimer.Reset ();
					RaycastHit hit;
					Vector3 rayOrigin = transform.position;
					dir.y = 0.2f;
					float dot = Vector3.Dot (transform.forward, dir);
					float distance = Vector3.Distance (transform.position, player.transform.position);
					if (distance <= maxViewingDistance && Physics.Raycast (rayOrigin, dir, out hit, maxViewingDistance)) {
						if (dot >= 0 || distance <= hearingDistance) {
							FPSPlayer player = hit.collider.GetComponent<FPSPlayer> ();
							if (player != null) {
								PlayerFound ();
							}
						}
					}
				}
			} else if (playerFound) {
				dir.y = 0.2f;
				float dot = Vector3.Dot (transform.forward, dir);
				if (!attacking && Vector3.Distance(player.transform.position, transform.position) <= (stoppingDistance * 1.2f) && dot >= 0) {
					StartCoroutine (Attack ());
				}
			}
			Agent.destination = currentTarget.position;
			animator.SetFloat ("Speed", Agent.speed);

			if (growlTimer.Finished && !AudioSrc.isPlaying) {
				growlTimer.Reset ();
				AudioSrc.PlayOneShot (growlClip);
				AudioSrc.volume = 1;
				Debug.Log ("Growl");
			}

			growlTimer.Update (Time.deltaTime);

		}
		raycastTimer.Update (Time.deltaTime);
	}

	private void PlayerFound() {
		playerFound = true;
		currentTarget = player.transform;
		Agent.stoppingDistance = stoppingDistance;
	}

	public override void Hit(int damage, RaycastHit hit, bool hitPresent) {
		health -= damage;
		if (health <= 0) {
			if (EEGGameManager.IsInitialized()) {
				EEGGameManager.Instance.RemoveOutlineObject (GetComponent<OutlineObject> ());
			}
			animator.SetTrigger ("Die");
			AudioSrc.PlayOneShot (deathClip);
			if (hitPresent) {
				deadEffect.transform.position = hit.point;
				deadEffect.Play ();
			}
			Agent.enabled = false;
			Invoke ("DisableAudioSrc", deathClip.length);
			SetCollidersEnabled (false);
			if (ZombieFactory.IsInitialized()) {
				StartCoroutine (ReturnDelayed (10));
				SpO2GameManager.Instance.ZombieDied ();
			}
		} else {
			Agent.Move(-hit.normal * 0.4f);
		}
	}

	private void SetCollidersEnabled(bool enabled) {
		if (colliders != null) {
			foreach (Collider collider in colliders) {
				collider.enabled = enabled;
			}
		}
	}

	private void DisableAudioSrc() {
		AudioSrc.enabled = false;
	}

	private IEnumerator Attack() {
		attacking = true;
		animator.SetTrigger ("Attack");
		player.Hit (damage, default(RaycastHit), false);
		yield return new WaitForSeconds (attackCooldown);	
		attacking = false;

	}

	public void OnRetrieve(ExtendablePool<Zombie> pool) {
		health = maxHealth;
		Agent.enabled = true;
		AudioSrc.enabled = true;
		SetCollidersEnabled (true);
	}

	public void OnReturn() {
	}

	public void SetPlayer(FPSPlayer player) {
		this.player = player;
		currentTarget = player.transform;
		playerFound = true;
		Agent.stoppingDistance = stoppingDistance;
		Agent.destination = currentTarget.position;
	}

	IEnumerator ReturnDelayed(float delayTime)
	{
		yield return new WaitForSeconds (delayTime);
		ZombieFactory.Instance.pool.Return (this);
	}
}
