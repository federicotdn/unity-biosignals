using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Poolable object with two methods called as a stack.
/// Lifecycle: 
/// 
/// <para>Create/Reparent if necessary</para>
/// <para>OnEnable()</para>
/// <para>OnRetrieve()</para>
/// 
/// <para>OnReturn()</para>
/// <para>OnDisable()</para>
/// <para>Reparent </para>
/// </summary>
public interface PoolableObject<T> where T: MonoBehaviour, PoolableObject<T>
{
	/// <summary>
	/// The object is being retrieved, AFTER OnEnable
	/// It also receives the pool in case it wants to unregister itself
	/// </summary>
	void OnRetrieve(ExtendablePool<T> pool);

	/// <summary>
	/// The object is being returned, BEFORE OnDisable
	/// </summary>
	void OnReturn();
}

public interface PoolListener<T> where T: MonoBehaviour, PoolableObject<T>
{
	void OnObjectRetrieved(PoolableObject<T> obj);

	void OnObjectReturned(PoolableObject<T> obj);
}

/// <summary>
/// A generic pool that accepts arbitrary game objects and assings a poolable component for easy handling.
/// Careful: this pool instantiates a prefab, assigns a component, and uses it as a "ingame prefab".
/// </summary>
public class ExtendablePoolGameObject : ExtendablePool<PoolGameObject>
{
	private GameObject prefabGO;

	public ExtendablePoolGameObject(int initialSize, GameObject prefabGO, Transform parent) : base(initialSize, null, parent)
	{
		this.prefabGO = prefabGO;

		if(prefabGO)
			this.namePrefix = prefabGO.name;
	}

	protected override PoolGameObject InstantiatePrefab()
	{
		GameObject modifiedPrefab = GameObject.Instantiate(prefabGO) as GameObject;

		// Maybe the object requires knowing it is poolable and already has a pool game object
		PoolGameObject pgo = modifiedPrefab.GetComponent<PoolGameObject>();

		if(!pgo)
			pgo = modifiedPrefab.AddComponent<PoolGameObject>();
		
		return pgo;
	}
}

/// <summary>
/// A generic pool of scriptable objects that can grow if needed.
/// </summary>
public class ExtendablePool<T> where T: MonoBehaviour, PoolableObject<T>
{
	/// <summary>
	/// Total pool size
	/// </summary>
	public int PoolSize 
	{
		get { return poolSize; }
	}

	/// <summary>
	/// Gets available elements count.
	/// </summary>
	public int AvailableElements
	{
		get { return pool.Count; }
	}

	protected T prefab;
	protected string namePrefix;

	private int poolSize = 0;
	private Transform parent;
	private List<T> pool = new List<T>();

	private List<PoolListener<T>> listeners = new List<PoolListener<T>>();

	public ExtendablePool(int initialSize, T prefab, Transform parent)
	{
		this.poolSize = initialSize;
		this.prefab = prefab;
		this.parent = parent;

		if(prefab)
			this.namePrefix = prefab.name;
	}

	public void AddListener(PoolListener<T> l)
	{
		this.listeners.Add(l);
	}

	public virtual void Initialize()
	{
		pool.Clear();

		for(int i = 0; i < poolSize; i++)
		{
			T obj = InstantiatePrefab();
			obj.name = namePrefix + "-" + pool.Count;
			obj.gameObject.SetActive(false);
			obj.transform.SetParent(parent, true);

			pool.Add(obj);
		}
	}

	protected virtual T InstantiatePrefab()
	{
		return GameObject.Instantiate(prefab) as T;
	}

	public virtual T Retrieve()
	{
		T obj = null;

        int tryCount = 0;

        // While object is not null and not destroyed...
        while(!obj && tryCount++ < 3)
        {
    		if(pool.Count > 0)
    		{
    			obj = pool[pool.Count - 1];
    			pool.RemoveAt(pool.Count - 1);
    		}
    		else
    		{
				obj = InstantiatePrefab();
				obj.transform.SetParent(parent, true);
				obj.name = namePrefix + "-" + poolSize;

    			// The total pool has grown
    			poolSize++;
    		}
        }

        if(obj)
        {
    		obj.gameObject.SetActive(true);
    		obj.OnRetrieve(this);

    		for(int i = 0; i < listeners.Count; i++)
    			listeners[i].OnObjectRetrieved(obj);
        }

		return obj;
	}

	public virtual void Return(T obj)
	{
		if(obj == null || HasAlreadyReturnedObject(obj))
			return;

		obj.OnReturn();
		obj.gameObject.SetActive(false);
		obj.transform.SetParent(parent, true);

		pool.Add(obj);
		
		for(int i = 0; i < listeners.Count; i++)
			listeners[i].OnObjectReturned(obj);
	}

	private bool HasAlreadyReturnedObject(T obj)
	{
		for(int i = 0; i < pool.Count; i++)
		{
			if(pool[i] == obj)
			{
				Debug.LogError("Object " + obj.name + " has already been returned!");
				return true;
			}
		}

		return false;
	}

	public static implicit operator bool(ExtendablePool<T> exists)
	{
		return exists != null;
	}
}