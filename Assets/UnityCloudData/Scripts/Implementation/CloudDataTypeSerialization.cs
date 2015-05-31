using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;

namespace UnityCloudData
{
    public static class CloudDataTypeSerialization
    {
        static Dictionary<Type, Func<object, string>> k_SerializationMap = new Dictionary<Type, Func<object, string>>
        {
            {typeof(object),        val => val.SerializeForCloudData()},
            {typeof(int),           val => val.SerializeForCloudData()},
            {typeof(long),          val => val.SerializeForCloudData()},
            {typeof(float),         val => val.SerializeForCloudData()},
            {typeof(double),        val => val.SerializeForCloudData()},
            {typeof(bool),          val => ((bool)val).SerializeForCloudData()},
            {typeof(string),        val => ((string)val).SerializeForCloudData()},
            {typeof(List<string>),  val => ((List<string>)val).SerializeForCloudData()},
            {typeof(Vector2),       val => ((Vector2)val).SerializeForCloudData()},
            {typeof(Vector3),       val => ((Vector3)val).SerializeForCloudData()},
            {typeof(Vector4),       val => ((Vector4)val).SerializeForCloudData()},
            {typeof(Color),         val => ((Color)val).SerializeForCloudData()}
        };

        delegate bool DeserializeDelegate(Dictionary<string, object> dict, out object outObj);
        static Dictionary<string, DeserializeDelegate> k_DeserializationMap = new Dictionary<string, DeserializeDelegate>
        {
            {typeof(Color).FullName,    TryParseColor},
            {typeof(Vector2).FullName,  TryParseVector2},
            {typeof(Vector3).FullName,  TryParseVector3},
            {typeof(Vector4).FullName,  TryParseVector4}
        };
        
        public static string SerializeValue(object val, System.Type valType)
        {
            string formattedValue = "";
            try 
            {
                formattedValue = k_SerializationMap[valType](val);
            }
            catch (Exception ex)
            {
                Debug.Log("[Unity Cloud Data] serialization error for"+valType+": "+ex.Message);
                formattedValue = val.ToString();
            }
            return formattedValue;
        }

        private static string SerializeForCloudData(this object val)
        {
            return val.ToString();
        }

        private static string SerializeForCloudData(this bool val)
        {
            return (bool)val ? "true" : "false";
        }

        private static string SerializeForCloudData(this string val)
        {
            return "\"" + val + "\"";
        }

        private static string SerializeForCloudData(this List<string> val)
        {
            return MiniJSON.Json.Serialize(val);
        }

        private static string SerializeForCloudData(this Vector2 val)
        {
            return "{"+
               "\"x\": "+ val.x.ToString() +","+
               "\"y\": "+ val.y.ToString() +","+
               "\"_type\": \""+typeof(Vector2)+"\""+
            "}";
        }

        private static string SerializeForCloudData(this Vector3 val)
        {
            return "{"+
               "\"x\": "+ val.x.ToString() +","+
               "\"y\": "+ val.y.ToString() +","+
               "\"z\": "+ val.z.ToString() +","+
               "\"_type\": \""+typeof(Vector3)+"\""+
            "}";
        }

        private static string SerializeForCloudData(this Vector4 val)
        {
            return "{"+
               "\"x\": "+ val.x.ToString() +","+
               "\"y\": "+ val.y.ToString() +","+
               "\"z\": "+ val.z.ToString() +","+
               "\"w\": "+ val.w.ToString() +","+
               "\"_type\": \""+typeof(Vector4)+"\""+
            "}";
        }

        private static string SerializeForCloudData(this Color val)
        {
            return "{"+
               "\"a\": "+ val.a.ToString() +","+
               "\"b\": "+ val.b.ToString() +","+
               "\"g\": "+ val.g.ToString() +","+
               "\"r\": "+ val.r.ToString() +","+
               "\"_type\": \""+typeof(Color)+"\""+
            "}";
        }

