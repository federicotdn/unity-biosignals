using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets;
using UnityStandardAssets.Characters.FirstPerson;

public class FPSPlayer : Humanoid {

	public Animator Animator;
	public AudioClip GunShotClip;
	public AudioClip ReloadClip;
	public AudioClip GunEmptyClip;
	public AudioClip ammoPickupClip;
	public AudioClip flashlightClip;
	public AudioSource gunAudioSource;
	public AudioSource MainAudioSource;
	public float CoolDown;
	public float ReloadTime;
	public int magsCount = 3;
	public int remainingRounds { get; private set; }
	public int magSize = 12;
	public int rounds { get; private set; }
	public Camera FPSCam;
	public float Range = 30;
	public ParticleSystem shellEffect;
	public List<AudioClip> hurtClips;
	public KeyCode reloadKey;
	public Light flashlight;

	private CounterTimer coolDownTimer;
	private CounterTimer reloadTimer;
	private FirstPersonController fpsController;

	void Start () {
		base.OnStart ();
		coolDownTimer = new CounterTimer(CoolDown);
		coolDownTimer.Update(CoolDown);

		reloadTimer = new CounterTimer(ReloadTime);
		reloadTimer.Update(ReloadTime);
		rounds = magSize;
		remainingRounds = (magsCount - 1) * magSize;
		fpsController = GetComponent<FirstPersonController> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (EEGGameManager.Instance.Status == GameStatus.Playing) {
			fpsController.enabled = true;
			coolDownTimer.Update (Time.deltaTime);
			reloadTimer.Update (Time.deltaTime); 

			if (Input.GetKeyDown (KeyCode.L)) {
				flashlight.enabled = !flashlight.enabled;
				MainAudioSource.PlayOneShot (flashlightClip);
			}

			if (Input.GetKeyDown (reloadKey) && reloadTimer.Finished) {
				if (remainingRounds >= 1) {
					gunAudioSource.PlayOneShot (ReloadClip);
					reloadTimer.Reset ();
					int reloadCount = Mathf.Min (magSize - rounds, remainingRounds);
					rounds += reloadCount;
					remainingRounds -= reloadCount;
				} else {
					gunAudioSource.PlayOneShot (GunEmptyClip);
				}

			}

			if (reloadTimer.Finished && coolDownTimer.Finished && Input.GetMouseButtonDown (0)) {
				if (rounds <= 0) {
					gunAudioSource.PlayOneShot (GunEmptyClip);
				} else {
					Animator.SetTrigger ("Fire");
					shellEffect.Play ();
					coolDownTimer.Reset ();
					gunAudioSource.PlayOneShot (GunShotClip);
					rounds--;
					LookManager.Instance.Recoil ();
					Vector3 rayOrigin = FPSCam.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0.0f));

					RaycastHit hit;

					if (Physics.Raycast (rayOrigin, FPSCam.transform.forward, out hit, Range)) {

						HitBox hitBox = hit.collider.GetComponent<HitBox> ();
						// If there was a health script attached
						if (hitBox != null) {
							// Call the damage function of that script, passing in our gunDamage variable
							hitBox.Hit (hit);
						} else {
							BombHitBox bombHitBox = hit.collider.GetComponent<BombHitBox> ();
							if (bombHitBox != null) { 
								Bomb bomb = bombHitBox.transform.parent.gameObject.GetComponent<Bomb> ();
								bomb.Explode ();
							}
						}
					}
				}
			}
		} else {
			fpsController.enabled = false;
		}
	}
		

	public override void Hit(int damage, RaycastHit hit, bool hitPresent) {
		health -= damage;
		MainAudioSource.PlayOneShot (hurtClips[Random.Range(0, hurtClips.Count)]);
		GetComponent<CharacterController> ().Move (-transform.forward * 0.5f);
	}

	void OnTriggerEnter(Collider other) {
		PickableItem item = other.GetComponent<PickableItem> ();
		if (item != null) {
			switch (item.type) {
			case ItemType.AMMO:
				if ((magsCount * magSize) != +rounds + remainingRounds) {
					remainingRounds = (magsCount * magSize) - rounds;
					item.Pickup ();
					MainAudioSource.PlayOneShot (item.soundEffect);
				}
				break;
			case ItemType.HEALTH:
				if (health != maxHealth) {
					MainAudioSource.PlayOneShot (item.soundEffect);
					health = maxHealth;
					item.Pickup ();
				}
				break;
			}

		}
	}
}
