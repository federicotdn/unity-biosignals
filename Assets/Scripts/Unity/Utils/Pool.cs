using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool : MonoBehaviour 
{
	private ExtendablePoolGameObject internalPool = null;

	public void Initialize(GameObject prefab, int initialSize)
    {
		if(!prefab)
			Debug.LogError("Could not find prefab", this);
		
		this.transform.position = Vector3.zero;
		this.transform.rotation = Quaternion.identity;
		this.transform.localScale = Vector3.one;

		this.internalPool = new ExtendablePoolGameObject(initialSize, prefab, this.transform);
		this.internalPool.Initialize();
    }

    public PoolGameObject Retrieve()
    {
		return internalPool.Retrieve();
    }

    public void Return(PoolGameObject go)
    {
		internalPool.Return(go);
    }
}