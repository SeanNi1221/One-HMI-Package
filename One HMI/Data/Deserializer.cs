using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
namespace Sean21.OneHMI {
    public static class Serializer {
        public static string Serialize(this object obj) {
            if (obj == null) return string.Empty;
            if (obj is string) {
                string _obj = (string)obj;
                return string.IsNullOrEmpty(_obj) ? "NULL" : _obj;
            }
            if (obj is bool) return (bool)obj ? "1" : "0";
            if (obj is Transform) {
                if (obj == null) return string.Empty;
                return $"'{(obj as Transform).localPosition.ToString("G9")},{(obj as Transform).localEulerAngles.ToString("G9")},{(obj as Transform).localScale.ToString("G9")}'";
            }
            return obj.ToString();
        }
        public static object Deserialize(string data, FieldInfo field, object obj) {
            if (string.IsNullOrEmpty(data) || string.Equals(data, "null", StringComparison.OrdinalIgnoreCase))
                return default;
            Type type = field.FieldType;
            if (type == typeof(string)) {
                field.SetValue(obj, data);
                return data;
            }
            if (type == typeof(bool)) {
                bool target = deserializeBool(data);
                field.SetValue(obj, target);
                return target;
            }
            if (type == typeof(Transform)) {
                Transform target = deserializeTransform(data, field.GetValue(obj) as Transform);
                return target;
            } else return data;
        }
        public static T Deserialize<T>(string data, T source = null)
        where T : UnityEngine.Object {
            if (string.IsNullOrEmpty(data) || string.Equals(data, "null", StringComparison.OrdinalIgnoreCase))
                return null;
            Type type = typeof(T);
            if (type == typeof(bool)) return deserializeBool(data) as T;
            if (type == typeof(Transform)) return deserializeTransform(data, source as Transform) as T;
            else return data as T;
        }
        public static Func<string, bool> deserializeBool = data => data == "1";
        public static Func<string, Int16> deserializeInt16 = data => Int16.Parse(data);
        public static Func<string, Int32> deserializeInt32 = data => Int32.Parse(data);
        public static Func<string, Int64> deserializeInt64 = data => Int64.Parse(data);
        public static Func<string, Single> deserializeSingle = data => Single.Parse(data);
        public static Func<string, Double> deserializeDouble = data => Double.Parse(data);
        public static Func<string, DateTime> deserializeDateTime = data => DateTime.Parse(data);
        public static Func<string, Vector2> deserializeVector2 = data => string.IsNullOrEmpty(data) || data == "null" ? Vector2.zero : ParseVector2(data);
        public static Func<string, Vector3> deserializeVector3 = data => string.IsNullOrEmpty(data) || data == "null" ? Vector3.zero : ParseVector3(data);
        public static Func<string, Quaternion> deserializeQuaternion = data => string.IsNullOrEmpty(data) || data == "null" ? Quaternion.identity : ParseQuaternion(data);
        public static Func<string, Transform, Transform> deserializeTransform = (data, tr) => string.IsNullOrEmpty(data) || string.Equals(data, "null", StringComparison.OrdinalIgnoreCase) ? tr : ParseTransform(data, tr);
        static Func<string, float[]> vectorElement = (vectorString) => {
            if (string.IsNullOrEmpty(vectorString) || vectorString == "null")
                return null;
            vectorString = vectorString.Trim(new char[] { '(', ')', ',' });
            return Array.ConvertAll(vectorString.Split(','), float.Parse);
        };
        //string: (-3.402823E+38, -3.402823E+38, -3.402823E+38),(-3.402823E+38, -3.402823E+38, -3.402823E+38),(-3.402823E+38, -3.402823E+38, -3.402823E+38)
        public static Transform ParseTransform(string s, Transform tr) {
            string[] property = s.Split(')');
            tr.localPosition = ParseVector3(property[0]);
            tr.localEulerAngles = ParseVector3(property[1]);
            tr.localScale = ParseVector3(property[2]);
            return tr;
        }
        //string: (-3.402823E+38, -3.402823E+38, -3.402823E+38, -3.402823E+38)
        public static Quaternion ParseQuaternion(string s) {
            float[] element = vectorElement(s);
            return new Quaternion(element[0], element[1], element[2], element[3]);
        }
        //string: (-3.402823E+38, -3.402823E+38, -3.402823E+38)
        public static Vector3 ParseVector3(string s) {
            float[] element = vectorElement(s);
            return new Vector3(element[0], element[1], element[2]);
        }
        //string: (-3.402823E+38, -3.402823E+38)
        public static Vector2 ParseVector2(string s) {
            float[] element = vectorElement(s);
            return new Vector2(element[0], element[1]);
        }
        public static string Bracket(int i) {
            return Bracket(i.ToString(), true);
        }
        public static string Bracket(float f) {
            return Bracket(f.ToString(), true);
        }
        public static string Bracket(string s, bool force = false) {
            if (force) {
                return "(" + s + ")";
            } else {
                if (s.StartsWith("(") && s.EndsWith(")")) {
                    return s;
                } else return "(" + s + ")";
            }
        }
        public static string Quote(string s) {
            if (s.StartsWith("'") && s.EndsWith("'")) {
                return s;
            } else return "'" + s + "'";
        }
        public static string Quote(int l) {
            return Quote(l.ToString());
        }
        public static string Quote(float f) {
            return Quote(f.ToString());
        }
        public static string Listify(this string json) {
            return "{\"list\":" + json + "}";
        }
        public static bool NoResults(this string json) {
            return json.StartsWith("0") ? true : false;
        }
    }
}