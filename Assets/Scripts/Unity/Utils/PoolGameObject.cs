using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface PoolGameObjectListener
{
	void OnRetrieve(PoolGameObject go);

	void OnReturn(PoolGameObject go);
}

[DisallowMultipleComponent]
public class PoolGameObject : MonoBehaviour, PoolableObject<PoolGameObject>
{
	private ExtendablePool<PoolGameObject> pool;
	private List<PoolGameObjectListener> listeners = new List<PoolGameObjectListener>();

    public void Return()
    {
		//Recycle the object only if the pool is still alive
		if (pool)
			pool.Return(this);
    }

	public void OnRetrieve (ExtendablePool<PoolGameObject> pool)
	{
		this.pool = pool;

		for(int i = 0; i < listeners.Count; i++)
			listeners[i].OnRetrieve(this);
	}

	public void OnReturn ()
	{
		for(int i = 0; i < listeners.Count; i++)
			listeners[i].OnReturn(this);
	}

	public void AddListener(PoolGameObjectListener l)
	{
		if(!this.listeners.Contains(l))
			this.listeners.Add(l);
	}

	public void RemoveListener(PoolGameObjectListener l)
	{
		this.listeners.Remove(l);
	}
}