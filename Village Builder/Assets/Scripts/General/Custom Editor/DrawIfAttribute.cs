﻿using System;
using UnityEngine;

/// <summary>
///     Draws the field/property ONLY if the compared property compared by the comparison type with the value of
///     comparedValue returns true.
///     Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfAttribute : PropertyAttribute
{
    /// <summary>
    ///     Only draws the field only if a condition is met. Supports enum and bools.
    /// </summary>
    /// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
    /// <param name="comparedValue">The value the property is being compared to.</param>
    /// <param name="disablingType">
    ///     The type of disabling that should happen if the condition is NOT met. Defaulted to
    ///     DisablingType.DontDraw.
    /// </param>
    public DrawIfAttribute(string comparedPropertyName, object comparedValue,
        DisablingType disablingType = DisablingType.DontDraw)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.comparedValue = comparedValue;
        this.disablingType = disablingType;
    }

    #region Fields

    public string comparedPropertyName { get; }
    public object comparedValue { get; }
    public DisablingType disablingType { get; }

    /// <summary>
    ///     Types of comperisons.
    /// </summary>
    public enum DisablingType
    {
        ReadOnly = 2,
        DontDraw = 3
    }

    #endregion
}