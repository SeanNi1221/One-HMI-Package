using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Sean21.OneHMI
{
    using static Generics;
    public class SQL
    {
        const string Dot = ".";
        const string Space = " ";
        public static string CreateDatabase(string db_name = null, bool if_not_exists = true) {
            string action = if_not_exists? "CREATE DATABASE IF NOT EXISTS ":"CREATE DATABASE ";
            db_name = SetDatabaseName(db_name);
            return action + db_name;
        }
        // public static string CreateDatabase
        // public static string CreateDatabase(Table table) {
        //     return "CREATE DATABASE IF NOT EXISTS " + table.dbName;
        // }
        public static string CreateTable<T>(string db_name = null, string tb_name = null, string timestamp_field_name = "ts", bool if_not_exists = true) {
            string action = if_not_exists? "CREATE TABLE IF NOT EXISTS ":"CREATE TABLE ";
            db_name = SetDatabaseName(db_name);
            tb_name = SetTableNameWith<T>(tb_name);
            string fieldTypes = FieldTypes<T>(timestamp_field_name);
            string sql = action + Quote(db_name) + Dot + Quote(tb_name) + fieldTypes;
            if(Schema.current.detailedDebugLog) Debug.Log("SQL- CREATE TABLE from type " + typeof(T).Name);
            return sql;
        }
        public static string CreateTable(UnityEngine.Object obj, string db_name = null, string tb_name = null, string timestamp_field_name = "ts", bool if_not_exists = true) {
            string action = if_not_exists? "CREATE TABLE IF NOT EXISTS ":"CREATE TABLE ";
            db_name = SetDatabaseName(db_name);
            tb_name = SetTableNameWith(obj, tb_name);
            string fieldTypes = FieldTypes(obj, timestamp_field_name);
            string sql = action + Quote(db_name) + Dot + Quote(tb_name) + fieldTypes;
            if(Schema.current.detailedDebugLog) Debug.Log("SQL- CREATE TABLE from object " + obj.name);
            return sql;
        }
        public static string CreateTable(Table table) {
            string action = "CREATE TABLE IF NOT EXISTS ";
            string db_name = table.dbName;
            string tb_name = db_name + Dot + table.tableName;
            string fieldTypes = FieldTypes(table);
            return action + tb_name + fieldTypes;
        }
    //INSERT INTO d1001 VALUES (NOW, 10.2, 219, 0.32)
        public static string Insert(UnityEngine.Object obj, string db_name = null, string tb_name = null, string time = "NOW") {
            string action = "INSERT INTO ";
            db_name = SetDatabaseName(db_name);
            tb_name = SetTableNameWith(obj,tb_name);
            return action + Quote(db_name) + Dot + Quote(tb_name) + FieldValues(obj, time);
        }
        //INSERT INTO d1001 VALUES ('NOW', 10.2, 219, 0.32) d1002 VALUES ('NOW', 10.2, 219, 0.32) ...
        public static string Insert(UnityEngine.Object[] objects, string db_name = null, string[] _tb_name = null, string[] _time = null) {
            var (tb_name, time) = multiTableInit(objects, _tb_name, _time);
            string action = "INSERT INTO ";
            List<string> tablesString = new List<string>();
            db_name = SetDatabaseName(db_name);
            for (int i=0; i< objects.Length; i++) {
                tb_name[i] = SetTableNameWith(objects[i], tb_name[i]);
                tablesString.Add( db_name + Dot + tb_name[i] + FieldValues(objects[i], time[i]) );
            }
            return action + string.Join(" ", tablesString);
        }
        public static string Insert(params Table[] tables) {
            if (tables.Length < 1) {Debug.Log("No table to insert, aborted!"); return string.Empty;}
            string action = "INSERT INTO ";
            List<string> tablesString = new List<string>();
            foreach (var table in tables) {
                string db_name = table.dbName;
                string tb_name = db_name + Dot + table.tableName;
                string fieldValues = FieldValues(table);
                tablesString.Add( tb_name + fieldValues);
            }        
            return action + string.Join(" ", tablesString);                        
        }
        //INSERT INTO d1001 (ts, current, phase) VALUES ('2021-07-13 14:06:33.196', 10.27, 0.31)
        public static string InsertSpecific(UnityEngine.Object obj, string db_name = null, string tb_name = null, string timestamp_field_name = "ts", string time = "NOW") {
            string action = "INSERT INTO ";
            db_name = SetDatabaseName(db_name);
            tb_name = SetTableNameWith(obj,tb_name);
            return action + db_name + Dot + tb_name + Space + FieldNames(obj, timestamp_field_name) + FieldValues(obj, time);
        }
        //INSERT INTO d1001 (ts, current, phase) VALUES ('NOW', 10.2, 219, 0.32) d1002 (ts, current, phase) VALUES ('NOW', 10.2, 219, 0.32) ...
        public static string InsertSpecific(UnityEngine.Object[] objects, string db_name = null, string[] _tb_name = null, string timestamp_field_name = "ts", string[] _time = null) {
            var (tb_name, time) = multiTableInit(objects, _tb_name, _time);
            string action = "INSERT INTO ";
            List<string> tablesString = new List<string>();
            db_name = SetDatabaseName(db_name);
            string fieldNames = FieldNames(objects[0], timestamp_field_name);
            for (int i=0; i< objects.Length; i++) {
                tb_name[i] = SetTableNameWith(objects[i], tb_name[i]);
                tablesString.Add( db_name + Dot + tb_name[i] + Space + fieldNames + FieldValues(objects[i], time[i]) );
            }
            return action + string.Join(" ", tablesString);                        
        }
        public static string InsertSpecific(params Table[] tables) {
            if (tables.Length < 1) {Debug.Log("No tables to insert, aborted!"); return string.Empty;}
            string action = "INSERT INTO ";
            List<string> tablesString = new List<string>();
            foreach (var table in tables) {
                string db_name = table.dbName;
                string tb_name = db_name + Dot + table.tableName;
                string fieldNames = FieldNames(table);
                string fieldValues = FieldValues(table);
                tablesString.Add( tb_name + Space + fieldNames + fieldValues);
            }        
            return action + string.Join(" ", tablesString);                        
        }
        static Func< UnityEngine.Object[], string[], string[], (List<string>, List<string>) > multiTableInit = (objects, _tb_name, _time) => {
            List<string> tb_name = _tb_name == null? new List<string>{ SetTableNameWith(objects[0]) } : new List<string>(_tb_name);
            List<string> time = _time == null? new List<string>{ "NOW" } : new List<string>(_time);
            if (tb_name.Count < objects.Length) {
                for (int i=tb_name.Count; i<objects.Length; i++) {
                    tb_name.Add(String.Empty);
                }
            }
            if (time.Count < objects.Length) {
                for (int i=time.Count; i<objects.Length; i++) {
                    time.Add("NOW");
                }
            }
            return (tb_name, time);            
        };
        public static string FieldNames(Table table, bool withBracket = true ) {
            string names = string.Join(", ", table.fields.Keys);
            return withBracket? Bracket(names) : names;
        }

        public static string FieldNames(UnityEngine.Object obj, string timestamp_field_name = "ts", bool withBracket = true) {
            return FieldNames( obj.GetType(), timestamp_field_name, withBracket);
        }
        public static string FieldNames<T>(string timestamp_field_name = "ts", bool withBracket = true) {
            return FieldNames( typeof(T), timestamp_field_name, withBracket);
        }
        //(ts, current, phase)
        public static string FieldNames( System.Type type, string timestamp_field_name = "ts", bool withBracket = true) {
            List<string> fieldNames = new List<string>{ Quote(timestamp_field_name) };
            foreach (var field in type.GetFields()) {
                Field f = Attribute.GetCustomAttribute(field, typeof(Field)) as Field;
                if ( f != null) {
                    fieldNames.Add(field.Name);
                }
            }
            string names = string.Join(", ", fieldNames);
            return withBracket? Bracket(names) : names;
        }
        public static string ColumnNamesWithoutTS(UnityEngine.Object obj,  bool withBracket = false) {
            return ColumnNamesWithoutTS( obj.GetType(), withBracket);
        }
        public static string ColumnNamesWithoutTS<T>(bool withBracket = false) {
            return ColumnNamesWithoutTS( typeof(T), withBracket);
        }
    //(current, phase, location)
        public static string ColumnNamesWithoutTS( System.Type type, bool withBracket = false) {
            List<string> columnNames = new List<string>();
            foreach (var field in type.GetFields()) {
                Field f = Attribute.GetCustomAttribute(field, typeof(Field)) as Field;
                if ( f != null ) {
                    columnNames.Add(field.Name);
                }
            }
            string names = string.Join(", ", columnNames);
            return withBracket? Bracket(names) : names;
        }
        public static string FieldTypes(Table table) {
            List<string> fieldTypes = new List<string>();
            foreach (var pair in table.fields ) {
                string key = pair.Key;
                int typeIndex = table.types[key];
                switch (typeIndex) {
                    default: break;
                    case 0: 
                        Debug.LogWarning("Field Type '" + Connector.varType[typeIndex].Name + "' is not supported, value will be force converted to nchar(100)!");
                        break;
                    case -1:
                        Debug.LogError("Field Type '" + Connector.varType[typeIndex].Name + "' is not supported!");
                        break;
                }
                string typeOfThis = " '" + key + "' " + MySQLConnector.dataType[typeIndex];
                if (isTextData(typeIndex)) {
                    typeOfThis += Bracket( table.lengths[key].ToString() );
                }
                fieldTypes.Add(typeOfThis);
            }
            return " (" + string.Join("," , fieldTypes) + ") ";
        }
        public static string FieldTypes<T>(string timestamp_field_name = "ts") {
            return FieldTypes(typeof(T), timestamp_field_name);
        }
        public static string FieldTypes(UnityEngine.Object obj, string timestamp_field_name = "ts") {
            return FieldTypes(obj.GetType(), timestamp_field_name);
        }
        public static string FieldTypes(Type type, string timestamp_field_name = "ts") {
            List<string> fieldTypes = new List<string>{ "'" + timestamp_field_name + "'" + " TIMESTAMP" };
            foreach (var field in type.GetFields()) {            
                Field f = Attribute.GetCustomAttribute(field, typeof(Field)) as Field;
                if ( f != null) {
                    Type fieldType = field.FieldType;
                    int typeIndex = Connector.varType.IndexOf(fieldType);
                    switch (typeIndex)
                    {
                        default: break;
                        case 0: 
                            Debug.LogWarning("Field Type '" + fieldType.Name + "' is not supported, value will be force converted to nchar(100)!");
                            break;
                        case -1:
                            Debug.LogError("Field Type '" + fieldType.Name + "' is not supported!");
                            break;
                    }
                    string typeOfThis = " '" + field.Name + "' " + MySQLConnector.dataType[typeIndex];
                    if (isTextData(typeIndex)) {
                        typeOfThis += Bracket( f.length.ToString() );
                    }
                    fieldTypes.Add(typeOfThis);
                }
            }
            return " (" + string.Join("," , fieldTypes) + ") ";
        }
        static Func<FieldInfo, int, string> serializeType = (field, length) => {
            int typeIndex = Connector.varType.IndexOf(field.FieldType);
            string typeOfThis = " '" + field.Name + "' " + MySQLConnector.dataType[typeIndex];
            if(isTextData(typeIndex)) return typeOfThis + Bracket( length.ToString() );
            else return typeOfThis;
        };
    //VALUES (NOW, 10.2, 219, 0.32)
        public static string FieldValues(Table table) {
            List<string> fieldValues = new List<string>();
            foreach (var pair in table.fields) {
                string key = pair.Key;
                string value = MySQLConnector.SerializeValue(table.target, pair.Value, table.types[key], table.lengths[key]);
                fieldValues.Add(value);
            }
            return " VALUES" + Bracket( string.Join(", " , fieldValues) );            
        }
        public static string FieldValues(UnityEngine.Object obj, string timestamp = "NOW") {
            List<string> fieldValues = new List<string>{timestamp};
            foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                Field f = Attribute.GetCustomAttribute(field, typeof(Field)) as Field;
                if (f != null) {
                    string value = MySQLConnector.SerializeValue(obj, field, Connector.varType.IndexOf(field.FieldType), f.length);                  
                    fieldValues.Add(value);
                }
            }
            return " VALUES" + Bracket( string.Join(", " , fieldValues) );            
        }
        public static string SetTableNameWith<T>(string tb_name = null) {
            if (string.IsNullOrEmpty(tb_name)) {
                tb_name = typeof(T).Name + "_instance_" + TimeStamp16;
            }
            return tb_name;
        }
        public static string SetTableNameWith(System.Type type, string tb_name =null) {
            if (string.IsNullOrEmpty(tb_name)) {
                tb_name = type.Name;
            }
            return tb_name;            
        }
        public static string SetTableNameWith(UnityEngine.Object obj, string tb_name = null) {
            if (string.IsNullOrEmpty(tb_name)) {
                if (obj is Component) {
                    GameObject go = (obj as Component).gameObject;
                    if (go)
                    {
                        tb_name = String.Concat(Array.FindAll(go.name.ToCharArray(), isValidForName));
                    }
                    else { tb_name = obj.name + "_instance_" + TimeStamp16; }
                }
                else {
                    tb_name = obj.name + "_instance_" + TimeStamp16;
                }
            }
            return tb_name;
        }
        public static string SetDatabaseName(string db_name = null) {
            if (string.IsNullOrEmpty(db_name)) {
                db_name = Connector.DefaultDatabaseName;
            }
            return db_name;
        }
        public static string Quote(string s) {
            if ( s.StartsWith("'") && s.EndsWith("'") ) {
                return s;
            }
            else return "'" + s + "'";
        }
        public static string Quote(int l) {
            return Quote(l.ToString());
        }
        public static string Quote(float f) {
            return Quote(f.ToString());
        }
        public static string Bracket(string s, bool force = false) {
            if (force ) {
                return "(" + s + ")";
            }
            else {
                if ( s.StartsWith("(") && s.EndsWith(")") ) {
                    return s;
                }
                else return "(" + s + ")";
            }
        }
        public static string Bracket(int i) {
            return Bracket(i.ToString(), true);
        }
        public static string Bracket(float f) {
            return Bracket(f.ToString(), true);
        }
        public static bool isTextData(int typeIndex) {
            return (typeIndex == 8 || typeIndex == 10)? true:false;
        }
        public static string TimeStamp16 {
            get {return System.DateTime.Now.ToString("yyMMddHHmmssffff");}
        }
        public static bool isValidForName(char c) {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || (c == '_');
        }  
    }
}
