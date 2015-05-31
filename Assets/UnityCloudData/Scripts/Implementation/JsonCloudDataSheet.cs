using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace UnityCloudData
{
    public abstract class JsonCloudDataSheet : BaseCloudDataSheet
    {
        // turn off serialization for this property since the dictionary wont be serialized and will 
        // be cleared after every script compile (so we will need to reload the values from the cache)
        [NonSerialized]
        bool m_LoadedFromCache;

        // the parsed JSON as a dictionary
        Dictionary<string, object> m_CloudDataDictionary = new Dictionary<string, object>();

        // path to the cache file on the users file system
        string m_UserCacheFilePath;

        // URL of remote data
        public abstract string sheetReadUrl  { get; }
        public abstract string sheetWriteUrl { get; }

        // Last time the data was refreshed
        public DateTime lastRefreshTime { get; private set; }

        // true if the upload of this sheet is in progress
        public bool isCreating { get; protected set; }

        protected override Dictionary<string,object> cloudDataDictionary
        {
            get
            {
                if (!m_LoadedFromCache)
                {
                    LoadFromCache();
                }
                return m_CloudDataDictionary;
            }
            set 
            {
                m_CloudDataDictionary = value;
            }
        }
        
        // A TextAsset that is baked into the build - the local cache of last resort
        protected virtual TextAsset cachedTextAsset
        {
            get
            {
                if(String.IsNullOrEmpty(path))
                {
                    return null;
                }

                return Resources.Load<TextAsset>(path.Replace("/", "-"));
            }
        }
        
        // Identifier used to build the cache file name.
        public override string cacheId
        { 
            get { return path; }
        }
        
        // Path to store and retrieve cached data
        protected virtual string cacheDataPath
        {
            get { return Application.persistentDataPath; }
        }

        // Encode filename as the MD5 hash of cacheId. This will by default cause all sheets that use
        // the same path to use the same cache file.
        protected string userCacheFilePath
        {
            get
            {
                if(String.IsNullOrEmpty(m_UserCacheFilePath) && !String.IsNullOrEmpty(cacheId))
                {
                    MD5CryptoServiceProvider cryptoProvider = new MD5CryptoServiceProvider();
                    byte[] bytes = Encoding.UTF8.GetBytes(cacheId);
                    byte[] hashBytes = cryptoProvider.ComputeHash(bytes);
                    StringBuilder sb = new StringBuilder(cacheDataPath + "/");
                    foreach (byte b in hashBytes)
                    {
                       sb.Append(b.ToString("x2").ToLower());
                    }
                    
                    sb.Append(".webcache");
                    m_UserCacheFilePath = sb.ToString();
                }
                
                return m_UserCacheFilePath;
            }
        }
        
        protected string typeName
        {
            get
            {
                return String.IsNullOrEmpty(this.path) ? this.GetType().Name : this.path;
            }
        }

        // Return a cached copy of the sheet from local storage.  If there is a cache file in the
        // user's persistent data path, use that. Otherwise, use the baked in TextAsset that is
        // part of the build (as returned by the cachedTextAsset property)
        protected string localSheetCache
        {
            get
            {
#if UNITY_WEBPLAYER
#else
                // First try to load from local storage
                string filePath = userCacheFilePath;
                if(File.Exists(filePath))
                {
                    string text;
                    StreamReader fileReader = new StreamReader(filePath);
                    text = fileReader.ReadLine();
                    fileReader.Close();
                    if(Application.isPlaying)
                    {
                        Debug.Log("[Unity Cloud Data] Loaded sheet '" + typeName + "' from cache at: '" + filePath + "'");
                    }
                    return text;
                }
#endif
                // Next return the baked-in asset
                TextAsset cache = cachedTextAsset;
                if(cache != null)
                {
                    if(Application.isPlaying)
                    {
                        Debug.Log("[Unity Cloud Data] Loaded sheet '" + typeName + "' from game asset.");
                    }
                    return cache.text;
                }
                
                // No dice
                if(Application.isPlaying)
                {
                    Debug.LogWarning("[Unity Cloud Data] No locally cached data for '" + typeName + "' found!");
                }
                return null;
            }
            
            set
            {
#if UNITY_WEBPLAYER
#else
                string filePath = userCacheFilePath;
                if(String.IsNullOrEmpty(filePath))
                {
                    Debug.LogError("[Unity Cloud Data] Unable to write sheet data to cache - no file path set!");
                    return;
                }
                
                if(File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                StreamWriter fileWriter = File.CreateText(filePath);
                if (fileWriter != null)
                { 
                    fileWriter.WriteLine(value);
                    fileWriter.Close();
                    Debug.Log("[Unity Cloud Data] Wrote sheet '" + typeName + "' to cache at '" + filePath + "'");
                }
                else
                {
                    Debug.LogError("[Unity Cloud Data] Unable to write sheet data to cache at'" + filePath + "'");
                }
#endif
            }
        }
        
        protected virtual string newSheetEntries
        {
            get
            {
                return "[]";
            }
        }

        void OnDestroy()
        {
            onRefreshCacheComplete = null;
            onLoadFromCacheComplete = null;
        }
        
        // Loads cloud data sheet from local cache
        protected virtual void LoadFromCache()
        {
            m_LoadedFromCache = true;

            string localCache = localSheetCache;
            if(localCache != null)
            {
                ParseSheetData(localCache);
                isLoaded = true;
                hasBeenCreatedInCloud = true;
                
                if(onLoadFromCacheComplete != null)
                {
                    onLoadFromCacheComplete(this);
                }
            }
        }
        
        // Updates the sheet data with data stored on a remote HTTP server
        protected virtual void LoadFromURL()
        {
            if(string.IsNullOrEmpty(path))
            {
                Debug.LogWarning ("[Unity Cloud Data] Not loading sheet - no path set!");
                return;
            }
            
            Debug.Log("[Unity Cloud Data] Requesting sheet from Unity Cloud Data at path: '"+path+"'");
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("[Unity Cloud Data] Internet connection not detected; skipping update for '"+path+"'");
                return;
            }

            isRefreshing = true;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
            WWWUtility.StartRequest(sheetReadUrl, FinishedLoadingFromURL);
        }

        protected virtual void FinishedLoadingFromURL(WWWUtility utility)
        {
			var www = utility.www;

            isRefreshing = false;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
            
            // handle loading error
            if(www.error != null)
            {
                if (www.error.Contains("404"))
                {
                    hasBeenCreatedInCloud = false;
                    Debug.LogWarning(string.Format("[Unity Cloud Data] CloudDataSheet '{0}' not found. Need to create it?", path));
                }
                else
                {
                    Debug.LogError(string.Format("[Unity Cloud Data] Error downloading CloudDataSheet '{0}': {1}", path, www.error));
                }
                return;
            }

            // mark as loaded
            hasBeenCreatedInCloud = true;
            lastRefreshTime = DateTime.Now;

            // validate and parse
            string text = www.text;
            if(ValidateSheetData(text))
            {
                // force the last refresh time into the JSON (so it will persist)
                string refreshString = "\"__lastRefreshTime\": \""+lastRefreshTime.ToString("s")+"\"";
                if(text != "{}") {
                    refreshString += ",";
                }
                text = text.Insert(1, refreshString);

                // parse the sheet and update the local cache
                Debug.Log("[Unity Cloud Data] Successfully updated '" + path + "' from network");
                ParseSheetData(text);
                localSheetCache = text;
                isLoaded = true;
                isRefreshedFromNetwork = true; 
            }
            else
            {
                Debug.LogError("[Unity Cloud Data] Error parsing JSON in '" + path + "'. Reverting to local cache.");
            }
    
            if(onRefreshCacheComplete != null)
            {
                onRefreshCacheComplete(this);
            }

            if(globalRefreshCacheComplete != null)
            {
                globalRefreshCacheComplete(this);
            }

            // now that we've refreshed the data, optionally save all new fields to cloud
#if UNITY_EDITOR
            if(CloudDataManager.instance.autoSaveNewFieldsToCloudOnPlay && Application.isPlaying)
            {
                Save(null);
            }
#endif
        }

        // Creates the sheet data with the current state of all CloudDataFields
        protected virtual void CreateFromURL()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("[Unity Cloud Data] Internet connection not detected; skipping create for '" + this.GetType().Name + "'");
                return;
            }

            // fecth the newEntries
            string rawData = "{"+
                        "\"name\": \""+path+"\","+
                        "\"description\": \"created from editor\","+
                        "\"values\": "+ newSheetEntries +
                        "}";
            byte[] byteArray = Encoding.UTF8.GetBytes(rawData);
            var headers = new Dictionary<string, string>();
            headers["Content-Type"] = "application/json";
            Debug.Log(string.Format("[Unity Cloud Data] Creating sheet in Unity Cloud Data at path: {0}",path));

            isCreating = true;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
            WWWUtility.StartRequest(sheetWriteUrl, byteArray, headers, FinishedCreatingFromURL);
        }

        protected virtual void FinishedCreatingFromURL(WWWUtility utility)
        {
			var www = utility.www;

            isCreating = false;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif

            // handle error
            if(www.error != null)
            {
                if (www.error.Contains("409"))
                {
                    // sheet has already been created, try to load it
                    LoadFromURL();
                }
                else
                {
                    Debug.LogError("[Unity Cloud Data] Error creating CloudDataSheet: " + www.error);

                }
                return;
            }
            
            // mark as created
            Debug.Log (string.Format("[Unity Cloud Data] Successfully created sheet in Unity Cloud Data at path: {0}",path));
            hasBeenCreatedInCloud = true;

            // refresh it!
            RefreshCache();
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
        }

        protected virtual void OnParseSheet(Dictionary<string,object> dict)
        {
            if(ContainsKey("__lastRefreshTime"))
            {
                string time = GetValue("__lastRefreshTime") as string;
                if(time != null)
                {
                    lastRefreshTime = DateTime.Parse(time);
                }
            }
        }

        protected virtual Dictionary<string,object> ParseSheetData(string sheetData)
        {
            cloudDataDictionary = MiniJSON.Json.Deserialize(sheetData) as Dictionary<string,object>;
            OnParseSheet(cloudDataDictionary);
            return cloudDataDictionary;
        }

        protected virtual bool ValidateSheetData(string sheetData)
        {
            var dict = MiniJSON.Json.Deserialize(sheetData) as Dictionary<string,object>;
            if (dict == null )
            {
                Debug.LogWarning("[Unity Cloud Data] JSON Verification failed in '" + this.GetType().Name + "'");
                Debug.LogWarning(sheetData);
                return false;
            }
            return true;
        }

        public override void ReloadFromCache()
        {
            LoadFromCache();
        }

        // Begin refreshing data from the remote server
        public override void RefreshCache()
        {
            if(!isRefreshing)
            {
                LoadFromURL();
            }
        }

        public virtual void RemoveSheet()
        {
            DestroyImmediate(this);
        }
        
        public virtual void CreateSheet(CreateFinishDelegate del)
        {
            if (!hasBeenCreatedInCloud && !isCreating)
            {
                CreateFromURL();
            }
        }
    }

}
