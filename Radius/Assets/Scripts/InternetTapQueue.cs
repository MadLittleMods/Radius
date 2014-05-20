/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: InternetTapQueue.cs, May 2014
 */

using UnityEngine;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;

public class InternetTapQueue : MonoBehaviour {

	public MainThreadTap mainThreadTap;
	public float timeout = 2f;

	// We use this to keep tasks needed to run in the main thread
	// Tasks todo if we have internet
	private static readonly Queue<Action> tasksIfOnline = new Queue<Action>();
	// Tasks todo if we don't have internet
	private static readonly Queue<Action> tasksIfOffline = new Queue<Action>();

	WebRequest request;

	bool checking = false;
	float currentTimoutTime = 0f;

	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {
		if(this.checking)
		{
			this.currentTimoutTime += Time.deltaTime;

			if(this.currentTimoutTime >= this.timeout)
			{
				// Stop the request
				this.request.Abort();

				// No internet, so clear the tasks
				Debug.Log("Request took too long, so clearing tasks");
				this.RespondToInternetCheck(false);

				this.checking = false;
			}
		}
	}

	public void QueueIfOnline(Action task)
	{
		lock (tasksIfOnline)
		{
			tasksIfOnline.Enqueue(task);
		}

		if(!this.checking)
		{
			// Quickly check this setting which is not accurate
			if(Application.internetReachability != NetworkReachability.NotReachable)
			{
				// Go on and make a real request to see if we can access the internet
				this.request = WebRequest.Create("http://www.google.com/");
				this.request.Method = "GET";
				this.request.Proxy = null;
				this.request.BeginGetResponse(new AsyncCallback(AsyncWebRequest), null);

				this.checking = true;
			}
			else
			{
				Debug.Log("No internet, so clearing tasks");
				this.RespondToInternetCheck(false);

				this.checking = false;
			}
		}

	}

	public void QueueIfOnline(Action task, Action offlineCallbackTask)
	{
		// You can have a callback if there is no internet after all

		lock (tasksIfOffline)
		{
			tasksIfOffline.Enqueue(offlineCallbackTask);
		}

		this.QueueIfOnline(task);
	}
	
	void AsyncWebRequest(IAsyncResult result)
	{
		HttpWebResponse webResponse = (HttpWebResponse)this.request.EndGetResponse(result);
		
		if (webResponse == null || webResponse.StatusCode != HttpStatusCode.OK)
		{
			// No internet, so clear the tasks
			Debug.Log("No internet, so clearing tasks");
			this.RespondToInternetCheck(false);
		}
		else
		{
			// There is internet so go run the tasks
			this.RespondToInternetCheck(true);
		}
		
		this.checking = false;
		
		webResponse.Close();
	}



	void RespondToInternetCheck(bool isOnline)
	{
		// Run the online tasks
		if(isOnline)
		{
			tasksIfOffline.Clear();
			this.RunInternetTasks();
		}
		// Otherwise run the offline callback tasks
		else
		{
			tasksIfOnline.Clear();
			this.RunOfflineTasks();
		}
	}

	void RunInternetTasks()
	{
		this.mainThreadTap.QueueOnMainThread(() => {
			// Run through the online tasks
			while (tasksIfOnline.Count > 0)
			{
				Action task = null;
				
				lock (tasksIfOnline)
				{
					if (tasksIfOnline.Count > 0)
					{
						task = tasksIfOnline.Dequeue();
					}
				}
				
				task();
			}
		});
	}

	void RunOfflineTasks()
	{

		this.mainThreadTap.QueueOnMainThread(() => {
			// Run through the offline tasks
			while (tasksIfOffline.Count > 0)
			{
				Action task = null;
				
				lock (tasksIfOffline)
				{
					if (tasksIfOffline.Count > 0)
					{
						task = tasksIfOffline.Dequeue();
					}
				}
				
				task();
			}
		});
	}
}
