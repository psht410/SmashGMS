using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using UnityCloudData;

[CustomEditor(typeof(CloudDataMonoBehaviour), true)]
[CanEditMultipleObjects]
class CloudDataMonoBehaviourEditor : Editor 
{
    List<SerializedProperty> m_CloudDataProperties;
    List<SerializedProperty> m_NewCloudDataProperties;
    string[]                 m_ExcludedProperties;

    public void OnEnable() 
    {
        // force the fields in the bahvior to update
        CloudDataMonoBehaviour behavior = target as CloudDataMonoBehaviour;
        behavior.UpdateCloudDataFields();

        // refresh the property lists
        RefreshPropertyLists();

        // watch for changes to cache
        BaseCloudDataSheet.globalRefreshCacheComplete += OnCloudDataSheetRefreshCacheComplete;
    }

    public void OnDisable() 
    {
        BaseCloudDataSheet.globalRefreshCacheComplete -= OnCloudDataSheetRefreshCacheComplete;
    }

    protected virtual void OnCloudDataSheetRefreshCacheComplete(BaseCloudDataSheet sheet)
    {
        RefreshPropertyLists();
        EditorUtility.SetDirty(target);
    }

    protected void RefreshPropertyLists()
    {
        // reset lists of properties
        m_CloudDataProperties = new List<SerializedProperty>();
        m_NewCloudDataProperties = new List<SerializedProperty>();

        // iterate through all properties on this behavior
        CloudDataMonoBehaviour behavior = target as CloudDataMonoBehaviour;
        var cloudDataFields = behavior.GetAllCloudDataFields();
        foreach (var pair in cloudDataFields)
        {
            var info = pair.Key;
            var cloudDataAttr = pair.Value;

            BaseCloudDataSheet sheet = behavior.defaultCloudDataSheet;
            
            // Try to load the custom cloud data sheet specified by the attribute
            if(cloudDataAttr.sheetPath != null)
            {
                sheet = CloudDataManager.GetSheet(cloudDataAttr.sheetPath);
                if(sheet == null)
                {
                    Debug.LogWarning(string.Format("[Unity Cloud Data] No CloudDataSheet with alias '{0}' found for {1}.{2}", cloudDataAttr.sheetPath, behavior.GetType().FullName, info.Name));
                }
            }

            // If a sheet exists, determine if this key has been added or not
            if(sheet != null)
            { 
                // check if the sheet actually HAS the field
                string cloudDataKey = target.GetType().FullName + "." + info.Name;
                if(sheet.ContainsKey(cloudDataKey))
                {   
                    m_CloudDataProperties.Add(serializedObject.FindProperty(info.Name));
                    continue;
                }
            }
            
            m_NewCloudDataProperties.Add(serializedObject.FindProperty(info.Name));
        }

        // generate the list of properties that AREN'T for cloud data
        m_ExcludedProperties = (from prop in m_CloudDataProperties
                                select prop.name)
                                .Concat
                                (from prop in m_NewCloudDataProperties
                                select prop.name).ToArray();
    }

    public override void OnInspectorGUI() 
    {
        serializedObject.Update();

        // Draw all of the properties other than the cloud properties
        DrawPropertiesExcluding(serializedObject, m_ExcludedProperties);

        // Draw all of the cloud properties
        CloudDataMonoBehaviour behavior = target as CloudDataMonoBehaviour;
        DrawCloudProperties(m_CloudDataProperties, "Cloud Data Fields", true, ref behavior.expandFieldsInInspector);
        DrawCloudProperties(m_NewCloudDataProperties,  "Cloud Data Fields [New]", behavior.isSavingSheets, ref behavior.expandNewFieldsInInspector);

        // Show button for Saving to cloud
        if (m_NewCloudDataProperties.Count > 0 && behavior.defaultCloudDataSheet != null)
        {
            if(behavior.isSavingSheets)
            {
                GUIStyle style = new GUIStyle ();
                style.richText = true;
                style.alignment = TextAnchor.MiddleCenter;

                GUILayout.BeginHorizontal("box");
                GUILayout.Label("<color=yellow>Saving new fields to Unity Cloud Data...</color>",style);
                GUILayout.EndHorizontal();
            }
            else if(GUILayout.Button("Save to Cloud Data Sheet"))
            {
                behavior.UpdateCloudDataFields(true);
                behavior.SaveAllAssociatedSheets();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    protected void DrawCloudProperties(List<SerializedProperty> properties, string label, bool readOnly, ref bool expandFields)
    {
        EditorGUI.indentLevel = 0;
        if (properties.Count > 0)
        {
            expandFields = EditorGUILayout.Foldout(expandFields, label);
            if (expandFields)
            {
                EditorGUI.BeginDisabledGroup(readOnly);
                foreach(SerializedProperty prop in properties)
                {
                    GUIStyle style = new GUIStyle();
                    style.richText = true;
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(prop, true);
                }
                EditorGUI.EndDisabledGroup();
            }
        }
        EditorGUI.indentLevel = 0;
    }
}