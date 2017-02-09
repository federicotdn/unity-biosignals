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
	public float raycastInterval = 0.5f;
	public List<Collider> colliders;
	private HashSet<Humanoid> inRangeHumanoids;

	public float beepInterval = 1.5f;
	public float visibilityTime = 5;

	public bool Visible {
		get {
			return visible || !visibilityTimer.Finished;
		}
	}

	private bool visible;
	private bool exploded;
	private bool waiting = false;
	private CounterTimer visibilityTimer;
	private FPSPlayer playerInRange;
	private CounterTimer raycastTimer;

	// Use this for initialization
	void Start () {
		visibilityTimer = new CounterTimer (visibilityTime);
		visibilityTimer.Update (visibilityTime);
		inRangeHumanoids = new HashSet<Humanoid> ();
		raycastTimer = new CounterTimer (raycastInterval);
	}
	
	// Update is called once per frame
	void Update () {
		if (visible && !waiting) {
			StartCoroutine (Play());
		}

		if (!visible && visibilityTimer.Finished && mesh.enabled) {
			mesh.enabled = false;
			hitCollider.enabled = false;
		}

		if (raycastTimer.Finished && playerInRange != null && CheckVisibility (playerInRange, triggerDistance * 1.2f)) {
			Explode (playerInRange);
		}

		visibilityTimer.Update (Time.deltaTime);
		raycastTimer.Update(Time.deltaTime);
	}

	void OnTriggerEnter(Collider other) {
		FPSPlayer player = other.GetComponent<FPSPlayer> ();

		if (player != null && InRange (triggerDistance, player.transform)) {
			if (CheckVisibility (player, triggerDistance * 1.2f)) {
				Explode (player);
			} else {
				// Player is in range but behind an object
				playerInRange = player;
			}
		}

		Vector3 pos = other.transform.position;
		pos.y = transform.position.y;
		float distance = Vector3.Distance (pos, transform.position);
		if (distance >= blastRange) {
			Humanoid humanoid = player;
			HitBox hitbox = other.GetComponent<HitBox> ();
			if (hitbox != null) {
				humanoid = hitbox.Enemy;
			}
			inRangeHumanoids.Add (humanoid);
		}
	}

	private bool CheckVisibility(Humanoid humanoid, float range) {
		RaycastHit hit;
		Vector3 rayOrigin = transform.position;
		Vector3 dir = ((humanoid.transform.position + Vector3.up * 0.2f) - transform.position);
		Debug.DrawRay (rayOrigin, dir, Color.green);
		if (Physics.Raycast (rayOrigin, dir, out hit, range)) {
			if (humanoid == hit.collider.GetComponent<Humanoid> ()) {
				return true;
			}
		}

		return false;
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
		if (distance >= blastRange) {
			Humanoid humanoid = other.GetComponent<Humanoid> ();
			HitBox hitbox = other.GetComponent<HitBox> ();
			if (hitbox != null) {
				humanoid = hitbox.Enemy;
			}
			inRangeHumanoids.Remove (humanoid);
		}
	}

	public void Explode(Humanoid humanoid) {
		if (exploded) {
			return;
		}
		exploded = true;
		visible = false;
		mesh.enabled = false;
		foreach (Collider c in colliders) {
			c.enabled = false;
		}
		explosion = Instantiate(explosion, transform.position + (Vector3.up * 0.2f), Quaternion.identity);
		Destroy(explosion, 4); 
		Destroy(gameObject, 4); 
		audioSource.PlayOneShot (explosionSound);
	}

	public void PlayBeepSound() {
		visible = true;
		mesh.enabled = true;
		hitCollider.enabled = true;
	}
		

	public void StopPlaying() {
		visible = false;
		visibilityTimer.Reset ();
	}

	private IEnumerator Play() {
		audioSource.PlayOneShot (beepSound);
		waiting = true;
		yield return new WaitForSeconds (beepInterval);	
		waiting = false;
	}
}
