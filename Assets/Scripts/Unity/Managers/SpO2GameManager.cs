using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public enum Spo2GameStatus {
	Playing, Paused, GameOver, PlayerWins
}

public class SpO2GameManager : MonoBehaviorSingleton<SpO2GameManager> {
	public FPSPlayer player;
	public List<int> waves;
	public int wave { get; private set; }
	public GameObject spawnLocationsContainer;
	public GameObject ammoSpawnLocationsContainer;
	public GameObject healthSpawnLocationsContainer;
	public int maxSpawnZombies = 15;
	public int minSpawnZombies = 6;
	public float minSpawnInterval = 3;
	public float maxSpawnInterval = 10;
	public float wavesInterval = 10;
	public int minHealthSpawn = 1;
	public int maxHealthSpawn = 3;
	public int minAmmoSpawn = 2;
	public int maxAmmoSpawn = 4;
	public AudioClip newWaveClip; 

	private int remainingZombies;
	private int spawnedZombies;
	private List<SpawnLocation> spawnLocations;
	private List<ItemSpawnLocation> ammoSpawnLocations;
	private List<ItemSpawnLocation> healthSpawnLocations;
	private CounterTimer spawnTimer;
	private CounterTimer waveTimer;
	private bool waveEnded;
	private int ammoCount;
	private int healthCount;
	private FirstPersonController fpsController;

	private Spo2GameStatus status;
	public Spo2GameStatus Status {
		get {
			return status;
		}

		set {
			switch (value) {
			case Spo2GameStatus.Playing:
				Time.timeScale = 1;
				fpsController.enabled = true;
				player.enabled = true;
				SpO2UIManager.Instance.Pause (false);
				SoundManager.Instance.UnPauseAudio ();
				break;
			case Spo2GameStatus.Paused:
				fpsController.enabled = false;
				player.enabled = false;
				SpO2UIManager.Instance.Pause (true);
				Time.timeScale = 0;
				SoundManager.Instance.PauseAudio ();
				break;
			case Spo2GameStatus.GameOver:
				Invoke ("GameOver", 1f);
				break;
			case Spo2GameStatus.PlayerWins:
				Invoke ("PlayerWins", 2);
				break;
			}

			status = value;
		}
	}

	// Use this for initialization
	void Start () {
		fpsController = player.GetComponent<FirstPersonController> ();
		remainingZombies = 0;
		wave = 0;
		spawnLocations = new List<SpawnLocation>(spawnLocationsContainer.GetComponentsInChildren<SpawnLocation> ());
		ammoSpawnLocations = new List<ItemSpawnLocation>(ammoSpawnLocationsContainer.GetComponentsInChildren<ItemSpawnLocation> ());
		healthSpawnLocations = new List<ItemSpawnLocation>(healthSpawnLocationsContainer.GetComponentsInChildren<ItemSpawnLocation> ());

		spawnTimer = new CounterTimer (0.5f);
		waveTimer = new CounterTimer (wavesInterval);
		Status = Spo2GameStatus.Playing;
	}
	
	// Update is called once per frame
	void Update () {
		if (Status == Spo2GameStatus.Playing) {
			if (Input.GetKeyDown (KeyCode.P)) {
				Status = Spo2GameStatus.Paused;
				return;
			}


			if (remainingZombies <= 0) {
				if (wave == waves.Count) { 
					Status = Spo2GameStatus.PlayerWins;
					return;
				} 

				wave += 1;
				SoundManager.Instance.PlayClip (newWaveClip);
				waveTimer.Reset ();
				remainingZombies = waves [wave - 1];
				waveEnded = true;
				spawnedZombies = 0;

				if (healthCount == 0) {
					healthCount = spawnItems (healthSpawnLocations, minAmmoSpawn, maxAmmoSpawn, ItemType.HEALTH);
				}

			}

			if (ammoCount == 0) {
				ammoCount = spawnItems (ammoSpawnLocations, minAmmoSpawn, maxAmmoSpawn, ItemType.AMMO);
			}

			if (waveEnded && waveTimer.Finished) {
				waveEnded = false;
				Spawn ();
			}

			if (!waveEnded && spawnTimer.Finished && spawnedZombies < waves [wave - 1]) {
				Spawn ();
			}

			spawnTimer.Update (Time.deltaTime);
			waveTimer.Update (Time.deltaTime);

			if (player.health <= 0) {
				Status = Spo2GameStatus.GameOver;
			}
		} else if (Status == Spo2GameStatus.Paused) {
			if (Input.GetKeyDown (KeyCode.P)) {
				Status = Spo2GameStatus.Playing;
				return;
			}
		}

	}

	private void Spawn() {
		int spawnNumber = Random.Range (minSpawnZombies, maxSpawnZombies + 1);
		spawnNumber = Mathf.Min (spawnNumber, remainingZombies - spawnedZombies);
		spawnedZombies += spawnNumber;
		for (int i = 0; i < spawnNumber; i++) {
			Zombie zombie = ZombieFactory.Instance.pool.Retrieve ();
			zombie.SetPlayer (player);
			SpawnLocation sl = spawnLocations [Random.Range (0, spawnLocations.Count)];
			zombie.transform.position = sl.transform.position + (Random.insideUnitSphere * sl.range);
		}
		spawnTimer = new CounterTimer(Random.Range(minSpawnInterval, maxSpawnInterval + 1));
	}

	private int spawnItems(List<ItemSpawnLocation> spawnLocations, int min, int max, ItemType type) {
		int spawnCount = Random.Range (min, max + 1);

		int i = 0;
		while (i < spawnCount) {
			ItemSpawnLocation spawnLocation = spawnLocations [Random.Range (0, spawnLocations.Count)];
			if (spawnLocation.item == null) {
				PickableItemContainer item = PickableitemFactory.Instance.pool.Retrieve ();
				spawnLocation.item = item;
				Vector3 pos = spawnLocation.transform.position;
				if (type == ItemType.AMMO) {
					item.ammo.gameObject.SetActive (true);
					pos.y += 1f;
				} else {
					item.health.gameObject.SetActive(true);
					pos.y += 0.8f;
				}
				item.transform.position = pos;
				i++;
			}
		}

		return spawnCount;
	}

	public void ItemPickedUp(PickableItemContainer item, ItemType type) {
		if (type == ItemType.AMMO) {
			ammoCount--;
		} else {
			healthCount--;
		}

		PickableitemFactory.Instance.pool.Return (item);
	}

	public void ZombieDied() {
		remainingZombies--;
	}

	private void PlayerWins() {
		Time.timeScale = 0;
		fpsController.enabled = false;
		player.enabled = false;
		SoundManager.Instance.PlayWinSong ();
		SpO2UIManager.Instance.PlayerWins ();
	}

	private void GameOver() {
		Time.timeScale = 0;
		fpsController.enabled = false;
		player.enabled = false;
		SpO2UIManager.Instance.GameOver ();
		SoundManager.Instance.PlayGameOverSong ();
	}

}
