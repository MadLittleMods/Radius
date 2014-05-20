/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: MainThreadTap.cs, May 2014
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class MainThreadTap : MonoBehaviour {

	// We use this to keep tasks needed to run in the main thread
	private static readonly Queue<Action> tasks = new Queue<Action>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.HandleTasks();
	}

	void HandleTasks() {
		while (tasks.Count > 0)
		{
			Action task = null;
			
			lock (tasks)
			{
				if (tasks.Count > 0)
				{
					task = tasks.Dequeue();
				}
			}
			
			task();
		}
	}
	
	public void QueueOnMainThread(Action task)
	{
		lock (tasks)
		{
			tasks.Enqueue(task);
		}
	}

}
