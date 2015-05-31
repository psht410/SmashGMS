using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using UnityCloudData;

[CustomEditor(typeof(CloudDataManager))]
[CanEditMultipleObjects]
class CloudDataManagerEditor : Editor 
{
    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        string orgId = CloudDataManager.instance.organizationId;
        string projId = CloudDataManager.instance.projectId;
        if(!String.IsNullOrEmpty(orgId) && !String.IsNullOrEmpty(projId))
        {
            GUI.contentColor = String.IsNullOrEmpty(CloudDataManager.accessToken) ? Color.red : Color.white;
            CloudDataManager.accessToken = EditorGUILayout.TextField("Access Token:", CloudDataManager.accessToken);
            GUI.contentColor = Color.white;
            EditorGUILayout.Space();
        }
        
        EditorGUI.BeginDisabledGroup(String.IsNullOrEmpty(CloudDataManager.accessToken));
        if(GUILayout.Button("Refresh All"))
        {
            CloudDataManager.RefreshAll(RefreshCallback);
        }
        if(GUILayout.Button("Create New Sheet"))
        {
            CloudDataManager.CreateNewSheet();
        }
        EditorGUI.EndDisabledGroup();
    }

    void RefreshCallback()
    {
        Debug.Log("[Unity Cloud Data] Sheets refreshed!");
    }
}