using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class OptionsManager : MonoBehaviour {

	// TODO: This script is non-functional and needs to be completed...
	// -------------------------------------------------------------------------
	// -------------------------------------------------------------------------

	public interface IOption<T>
	{
		T Value
		{
			get;
			set;
		}

		string AsString
		{
			get;
		}

		void ParseString(string inputString);
	}

	public class OptionRange : IOption<float>
	{
		private float _value;
		public float Value
		{
			get {
				return this._value;
			}
			set {
				this._value = Mathf.Clamp(value, this.Min, this.Max);
			}
		}

		public float Min;
		public float Max;


		public string AsString
		{
			get {
				var rangeValues = new Dictionary<string, string>() {
					{"value", this.Value.ToString()},
					{"min", this.Min.ToString()},
					{"max", this.Max.ToString()},
				};

				return UtilityMethods.GenerateSeparatedString(rangeValues);
			}
		}

		public void ParseString(string inputString)
		{
			var parsedChunks = UtilityMethods.ParseSeparatedStrToDict(inputString);
			
			this.Value = float.Parse(parsedChunks.GetValueOrDefault("value", "0"));
			this.Min = float.Parse(parsedChunks.GetValueOrDefault("min", "0"));
			this.Max = float.Parse(parsedChunks.GetValueOrDefault("max", "1"));
		}
	}

	public class OptionBool : IOption<bool>
	{
		public bool _value;
		public bool Value
		{
			get {
				return this._value;
			}
			set {
				this._value = value;
			}
		}

		public string AsString
		{
			get {
				var rangeValues = new Dictionary<string, string>() {
					{"value", this.Value.ToString()},
				};
				
				return UtilityMethods.GenerateSeparatedString(rangeValues);
			}
		}

		public void ParseString(string inputString)
		{
			var parsedChunks = UtilityMethods.ParseSeparatedStrToDict(inputString);
			
			this.Value = parsedChunks.GetValueOrDefault("value", "false").ToLower() == "true";
		}
	}



	public struct OptionType
	{

	}



	string PlayerPrefsKeyPrefix = "Option_";
	public Dictionary<string, IOption<System.Object>> OptionTable = new Dictionary<string, IOption<System.Object>>();

	public Dictionary<string, Type> dict = new Dictionary<String, Type>() {
		{ "bool", typeof(OptionBool) },
		{ "range", typeof(OptionRange) },
	};


	// Use this for initialization
	void Start () {
		// Collect the values from PlayerPrefs
		foreach(KeyValuePair<string, IOption<System.Object>> entry in this.OptionTable)
		{
			string outValue = PlayerPrefs.GetString(this.PlayerPrefsKeyPrefix + entry.Key);

			this.OptionTable[entry.Key].ParseString(outValue);
			//this.OptionTable[entry.Key] = outValue;
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
