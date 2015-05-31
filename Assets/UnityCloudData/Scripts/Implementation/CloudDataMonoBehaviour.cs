using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace UnityCloudData
{
    [ExecuteInEditMode]
    public class CloudDataMonoBehaviour : MonoBehaviour
    {
        [HideInInspector]
        public bool expandFieldsInInspector;
        [HideInInspector]
        public bool expandNewFieldsInInspector;

        BaseCloudDataSheet m_DefaultSheet;
        CloudDataManager   m_CloudDataManager;
        int                m_NumSheetsToSave;
        
        // An implementation of BaseCloudDataSheet that this object will pull values from
        public virtual BaseCloudDataSheet defaultCloudDataSheet
        {
            get 
            {
                if(m_DefaultSheet == null)
                {
                    if(activeCloudDataManager != null)
                    {
                        m_DefaultSheet = activeCloudDataManager.defaultCloudDataSheet;
                    }
                }
                return m_DefaultSheet;
            }
        }

        // A cloud data manager that is currently in the scene
        protected CloudDataManager activeCloudDataManager
        {
            get 
            {
                if(m_CloudDataManager == null)
                {
                    m_CloudDataManager = FindObjectOfType(typeof(CloudDataManager)) as CloudDataManager;
                }
                return m_CloudDataManager;
            }
        }

        public virtual bool isSavingSheets
        {
            get { return m_NumSheetsToSave > 0; }
        }
        
        protected virtual void Awake()
        {
            if(defaultCloudDataSheet != null)
            {
                defaultCloudDataSheet.onLoadFromCacheComplete += OnCloudDataSheetRefreshCacheComplete;
                defaultCloudDataSheet.onRefreshCacheComplete += OnCloudDataSheetRefreshCacheComplete;
                
                if(defaultCloudDataSheet.isLoaded || !defaultCloudDataSheet.hasBeenCreatedInCloud)
                {
                    UpdateCloudDataFields();
                }
            }
            else
            {
                UpdateCloudDataFields();
            }
        }

        void OnDestroy()
        {
            if(defaultCloudDataSheet != null)
            {
                defaultCloudDataSheet.onLoadFromCacheComplete -= OnCloudDataSheetRefreshCacheComplete;
                defaultCloudDataSheet.onRefreshCacheComplete -= OnCloudDataSheetRefreshCacheComplete;
            }
            
            m_DefaultSheet = null;
        }
        
        protected virtual void OnCloudDataSheetRefreshCacheComplete(BaseCloudDataSheet sheet)
        {
            UpdateCloudDataFields();
        }

        public List<KeyValuePair<FieldInfo, CloudDataField>> GetAllCloudDataFields()
        {
            var list = new List<KeyValuePair<FieldInfo, CloudDataField>>();

            // Go through all of this behaviors fields
            FieldInfo[] fieldInfos = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo info in fieldInfos)
            {
                // Look for fields with the CloudDataField attribute
                object[] attrs = info.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    CloudDataField cloudDataAttr = attr as CloudDataField;
                    if(cloudDataAttr == null)
                        continue;

                    var pair = new KeyValuePair<FieldInfo, CloudDataField>(info, cloudDataAttr);
                    list.Add(pair);
                    break;
                }
            }

            return list;
        }

        // Finds all fields in current object with the CloudDataField attribute and attempts to set the value from cloud data
        public virtual void UpdateCloudDataFields(bool forceInsertNewValues=false)
        {
            var cloudDataFields = GetAllCloudDataFields();
            foreach (var pair in cloudDataFields)
            {
                var info = pair.Key;
                var cloudDataAttr = pair.Value;

                // try to load the custom cloud data sheet specified by the attribute
                BaseCloudDataSheet sheet = defaultCloudDataSheet;
                if (cloudDataAttr.sheetPath != null)
                {
                    sheet = CloudDataManager.GetSheet(cloudDataAttr.sheetPath);
                    if(sheet == null)
                    {
                        Debug.LogWarning(string.Format("[CloudDataField] No CloudDataSheet with path '{0}' found for {1}.{2}", cloudDataAttr.sheetPath, this.GetType().FullName, info.Name));
                    }
                }

                // make sure a sheet was found
                if (sheet == null)
                {
                    Debug.LogWarning(string.Format("[CloudDataField] No cloud data sheet found for field {0}.{1}", this.GetType().FullName, info.Name));
                    continue;
                }
                
                // get the key for this field and check if the sheet contains it
                string cloudDataKey = this.GetType().FullName + "." + info.Name;
                if (sheet.ContainsKey(cloudDataKey))
                {
                    object newFieldValue = sheet.GetValue(cloudDataKey);
                    CloudDataTypeSerialization.DeserializeValue(this, info, newFieldValue);
                }
#if UNITY_EDITOR
                else if (forceInsertNewValues || (CloudDataManager.instance.autoSaveNewFieldsToCloudOnPlay && Application.isPlaying))
                {
                    object newVal = info.GetValue(this);
                    sheet.InsertValue(cloudDataKey, newVal, info.FieldType);
                }
#endif
            }
        }

        public void SaveAllAssociatedSheets()
        {
            var sheetPathsToSave = new List<string>();
            sheetPathsToSave.Add(defaultCloudDataSheet.path);

            // find all of the associated sheets
            var cloudDataFields = GetAllCloudDataFields();
            foreach (var pair in cloudDataFields)
            {
                var cloudDataAttr = pair.Value;
                if (cloudDataAttr.sheetPath != null)
                {
                    sheetPathsToSave.Add(cloudDataAttr.sheetPath);
                }
            }

            // save each sheet
            sheetPathsToSave = sheetPathsToSave.Distinct().ToList();
            m_NumSheetsToSave = sheetPathsToSave.Count;
            foreach (var path in sheetPathsToSave)
            {
                var sheet = CloudDataManager.GetSheet(path);
                if(sheet != null)
                {
                    sheet.Save(SaveSheetFinished);
                }
            }
        }

        protected void SaveSheetFinished(BaseCloudDataSheet sheet, bool success)
        {
            m_NumSheetsToSave--;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
        }
    }
}
