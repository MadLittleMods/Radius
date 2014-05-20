/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ExposePropertyAttribute.cs, May 2014
 */

using UnityEngine;
using System;
using System.Collections;

[AttributeUsage( AttributeTargets.Property )]
public class ExposePropertyAttribute : Attribute
{
	// See Editor/ExposeProperties.cs
}