using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityCloudData 
{
    public class WWWUtility : MonoBehaviour
    {
		public delegate void WWWCallback(WWWUtility utility);

        protected WWW         m_Request;
        protected WWWCallback m_Callback;
		protected object	  m_metaData;

		public WWW www
		{
			get { return m_Request; }
		}

		public object metaData
		{
			get { return m_metaData; }
		}

		public static WWWUtility StartRequest(string url, WWWCallback callback)
        {
			return StartRequest(url, null, null, callback);
        }

		public static WWWUtility StartRequest(string url, WWWCallback callback, object metaData)
		{
			return StartRequest(url, null, null, callback, metaData);
		}

		public static WWWUtility StartRequest(string url, byte[] postData, Dictionary<string, string> headers, WWWCallback callback)
		{
			return StartRequest(url, postData, headers, callback, null);
		}

        public static WWWUtility StartRequest(string url, byte[] postData, Dictionary<string, string> headers, WWWCallback callback, object metaData)
        {
			WWW www;
			if(postData != null && headers != null) {
            	www = new WWW(url, postData, headers);
			}
			else {
				www = new WWW(url);
			}

            var util = CreateTempBehavior();
			util.m_metaData = metaData;
            util.StartRequestInternal(www, callback);
			return util;
        }

        protected void StartRequestInternal(WWW request, WWWCallback callback)
        {
            m_Request = request;
            m_Callback = callback;
            
            if(!Application.isPlaying)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.update += WaitForRequestInEditor;
#else
                StartCoroutine(WaitForRequest());
#endif
            }
            else
            {
                StartCoroutine(WaitForRequest());
            }
        }

        protected static WWWUtility CreateTempBehavior()
        {
            var go = new GameObject("wwwutility");
            go.hideFlags = HideFlags.HideAndDontSave;
            return go.AddComponent<WWWUtility>();
        }

#if UNITY_EDITOR
        protected void WaitForRequestInEditor()
        {
            if(m_Request.isDone)
            {
                UnityEditor.EditorApplication.update -= WaitForRequestInEditor;
                InvokeCallback();
                Cleanup();
            }
        }
#endif

        protected IEnumerator WaitForRequest()
        {
            yield return m_Request;
            InvokeCallback();
            Cleanup();
        }

        protected void InvokeCallback()
        {
            if(m_Callback != null)
            {
                m_Callback(this);
            }
            m_Callback = null;
        }

        protected void Cleanup()
        {
            if(!Application.isPlaying)
            {
                GameObject.DestroyImmediate(gameObject);
            }
            else
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}