        public static void DeserializeValue(object owningObject, FieldInfo info, object newval)
        {
            // check for numeric field type (will require some conversion)
            var fieldType = info.FieldType;
            var fieldIsNumericType = (fieldType == typeof(int) || fieldType == typeof(long) || fieldType == typeof(float) || fieldType == typeof(double));
            if(fieldIsNumericType)
            {
                newval = ConvertNumericValue(newval, fieldType);   
            }

            // check assignability
            var newValType = (newval != null) ? newval.GetType() : null;
            if(fieldType.IsAssignableFrom(newValType)) 
            {
                info.SetValue(owningObject, newval);
            }
            else if(newValType == typeof(List<object>))
            {
                // TODO: support lists of other primitives
                List<object> newlist = newval as List<object>;
                List<string> list = newlist.Select(i => i.ToString()).ToList();
                info.SetValue(owningObject, list);
            }
            else if(newValType == typeof(Dictionary<string,object>))
            {
                Dictionary<string,object> dict = newval as Dictionary<string,object>;
                string _type = (string)dict["_type"];
                if(k_DeserializationMap.ContainsKey(_type))
                {
                    var parseCustomValue = k_DeserializationMap[_type];
                    object parsedVal;
                    if (parseCustomValue(dict, out parsedVal))
                    {
                        info.SetValue(owningObject, parsedVal);
                    }
                    else 
                    {
                        Debug.LogWarning(string.Format("[CloudDataTypeSerialization] Unable to assign value type '{0}' to {1}.{2}", _type, owningObject.GetType().FullName, info.Name));                     
                    }
                }
                else
                {
                     Debug.LogError(string.Format("[CloudDataTypeSerialization] No deserialization method defined for type '{0}' - {1}.{2}", _type, owningObject.GetType().FullName, info.Name));                     
                }
            } 
            else 
            {
                string valStr = newval == null ? "<NULL>" : newval.ToString();
                Debug.LogWarning(string.Format("[CloudDataTypeSerialization] Unable to assign value '{0}' of type '{1}' to {2}.{3} of type '{4}'", valStr, newValType, owningObject.GetType().FullName, info.Name, fieldType));
            }
        }
        
        private static object ConvertNumericValue(object newval, Type fieldType)
        {
            try
            {
                var fieldTypeCode = Type.GetTypeCode(fieldType);
                switch(fieldTypeCode)
                {
                    case TypeCode.Int32:
                        return Convert.ToInt32(newval);
                    case TypeCode.Int64:
                        return Convert.ToInt64(newval);
                    case TypeCode.Single:
                        return Convert.ToSingle(newval);
                    case TypeCode.Double:
                        return Convert.ToDouble(newval);
                    default:
                        return newval;
                }
            }
            catch(Exception ex)
            {
                string valStr = newval == null ? "<NULL>" : newval.ToString();
                Debug.LogWarning(string.Format("[CloudDataTypeSerialization] Failed trying to convert '{0}' to numeric value. {1}", valStr, ex.ToString()));
                return newval;
            }
        }

        private static bool TryParseColor(Dictionary<string, object> dict, out object outObj)
        {
            outObj = null;
            if( !dict.ContainsKey("r") ||
                !dict.ContainsKey("g") ||
                !dict.ContainsKey("b") ||
                !dict.ContainsKey("a") )
            {
                return false;
            }

            Color color = new Color();
            color.r = Convert.ToSingle(dict["r"]);
            color.g = Convert.ToSingle(dict["g"]);
            color.b = Convert.ToSingle(dict["b"]);
            color.a = Convert.ToSingle(dict["a"]);
            outObj = color;
            return true;
        }

        private static bool TryParseVector2(Dictionary<string, object> dict, out object outObj)
        {
            outObj = null;
            if( !dict.ContainsKey("x") ||
                !dict.ContainsKey("y") )
            {
                return false;
            }

            Vector2 vec = new Vector2();
            vec.x = Convert.ToSingle(dict["x"]);
            vec.y = Convert.ToSingle(dict["y"]);
            outObj = vec;
            return true;
        }

        private static bool TryParseVector3(Dictionary<string, object> dict, out object outObj)
        {
            outObj = null;
            if( !dict.ContainsKey("x") ||
                !dict.ContainsKey("y") ||
                !dict.ContainsKey("z") )
            {
                return false;
            }

            Vector3 vec = new Vector3();
            vec.x = Convert.ToSingle(dict["x"]);
            vec.y = Convert.ToSingle(dict["y"]);
            vec.z = Convert.ToSingle(dict["z"]);
            outObj = vec;
            return true;
        }

        private static bool TryParseVector4(Dictionary<string, object> dict, out object outObj)
        {
            outObj = null;
            if( !dict.ContainsKey("x") ||
                !dict.ContainsKey("y") ||
                !dict.ContainsKey("z") || 
                !dict.ContainsKey("w") )
            {
                return false;
            }

            Vector4 vec = new Vector4();
            vec.x = Convert.ToSingle(dict["x"]);
            vec.y = Convert.ToSingle(dict["y"]);
            vec.z = Convert.ToSingle(dict["z"]);
            vec.w = Convert.ToSingle(dict["w"]);
            outObj = vec;
            return true;
        }
    }
}