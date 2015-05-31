#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace UnityCloudData
{
    public struct CloudDataSheetWriteQueueEntry
    {
        public string sheetKey { get; private set; }
        public string putEntry { get; private set; }
        public string putUrl { get; private set; }
            
        public static CloudDataSheetWriteQueueEntry Get(string key, string entry, string url)
        {
            CloudDataSheetWriteQueueEntry writeQueueEntry = new CloudDataSheetWriteQueueEntry();
            writeQueueEntry.sheetKey = key;
            writeQueueEntry.putEntry = entry;
            writeQueueEntry.putUrl = url;
            return writeQueueEntry;
        }
    }

    [ExecuteInEditMode]
    [AddComponentMenu("Unity Cloud Data/Cloud Data Sheet")]
    public class CloudDataSheet : JsonCloudDataSheet
    {
		public enum ApiEnvironments {
			dev,
			staging,
			production
		}

		const string k_ApiHostDev = "data-api-dev.cloud.unity3d.com";
		const string k_ApiHostStaging = "data-api-staging.cloud.unity3d.com";
		const string k_ApiHostProduction = "data-api.cloud.unity3d.com";
        const string k_AccessTokenPostfixTemplate = "?access_token={0}";
        const string k_SheetTokenPostfixTemplate  = "?sheet_token={0}";

        public bool debugUnusedKeys;

        [HideInInspector]
        public string sheetToken;

		[HideInInspector]
		public ApiEnvironments apiEnv = ApiEnvironments.production;

        protected Queue<CloudDataSheetWriteQueueEntry> m_WriteQueue = new Queue<CloudDataSheetWriteQueueEntry>();
        protected WriteFinishDelegate m_WriteFinishCallback;
        protected CloudDataSheetWriteQueueEntry m_CurrentWriteEntry;

        public List<string> unusedSheetKeys { get; protected set; }

		protected virtual string apiHost
		{
			get
			{
				switch(apiEnv)
				{
				case ApiEnvironments.dev:
					return k_ApiHostDev;
				case ApiEnvironments.staging:
					return k_ApiHostStaging;
				default:
					return k_ApiHostProduction;
				}
			}
		}

		protected virtual string defaultUrlTemplate
		{
			get { return "https://" + apiHost + "/api/orgs/{0}/projects/{1}/sheet/{2}"; }
		}

		protected virtual string valueUrlTemplate
		{
			get { return "https://" + apiHost + "/api/orgs/{0}/projects/{1}/value/{2}/{3}"; }
		}

		protected virtual string sheetTokenUrlTemplate
		{
			get { return "https://" + apiHost + "/api/orgs/{0}/projects/{1}/tokens/{2}"; }
		}

        protected virtual string baseUrl
        {
            get
            {
                return string.Format(defaultUrlTemplate, myManager.organizationId, myManager.projectId, path);
            }
        }

        public override string sheetReadUrl
        {
            get
            {
                return baseUrl + GetAccessUrlPostfix(true);
            }
        }

        public override string sheetWriteUrl
        {
            get
            {
                return baseUrl + GetAccessUrlPostfix(false);
            }
        }

        public virtual string tokenReadUrl
        {
            get
            {
                return string.Format(sheetTokenUrlTemplate, myManager.organizationId, myManager.projectId, path) + GetAccessUrlPostfix(false); 
            }
        }
 
        protected override string newSheetEntries
        {
            get
            {
                Debug.Log(string.Format("[Unity Cloud Data] There are {0} entries to save in the queue.", m_WriteQueue.Count));
                string rawData = "[";
                bool first = true;
                while(m_WriteQueue.Count > 0)
                {
                    CloudDataSheetWriteQueueEntry entry = m_WriteQueue.Dequeue();
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        rawData += ",";
                    }
                    rawData += entry.putEntry;
                }
                rawData += "]";
                return rawData;
            }
        }

        protected virtual string getValueUrl(string key)
        {
            return string.Format(valueUrlTemplate, new object[] { myManager.organizationId, myManager.projectId, path, key });
        }

        protected virtual string GetAccessUrlPostfix(bool readOnly)
        {
#if UNITY_EDITOR
            if(readOnly)
            {
                return string.Format(k_SheetTokenPostfixTemplate, sheetToken);
            }
            else
            {
                return string.Format(k_AccessTokenPostfixTemplate, CloudDataManager.accessToken);
            }
#else
            return string.Format(k_SheetTokenPostfixTemplate, sheetToken);
#endif
        }

        protected override Dictionary<string,object> ParseSheetData(string tableData)
        {
            unusedSheetKeys = new List<string>();
            return base.ParseSheetData(tableData);
        }
        
        protected override void OnParseSheet(Dictionary<string,object> dict)
        {
            foreach (var entry in dict)
            {
                unusedSheetKeys.Add(entry.Key);        
            }
            base.OnParseSheet(dict);
        }
        
        public override object GetValue(string key)
        {
            unusedSheetKeys.Remove(key);
            return base.GetValue (key);
        }

        protected override void LoadFromURL()
        {
#if UNITY_EDITOR
            if(String.IsNullOrEmpty(sheetToken))
            {
                LoadSheetToken();
            }
            else
            {
                base.LoadFromURL();
            }
#else
            base.LoadFromURL();
#endif
        }

        protected virtual void LoadSheetToken()
        {
            Debug.Log("[Unity Cloud Data] Requesting sheet token from Unity Cloud Data for sheet: '"+path+"'");
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("[Unity Cloud Data] Internet connection not detected; skipping sheet token request for '"+path+"'");
                return;
            }

            isRefreshing = true;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
            WWWUtility.StartRequest(tokenReadUrl, FinishedLoadingSheetToken);
        }

        protected virtual void FinishedLoadingSheetToken(WWWUtility utility)
        {
            isRefreshing = false;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
			var www = utility.www;
            
            // handle loading error
            if(www.error != null)
            {
                if (www.error.Contains("404"))
                {
                    hasBeenCreatedInCloud = false;
                    Debug.LogWarning(string.Format("[Unity Cloud Data] sheet '{0}' not found in Unity Cloud Data. Need to create it?", path));
                }
                else
                {
                    Debug.LogError(string.Format("[Unity Cloud Data] Error getting token for sheet '{0}': {1}", path, www.error));
                }
                return;
            }

            string text = www.text;
            
            var data = MiniJSON.Json.Deserialize(text) as List<object>;
            if(data != null && data.Count > 0)
            {
                var dict = data[0] as Dictionary<string,object>;
                sheetToken = (dict != null) ? dict["token"] as string : null;
            }

            // load data using new sheet token
            if(!String.IsNullOrEmpty(sheetToken))
            {
                base.LoadFromURL();
            }
            else 
            {
                Debug.LogError(string.Format("[Unity Cloud Data] Error parsing sheet token for sheet'{0}':  raw data: {1}", path, text));
            }
        }

        void WriteQueueEntries()
        {
            if(m_WriteQueue.Count > 0)
            {
                m_CurrentWriteEntry = m_WriteQueue.Dequeue();
              
                // Create request headers
                var headers = new Dictionary<string, string>();
                headers.Add("Content-Type", "application/json");
                Debug.Log(string.Format("[Unity Cloud Data] writing key '{0}' for sheet at path '{1}'", m_CurrentWriteEntry.sheetKey, path));
                byte[] body = Encoding.UTF8.GetBytes(m_CurrentWriteEntry.putEntry);
                WWWUtility.StartRequest(m_CurrentWriteEntry.putUrl, body, headers, FinishedWritingEntry);
            }
            else
            {
                Debug.Log(string.Format("[Unity Cloud Data] Done saving sheet at path '{0}'!", path));
                // wait until after the refresh completes to trigger the callback
                onRefreshCacheComplete += PostWriteRefreshFinished;
                RefreshCache();
            }
        }

        void FinishedWritingEntry(WWWUtility utility)
        {
			var response = utility.www;

            if(response.error != null)
            {
                Debug.LogWarning(string.Format("[Unity Cloud Data] Error writing key '{0}' to sheet at path '{1}': {2}", m_CurrentWriteEntry.sheetKey, path,response.error));
            }
            else
            {
                Debug.Log(string.Format("[Unity Cloud Data] Successfully wrote key '{0}' to sheet at path '{1}'", m_CurrentWriteEntry.sheetKey, path));
            }

            WriteQueueEntries();
        }

        protected void PostWriteRefreshFinished(BaseCloudDataSheet sheet)
        {
            onRefreshCacheComplete -= PostWriteRefreshFinished;
            if (m_WriteFinishCallback != null)
            {
                m_WriteFinishCallback(this, true);
            }
        }

        protected virtual CloudDataSheetWriteQueueEntry GetInsertQueueEntry(string cloudDataKey, object val)
        {
            // Create put entry
            string entry = 
                "    {" +
                "        \"value\": " + val +
                "    }";

            // Format URL
            string putUrl = getValueUrl(cloudDataKey) + GetAccessUrlPostfix(false);
            return CloudDataSheetWriteQueueEntry.Get(cloudDataKey, entry, putUrl);
        }

        public override void InsertValue(string key, object val, System.Type fieldType)
        {
            // Make sure this key hasn't been queued up already and create a write queue entry
            string formattedValue = CloudDataTypeSerialization.SerializeValue(val, fieldType);
            int count = m_WriteQueue.Count(item => (item.sheetKey == key));
            if(count == 0)
            {
                m_WriteQueue.Enqueue(GetInsertQueueEntry(key, formattedValue));
            }
        }
        
        public override void Save(WriteFinishDelegate del) 
        {
            if(m_WriteQueue.Count <= 0)
            {
                if (del != null)
                {
                    del(this, true);
                }
                return;
            }

            Debug.Log(string.Format("[Unity Cloud Data] There are {0} entries to save in the queue.", m_WriteQueue.Count));
#if UNITY_EDITOR
            m_WriteFinishCallback = del;
            WriteQueueEntries();
#endif
        }
    }
}
