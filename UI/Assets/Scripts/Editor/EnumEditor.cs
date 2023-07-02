using System.Collections;
using System.Collections.Generic;
using Menthus15Mods.Valheim.BetterTraderClient.Attributes;
using UnityEngine;
using UnityEditor;

// Source: https://discussions.unity.com/t/enum-drop-down-menu-in-inspector-for-nested-arrays/19915/3
[CustomPropertyDrawer(typeof(EnumAttribute))]
public class EnumEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        System.Enum v = EditorGUI.EnumPopup(position, label, (System.Enum)System.Enum.ToObject((this.attribute as EnumAttribute).enumType, prop.intValue));
        prop.intValue = System.Convert.ToInt32(v);
    }
}
