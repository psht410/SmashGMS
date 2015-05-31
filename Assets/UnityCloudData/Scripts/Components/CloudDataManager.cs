using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityCloudData
{
    // Singleton, class that creates and stores CloudDataSheet objects as components
    [AddComponentMenu("Unity Cloud Data/Cloud Data Manager")]
    public class CloudDataManager : MonoBehaviour
    {
        public delegate void RefreshAllCompleteDelegate();

        // organization id as setup in cloud data
        public string organizationId;

        // project id as setup in cloud data
        public string projectId;

        // should sheets be refreshed when Awake is triggered?
        public bool   refreshSheetsOnAwake = true;

        // should new fields be automatically saved to the cloud whenever play mode starts?
        public bool   autoSaveNewFieldsToCloudOnPlay;

        // reference to a cloud data sheet that will be used to populate fields that do not explicitly define a sheetPath in the CloudDataField attribute
        public BaseCloudDataSheet defaultCloudDataSheet;
        
        static CloudDataManager    s_Instance;
        int                        m_RefreshingCount;
        RefreshAllCompleteDelegate m_RefreshAllDel;

        public static CloudDataManager instance
        {
            get
            {
                if(s_Instance == null)
                {
                    // try to find instance in the scene first
                    s_Instance = FindObjectOfType(typeof(CloudDataManager)) as CloudDataManager;
                    if(s_Instance == null)
                    {
                        // create an instance if necessary
                        s_Instance = new GameObject("CloudDataManager").AddComponent<CloudDataManager>();
                    }
                }
                return s_Instance;
             }   
        }

#if UNITY_EDITOR
        public static string accessToken
        {
            get
            {
                if(UnityEditor.EditorPrefs.HasKey(accessTokenPrefsKey))
                {
                    return UnityEditor.EditorPrefs.GetString(accessTokenPrefsKey);
                }
                return "";
            } 
            set
            {
                UnityEditor.EditorPrefs.SetString(accessTokenPrefsKey, value);
            }
        }

        protected static string accessTokenPrefsKey
        {
            get
            {
                string orgId = instance.organizationId;
                string projId = instance.projectId;
                if(String.IsNullOrEmpty(orgId) || String.IsNullOrEmpty(projId)) {
                    return "";
                }
                return orgId + "_" + projId + "_" + "accessToken";
            }
        }
#endif

        protected void Awake()
        {
            s_Instance = this;

            if(defaultCloudDataSheet == null)
            {
                var sheets = GetComponentsInChildren<BaseCloudDataSheet>();
                defaultCloudDataSheet = (sheets != null && sheets.Length > 0) ? sheets[0] : null;
            }

#if UNITY_EDITOR
            if(String.IsNullOrEmpty(organizationId))
                organizationId = UnityEditor.PlayerSettings.companyName;
            if(String.IsNullOrEmpty(projectId))
                projectId = UnityEditor.PlayerSettings.productName;
            if(String.IsNullOrEmpty(accessToken))
                Debug.LogError("[Unity Cloud Data] No AccessToken defined in CloudDataManager (required to use cloud data)!");
#endif
            
            if(refreshSheetsOnAwake)
            {
                RefreshAll(null);
            }
        }

        protected void Start()
        {
            if(!refreshSheetsOnAwake)
            {
                TryAutoSaveAllNewFields();
            }
        }

        void OnDestroy()
        {
            // Clear delegate references for all children that implement ICachingCloudDataSheet
            var sheets = GetComponentsInChildren<BaseCloudDataSheet>();
            foreach(var sheet in sheets)
            {
                sheet.onRefreshCacheComplete = null;
            }
        }

        protected void TryAutoSaveAllNewFields()
        {
#if UNITY_EDITOR
            if(autoSaveNewFieldsToCloudOnPlay && Application.isPlaying)
            {
                var sheets = GetComponentsInChildren<BaseCloudDataSheet>();
                foreach(var sheet in sheets)
                {
                    sheet.Save(null);
                }
            }
#endif
        }

        // Finds and returns an existing component with the given path.  In case of multiples, returns the first match.
        public static BaseCloudDataSheet GetSheet(string sheetPath)
        {
            var sheets = instance.GetComponentsInChildren<BaseCloudDataSheet>();
            foreach(BaseCloudDataSheet sheet in sheets)
            {
                if(sheet.path == sheetPath)
                {
                    return sheet;
                }
            }
            
            return null;
        }

        public static void CreateNewSheet()
        {
            CloudDataSheet sheet = instance.gameObject.AddComponent<CloudDataSheet>();
            if(instance.defaultCloudDataSheet == null)
            {
                instance.defaultCloudDataSheet = sheet;
            }
        }

        public static void ReloadAllFromCache()
        {
            var sheets = instance.GetComponentsInChildren<BaseCloudDataSheet>();
            foreach(var sheet in sheets)
            {
                sheet.ReloadFromCache();
            }
        }
        
        // Signal all sheets in our component list to refresh their data.  Intelligently use the cache ID
        // to only refresh sheets with unique IDs.
        public static void RefreshAll(RefreshAllCompleteDelegate del)
        {
            instance.m_RefreshAllDel = del;
            List<string> refreshedCacheIds = new List<string>();
            
            var sheets = instance.GetComponentsInChildren<BaseCloudDataSheet>();
            foreach(var sheet in sheets)
            {
                if(sheet.isRefreshing)
                    continue;
                
                string cacheId = sheet.cacheId;
                if(refreshedCacheIds.Contains(cacheId))
                {
                    sheet.ReloadFromCache();
                }
                else
                {
                    instance.m_RefreshingCount++;
                    sheet.onRefreshCacheComplete += instance.OnSheetRefreshComplete;
                    sheet.RefreshCache();
                    refreshedCacheIds.Add(cacheId);
                }
            }
        }
        
        void OnSheetRefreshComplete(BaseCloudDataSheet sheet)
        {
            m_RefreshingCount--;
            sheet.onRefreshCacheComplete -= OnSheetRefreshComplete;
            
            if(m_RefreshingCount == 0 && m_RefreshAllDel != null)
            {
                m_RefreshAllDel();
            }
            
            m_RefreshAllDel = null;
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            int x = 5;
            int y = 5;
            int maxWidth = 0;
            int cols = 0;
            int rowPadding = 2;
            int colPadding = 5;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;

            var sheets = instance.GetComponentsInChildren<BaseCloudDataSheet>();
            foreach (var baseSheet in sheets)
            {
                var sheet = baseSheet as CloudDataSheet;
                if(sheet == null || !sheet.debugUnusedKeys || sheet.unusedSheetKeys == null)
                    continue;

                foreach (string key in sheet.unusedSheetKeys)
                {
                    Vector2 labelSize = style.CalcSize(new GUIContent(key));
                    maxWidth = (int) Mathf.Max(maxWidth, labelSize.x);
                    GUI.Label(new Rect(x, y, labelSize.x, labelSize.y), key, style);
                    y+= (int) labelSize.y + rowPadding;
                    
                    if(y + labelSize.y > Screen.height)
                    {
                        cols ++;
                        y = 5;
                        x = (maxWidth + colPadding) * cols;
                    }
                }
            }
        }
#endif
    }
}