using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace UnityCloudData
{
    public class RefreshLocalAssets : ScriptableWizard
    {
        public struct DownloadItem
        {
            public string Url;
            public string AssetPath;
            public string AssetName;
            
            public override string ToString()
            {
                return string.Format("[DownloadItem {0} : {1} : {2}]", AssetName, AssetPath, Url);
            }
        }

        private static readonly string KEY_ASSET_FOLDER = "CloudDataSheets.RefreshLocalAssets.assetFolder";
        private static readonly string DEFAULT_ASSET_FOLDER = "Resources";
        public string assetFolder;
        private bool m_sync = false;

        public static bool RefreshSync()
        {
            var wizard = ScriptableWizard.DisplayWizard<RefreshLocalAssets>(string.Empty);
            wizard.m_sync = true;
            wizard.LoadSettings();
            return wizard.DoRefresh();
        }
        
        [UnityEditor.MenuItem("Cloud Data/Refresh Local CloudDataSheet Assets")]
        public static void CreateWizard()
        {
            RefreshLocalAssets rla = ScriptableWizard.DisplayWizard<RefreshLocalAssets>("Refresh Local CloudDataSheet Assets", "Refresh");
            rla.LoadSettings();
            
            // Get on with it if we're in batch mode
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                rla.OnWizardCreate();
            }
        }

        void LoadSettings()
        {
            assetFolder = EditorPrefs.GetString(KEY_ASSET_FOLDER, DEFAULT_ASSET_FOLDER);
        }

        void SaveSettings()
        {
            EditorPrefs.SetString(KEY_ASSET_FOLDER, assetFolder);
        }

        void OnWizardUpdate()
        {
            if (assetFolder == string.Empty)
            {
                isValid = false;
                errorString = "Asset Folder cannot be blank.";
            }
            else
            {
                errorString = string.Empty;
                helpString = "Press Refresh to update assets.";
                isValid = true;
            }
        }

        void OnWizardCreate()
        {
            DoRefresh();
        }

        bool DoRefresh()
        {
            SaveSettings();

            // Find all tweak table prefabs
            List<Component> dataSheets = UnityCloudData.EditorTools.GetAllComponentsInPrefabs<CloudDataSheet>();

            string baseAssetPath = Application.dataPath + "/" + assetFolder;
            if (false == Directory.Exists(baseAssetPath))
            {
                Directory.CreateDirectory(baseAssetPath);
            }
            
            // Fancy LINQ syntax to build a list of DownloadItem structs
            IEnumerable<DownloadItem> urls =
                from component in dataSheets
                select new DownloadItem
                {
                    Url = ((CloudDataSheet) component).sheetReadUrl,
                    AssetName = ((CloudDataSheet) component).path,
                    AssetPath = baseAssetPath + "/" + ((CloudDataSheet) component).path.Replace("/", "-") + ".txt"
                };

            bool refreshResult = true;

            // Save each URL to asset
            foreach (DownloadItem item in urls)
            {
                if (m_sync)
                {
                    WWW request = new WWW(item.Url);
                    while (false == request.isDone)
                    {
                        System.Threading.Thread.Sleep(100);
                    }

                    // Make sure we return false if there are any failures
                    refreshResult &= HandleResponseForDownloadItem(request, item);
                }
                else
                {
                    WWWUtility.StartRequest(item.Url, FinishedCreatingFromURL, item);
                }
            }

            return refreshResult;
        }
        
        protected virtual void FinishedCreatingFromURL(WWWUtility utility)
        {
            var request = utility.www;
            var item = (DownloadItem)utility.metaData;
            HandleResponseForDownloadItem(request, item);
        }

        protected virtual bool HandleResponseForDownloadItem(WWW request, DownloadItem item)
        {
            if (request.error != null)
            {
                Debug.LogError("Error encountered downloading '" + item.AssetName + "' from " + item.Url);
                return false;
            }
            
            var writer = new StreamWriter(item.AssetPath);
            writer.Write(request.text);
            writer.Close();
            
            Debug.Log(string.Format("Wrote CloudDataSheet '{0}' to {1}", item.AssetName, item.AssetPath));

            return true;
        }
    }
}
