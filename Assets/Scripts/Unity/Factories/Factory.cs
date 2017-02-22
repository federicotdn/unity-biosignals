using UnityEngine;
using System.Collections;

public class Factory<T> : MonoBehaviorSingleton<Factory<T>> where T : MonoBehaviour, PoolableObject<T>  {
	public ExtendablePool<T> pool;
	public T prefab;
	public Transform parent;
	public int size;

	protected override void Initialize() {
		pool = new ExtendablePool<T>(size, prefab, parent);
	}
}
