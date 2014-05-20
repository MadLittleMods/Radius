using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class OptionsManager : MonoBehaviour {

	// TODO: This script is non-functional and needs to be completed...
	// -------------------------------------------------------------------------
	// -------------------------------------------------------------------------


	public class OptionRange
	{
		private float _value;
		public float value
		{
			get {
				return this._value;
			}
			set {
				this._value = Mathf.Clamp(value, this.min, this.max);
			}
		}

		public float min;
		public float max;

	}

	public class OptionBool
	{
		public bool value;
	}




	string PlayerPrefsKeyPrefix = "Option_";
	public Dictionary<string, string> OptionTable = new Dictionary<string, string>();

	public Dictionary<string, Type> dict = new Dictionary<String, Type>() {
		{ "bool", typeof(OptionBool) },
		{ "range", typeof(OptionRange) },
	};


	// Use this for initialization
	void Start () {
		// Collect the values from PlayerPrefs
		foreach(KeyValuePair<string, string> entry in this.OptionTable)
		{
			string adf = PlayerPrefs.GetString(this.PlayerPrefsKeyPrefix + entry.Key);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetOption(string key, string value)
	{
		PlayerPrefs.SetString(this.PlayerPrefsKeyPrefix + key, value);
	}
}
