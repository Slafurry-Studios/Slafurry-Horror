using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// ============ CUSTOM PROPERTY DRAWER ============
[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);

            List<string> tagList = new List<string>();
            tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);

            string propertyString = property.stringValue;
            int index = -1;

            for (int i = 0; i < tagList.Count; i++)
            {
                if (tagList[i] == propertyString)
                {
                    index = i;
                    break;
                }
            }

            index = EditorGUI.Popup(position, label.text, index, tagList.ToArray());

            if (index >= 0)
            {
                property.stringValue = tagList[index];
            }
            else
            {
                property.stringValue = "";
            }

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}