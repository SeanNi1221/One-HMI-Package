using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
namespace Sean21.OneHMI
{
    public class Table : MonoBehaviour
    {
        public UnityEngine.Object target;
        public Node node;
        public string tableName;
        public string dbName;
        public string getSQL;
        public readonly List<FieldInfo> fieldList = new List<FieldInfo>();
        public readonly Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();
        public readonly Dictionary<string, int> types = new Dictionary<string, int>();
        public readonly Dictionary<string, int> lengths = new Dictionary<string, int>();
        protected virtual void OnEnable() {
            if (!node) node = GetComponent<Node>();
        }
        public virtual void GenerateGet () {
            
        }
    }
}
