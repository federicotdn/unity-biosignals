using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviorSingleton<MenuManager> {

	public AudioSource secondaryAudioSource;
	public AudioClip clickClip;
	public GameObject loadingPanel;

	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
		loadingPanel.SetActive (false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoadScene(string sceneName) {
		loadingPanel.SetActive (true);
		secondaryAudioSource.PlayOneShot (clickClip);
		SceneManager.LoadScene (sceneName);
	}
}