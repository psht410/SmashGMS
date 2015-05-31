using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityCloudData
{
    public abstract class BaseCloudDataSheet : MonoBehaviour
    {
        public delegate void CreateFinishDelegate(BaseCloudDataSheet sheet, bool success);
        public delegate void WriteFinishDelegate(BaseCloudDataSheet sheet, bool success);
        public delegate void RefreshCacheCompleteDelegate(BaseCloudDataSheet sheet);
        public delegate void LoadFromCacheCompleteDelegate(BaseCloudDataSheet sheet);
        
        // The path to this sheet in cloud data
        public string path;

        // Backing datastore for the keys/values
        protected virtual Dictionary<string,object> cloudDataDictionary { get; set; }

        // Whether this sheet has been created in the cloud
        public virtual bool hasBeenCreatedInCloud { get; protected set; }

        // A unique string identifier for the cache used to back this sheet
        public virtual string cacheId { get; protected set; }

        // Returns true if the data for this cloud data sheet was loaded from any source
        public virtual bool isLoaded { get; protected set; }
        
        // Returns true if the sheet is in the process of refreshing after a call to RefreshCache()
        public virtual bool isRefreshing { get; protected set; }
        
        // Gets a value indicating whether this instance is refreshed from network.
        public virtual bool isRefreshedFromNetwork { get; protected set; }
        
        // Delegate that is called when cache refresh is finished
        public virtual RefreshCacheCompleteDelegate onRefreshCacheComplete { get; set; }
        public static  RefreshCacheCompleteDelegate globalRefreshCacheComplete { get; set; }
        
        // Delegate that is called when values are loaded from cache
        public virtual LoadFromCacheCompleteDelegate onLoadFromCacheComplete { get; set; }

        // Refresh the data for this sheet from the currently cached data
        public abstract void ReloadFromCache();

        // Refresh the data for this sheet from source and update the cache
        public abstract void RefreshCache();

        // Insert a value into the cache
        public abstract void InsertValue(string key, object val, System.Type fieldType);

        // Save the values to the cloud data store
        public abstract void Save(WriteFinishDelegate del);

        // Retrieve the value for the given key.
        public virtual object GetValue(string key)
        {   
            object objVal = null;
            if(cloudDataDictionary.ContainsKey(key))
            {
                objVal = cloudDataDictionary[key];
            }
            
            return objVal;
        }

        // Determine if this cloud data sheet contains the specified key
        public virtual bool ContainsKey(string key)
        {
            return cloudDataDictionary.ContainsKey(key);
        }

		// Finds the ClouldDataManager of this sheet
        public virtual CloudDataManager myManager
        {
            get
            {
				//This implementation works with prefabs as well, as opposed to GetComponentInParent
				for(Transform t = transform; t != null; t = t.parent)
				{
					CloudDataManager manager = t.GetComponent<CloudDataManager>();
					if(manager != null)
						return manager;
				}

				return null;
            }
        }
    }
}