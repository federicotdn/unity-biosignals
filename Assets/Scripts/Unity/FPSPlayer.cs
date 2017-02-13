using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSPlayer : Humanoid {

	public Animator Animator;
	public AudioClip GunShotClip;
	public AudioClip ReloadClip;
	public AudioClip GunEmptyClip;
	public AudioSource gunAudioSource;
	public AudioSource MainAudioSource;
	public float CoolDown;
	public float ReloadTime;
	public int MagSize = 12;
	public int rounds { get; private set; }
	public Camera FPSCam;
	public float Range = 30;
	public ParticleSystem shellEffect;
	public List<AudioClip> hurtClips;

	private CounterTimer coolDownTimer;
	private CounterTimer reloadTimer;

	void Start () {
		base.OnStart ();
		coolDownTimer = new CounterTimer(CoolDown);
		coolDownTimer.Update(CoolDown);

		reloadTimer = new CounterTimer(ReloadTime);
		reloadTimer.Update(ReloadTime);
		rounds = MagSize;
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.F)) {
			FPSUI.Instance.Flash ();
		}

		coolDownTimer.Update (Time.deltaTime);
		reloadTimer.Update (Time.deltaTime); 

		if (Input.GetKeyDown (KeyCode.R) && reloadTimer.Finished) {
			gunAudioSource.PlayOneShot (ReloadClip);
			reloadTimer.Reset ();
			rounds = MagSize;
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
				Vector3 rayOrigin = FPSCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));

				RaycastHit hit;

				if (Physics.Raycast (rayOrigin, FPSCam.transform.forward, out hit, Range))
				{

					HitBox hitBox = hit.collider.GetComponent<HitBox>();
					// If there was a health script attached
					if (hitBox != null) {
						// Call the damage function of that script, passing in our gunDamage variable
						hitBox.Hit (hit);
					} else {
						BombHitBox bombHitBox = hit.collider.GetComponent<BombHitBox> ();
						if (bombHitBox != null) { 
							Bomb bomb = bombHitBox.transform.parent.gameObject.GetComponent<Bomb>();
							bomb.Explode ();
						}

					}
				}
			}
		}
	}
		

	public override void Hit(int damage, RaycastHit hit, bool hitPresent) {
		health -= damage;
		FPSUI.Instance.Flash ();
		MainAudioSource.PlayOneShot (hurtClips[Random.Range(0, hurtClips.Count)]);
		GetComponent<CharacterController> ().Move (-transform.forward * 0.5f);
	}
}
