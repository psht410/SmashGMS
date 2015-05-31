using System;
using System.Reflection;

namespace UnityCloudData
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CloudDataField : Attribute
    {
    	// allows explicity defining which sheet this value should come from
        public string sheetPath { get; set; }
    }
}
