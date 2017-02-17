using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {
	public GameObject explosion;
	public AudioSource audioSource;
	public AudioClip beepSound;
	public AudioClip explosionSound;
	public MeshRenderer mesh;
	public Collider hitCollider;
	public float triggerDistance;
	public float blastRange;
	public float raycastInterval = 0.8f;
	public List<Collider> colliders;
	private HashSet<Humanoid> inRangeHumanoids;

	public float beepInterval = 1.5f;

	private bool playing;
	private bool exploded;
	private bool waiting = false;
	private FPSPlayer playerInRange;
	private CounterTimer raycastTimer;
	private OutlineObject outline;

	// Use this for initialization
	void Start () {
		inRangeHumanoids = new HashSet<Humanoid> ();
		raycastTimer = new CounterTimer (raycastInterval);
		mesh.enabled = false;
		outline = GetComponent<OutlineObject> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (playing && !waiting) {
			StartCoroutine (Play());
		}

		if (outline.Outline && !exploded) {
			hitCollider.enabled = true;
			mesh.enabled = true;
		}

		if (raycastTimer.Finished && playerInRange != null && CheckVisibility (playerInRange, triggerDistance * 1.2f)) {
			Explode ();
		}

		raycastTimer.Update(Time.deltaTime);
	}

	void OnTriggerEnter(Collider other) {
		FPSPlayer player = other.GetComponent<FPSPlayer> ();

		if (player != null && InRange (triggerDistance, player.transform)) {
			if (CheckVisibility (player, triggerDistance * 1.2f)) {
				Explode ();
			} else {
				// Player is in range but behind an object
				playerInRange = player;
			}
		}
			
		Humanoid humanoid = player;
		HitBox hitbox = other.GetComponent<HitBox> ();
		if (hitbox != null) {
			humanoid = hitbox.Enemy;
		} 
		if (humanoid != null && !inRangeHumanoids.Contains (humanoid)) {
			inRangeHumanoids.Add (humanoid);
		}
	}

	private bool CheckVisibility(Humanoid humanoid, float range) {
		if (humanoid == null) {
			return false;
		}

		RaycastHit hit;
		Vector3 rayOrigin = transform.position;
		Vector3 dir = ((humanoid.transform.position + Vector3.up * 0.2f) - transform.position);
		rayOrigin += dir.normalized;
		if (Physics.Raycast (rayOrigin, dir, out hit, range)) {
			return hit.collider.gameObject.layer != LayerMask.NameToLayer ("Wall");
		}

		return true;
	}

	private bool InRange(float range, Transform t) {
		Vector3 pos = t.position;
		pos.y = transform.position.y;
		float distance = Vector3.Distance (pos, transform.position);
		return distance <= (range * 1.1f);
	}

	void OnTriggerExit(Collider other) {
		Vector3 pos = other.transform.position;
		pos.y = transform.position.y;
		float distance = Vector3.Distance (pos, transform.position);

		FPSPlayer player = other.GetComponent<FPSPlayer> ();
		if (distance >= blastRange) {
			if (player != null) {
				playerInRange = null;
				inRangeHumanoids.Remove (player);
				return;
			}
			HitBox hitbox = other.GetComponent<HitBox> ();
			if (hitbox != null) {
				Zombie zombie = hitbox.Enemy;
				inRangeHumanoids.Remove (zombie);
			}
		} else if (player != null) {
			playerInRange = null;
		}
	}

	public void Explode() {
		if (exploded) {
			return;
		}

		foreach (Humanoid humanoid in inRangeHumanoids) {
			Hit (humanoid);
		}

		exploded = true;
		playing = false;
		mesh.enabled = false;
		foreach (Collider c in colliders) {
			c.enabled = false;
		}
		explosion = Instantiate(explosion, transform.position, Quaternion.identity);
		EEGGameManager.Instance.RemoveBomb (this);
		Destroy(explosion, 3.9f); 
		Destroy(gameObject, 4); 
		audioSource.PlayOneShot (explosionSound);
	}

	private void Hit(Humanoid humanoid) {
		Vector3 pos = humanoid.transform.position;
		pos.y = transform.position.y;
		float distance = Vector3.Distance (pos, transform.position);
		if (CheckVisibility(humanoid, blastRange) && distance <= blastRange) {
			float damage = (blastRange - distance) / blastRange;
			humanoid.Hit ((int)(humanoid.maxHealth * 2 * damage), default(RaycastHit), false);
		}
	}

	public void PlayBeepSound() {
		playing = true;
	}
		

	public void StopPlaying() {
		playing = false;
	}

	private IEnumerator Play() {
		audioSource.PlayOneShot (beepSound);
		waiting = true;
		yield return new WaitForSeconds (beepInterval);	
		waiting = false;
	}
}
