using UnityEngine;

public class PrefabSpawner : MonoBehaviour {

    public GameObject prefab;
    private GameObject lastInstance = null;
	
    void Start() {
        if (transform.childCount > 0) {
            lastInstance = transform.GetChild(0).gameObject;
        } else {
            Respawn();
        }
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
