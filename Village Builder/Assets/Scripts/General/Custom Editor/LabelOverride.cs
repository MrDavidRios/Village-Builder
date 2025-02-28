﻿using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class LabelOverride : PropertyAttribute
{
    public string label;

    public LabelOverride(string label)
    {
        this.label = label;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LabelOverride))]
    public class ThisPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                var propertyAttribute = attribute as LabelOverride;
                if (IsItBloodyArrayTho(property) == false)
                    label.text = propertyAttribute.label;
                else
                    Debug.LogWarningFormat(
                        "{0}(\"{1}\") doesn't support arrays ",
                        typeof(LabelOverride).Name,
                        propertyAttribute.label
                    );
                EditorGUI.PropertyField(position, property, label);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private bool IsItBloodyArrayTho(SerializedProperty property)
        {
            var path = property.propertyPath;
            var idot = path.IndexOf('.');
            if (idot == -1) return false;
            var propName = path.Substring(0, idot);
            var p = property.serializedObject.FindProperty(propName);
            return p.isArray;
            //CREDITS: https://answers.unity.com/questions/603882/serializedproperty-isnt-being-detected-as-an-array.html
        }
    }
#endif
}