using UnityEngine;

public class EMGDemoController : MonoBehaviour {

    public GameObject cube;

	// Use this for initialization
	void Start () {
        EMGManager.Instance.Setup();
        EMGManager.Instance.StartReading();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
