using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using UnityCloudData;

[CustomEditor(typeof(CloudDataSheet), true)]
[CanEditMultipleObjects]
class CloudDataSheetEditor : Editor
{
    protected virtual void OnEnable()
    {
        CloudDataSheet sheet = target as CloudDataSheet;
        sheet.ReloadFromCache();
    }

    public override void OnInspectorGUI() 
    {
        CloudDataSheet sheet = target as CloudDataSheet;
        bool hasInvalidPath = String.IsNullOrEmpty(sheet.path);

        GUIStyle style = new GUIStyle ();
        style.richText = true;
        style.alignment = TextAnchor.MiddleCenter;

        GUILayout.BeginHorizontal("box");
        if(hasInvalidPath)
        {
            GUILayout.Label("<color=red>INVALID PATH: Sheet must have a path set!</color>",style);
        }
        else if(sheet.isCreating)
        {
             GUILayout.Label("<color=yellow>Syncing sheet with Unity Cloud Data...</color>",style);
        }
        else if(sheet.isRefreshing)
        {
            GUILayout.Label("<color=yellow>Refreshing from Unity Cloud Data...</color>",style);
        }
        else if(!sheet.hasBeenCreatedInCloud)
        {
            GUILayout.Label("<color=red>NOT SYNCED: Sheet not synced with Unity Cloud Data!</color>",style);
        }
        else
        {
            GUILayout.Label("<color=green>Last refresh: "+sheet.lastRefreshTime+"</color>",style);
        }
        GUILayout.EndHorizontal();

        base.OnInspectorGUI();

        if(EditorPrefs.GetBool("UnityCloudData.EnableDeveloperOptions", false)) {
            sheet.apiEnv = (CloudDataSheet.ApiEnvironments) EditorGUILayout.EnumPopup("API Environment", sheet.apiEnv);
        }

        if (!sheet.hasBeenCreatedInCloud)
        {
            EditorGUI.BeginDisabledGroup(hasInvalidPath || sheet.isCreating);
            if(GUILayout.Button("Sync Sheet with Unity Cloud Data"))
            {
                Debug.Log("[Unity Cloud Data] Creating sheet at path: "+sheet.path);
                sheet.CreateSheet(OnCreateFinish);
            }
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Sheet Token:", sheet.sheetToken);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(sheet.isRefreshing);
            if(GUILayout.Button("Refresh from Unity Cloud Data"))
            {
                // force the sheet token to update when refreshing
                sheet.sheetToken = null;
                sheet.RefreshCache();
            }
            EditorGUI.EndDisabledGroup();
        }
    }

    void OnCreateFinish(BaseCloudDataSheet sheet, bool success)
    {
        Debug.Log("[Unity Cloud Data] Sync status: "+success);
    }
}