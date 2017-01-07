using UnityEngine;
using System.Collections;

public class CounterTimer
{
	private float time = 0f;
	private float length = 1f;
	private bool finished = false;

	public CounterTimer(float length)
	{
		this.length = length;
		this.time = 0f;
		this.finished = false;
	}

	public void Update (float delta) 
	{
		if(time + delta >= length)
		{
			time = length;
			finished = true;
		}
		else
		{
			time += delta;
			finished = false;
		}
	}

	public bool Finished
	{
		get { return finished; } 
	}

	public float NormalizedTime
	{
		get { return length == 0f ? 1f : Mathf.Clamp01(time / length); }
	}

	public float CurrentTime
	{
		get { return time; }
	}

	public float Length
	{
		get { return length; }
	}

	public void Reset()
	{
		time = 0f;
		finished = false;
	}

	public static implicit operator bool(CounterTimer exists)
	{
		return exists != null;
	}
}