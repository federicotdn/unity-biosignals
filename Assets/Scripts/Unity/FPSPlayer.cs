using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSPlayer : MonoBehaviour {

	public Animator Animator;
	public AudioClip GunShotClip;
	public AudioClip ReloadClip;
	public AudioClip GunEmptyClip;
	public AudioSource MainAudioSource;
	public float CoolDown;
	public float ReloadTime;
	public int MagSize = 12;
	public int rounds { get; private set; }

	private CounterTimer coolDownTimer;
	private CounterTimer reloadTimer;


	// Use this for initialization

	void Awake() {
		coolDownTimer = new CounterTimer(CoolDown);
		coolDownTimer.Update(CoolDown);

		reloadTimer = new CounterTimer(ReloadTime);
		reloadTimer.Update(ReloadTime);
		rounds = MagSize;
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		coolDownTimer.Update (Time.deltaTime);
		reloadTimer.Update (Time.deltaTime); 

		if (Input.GetKeyDown (KeyCode.R) && reloadTimer.Finished) {
			MainAudioSource.PlayOneShot (ReloadClip);
			reloadTimer.Reset ();
			rounds = MagSize;
		}

		if (reloadTimer.Finished && coolDownTimer.Finished && Input.GetMouseButtonDown (0)) {
			if (rounds <= 0) {
				MainAudioSource.PlayOneShot (GunEmptyClip);
			} else {
				Animator.SetTrigger ("Fire");
				coolDownTimer.Reset ();
				MainAudioSource.PlayOneShot (GunShotClip);
				rounds--;
				LookManager.Instance.Recoil ();
			}
		}
	}
}
