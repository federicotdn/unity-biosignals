using UnityEngine;

public class PrefabSpawner : MonoBehaviour {

    public GameObject prefab;
    private GameObject lastInstance = null;
	
    void Start() {
        Respawn();
    }

    public void Respawn() {
        if (lastInstance != null) {
            Destroy(lastInstance);
            lastInstance = null;
        }

        lastInstance = Instantiate(prefab);
        lastInstance.transform.position = transform.position;
        lastInstance.transform.rotation = transform.rotation;
    }
}
