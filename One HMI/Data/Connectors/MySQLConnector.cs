using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Reflection;
namespace Sean21.OneHMI
{
    using static Generics;
    using static StringManipulator;
    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "MySQL Connector", menuName = "One HMI/MySQL Connector")]
    public partial class MySQLConnector : Connector {
        public string serverName = "localhost";
        public string backendLocation = "/UnityMySQLBackend_PHP/";
        public override string uri => Application.isEditor ?
        editorUri + backendLocation + "DataIO.php" :
        buildUri + backendLocation + "DataIO.php";
        private string uriVerify => Application.isEditor ?
        editorUri + backendLocation + "Connector.php" :
        buildUri + backendLocation + "Connector.php";
        public static readonly List<string> dataType = new List<string> {
            "bool", //0, System.Boolean
            "tinyint", //1, System.Byte
            "smallint", //2, System.Int16
            "int", //3, System.Int32
            "bigint", //4, System.Int64
            "float", //5, System.Single
            "double", //6, System.Double
            "timestamp", //7, System.DateTime
            "varchar", //8, System.String
            "varchar(36)", //9, UnityEngine.Vector2
            "varchar(54)", //10, UnityEngine.Vector3
            "varchar(72)", //11, UnityEngine.Quaternion
            "varchar(164)" //12, UnityEngine.Transform
        };
        public override void ResetCreateDatabaseCommand(Request request) {
            base.ResetCreateDatabaseCommand(request);
            request.createDatabaseCommand = $"CREATE TABLE IF NOT EXISTS {request.databaseName}";
        }
        public override void ResetCreateNodeTableCommand(Request request) {
            base.ResetCreateNodeTableCommand(request);
            string sql = $"CREATE TABLE IF NOT EXISTS {request.databaseName}.{request.nodeTableName} ("
            + "\n" + indent + "id varchar(63),"
            + "\n" + indent + "parent_id varchar(63),"
            + "\n" + indent + "display_name varchar(63),"
            + "\n" + indent + "is_visible bool,"
            + "\n" + indent + "is_virtual bool,"
            + "\n" + indent + "spawn_point varchar(164),"
            + "\n" + indent + "latest_area_id varchar(63),"
            + "\n" + indent + "container_id varchar(63),"
            + "\n" + indent + "is_in_container bool,"
            + "\n" + indent + "socket_position varchar(54),"
            + "\n" + indent + "socket_rotation varchar(72),"
            + "\n" + indent + "model_3d_path varchar(255),"
            + "\n" + indent + "indicator_path varchar(255),"
            + "\n" + indent + "icon_path varchar(255),"
            + "\n" + indent + "has_image bool,"
            + "\n" + indent + "overriding_image_path varchar(255),"
            + "\n" + indent + "has_description bool,"
            + "\n" + indent + "overriding_desc_path varchar(255),"            
            + "\n)";
            request.createNodeTableCommand = sql;
        }
        public override void ResetGetNodeSQL(Request request) {
            base.ResetGetNodeSQL(request);
            string sql = $"SELECT ";
        }
        public void Authorize(WWWForm form) {
            form.AddField("serverName", serverName);
            form.AddField("userName", userName);
            form.AddField("password", password);
        }
        public override IEnumerator Send() {
            yield return Send(messageOut);
        }
        public override IEnumerator Send(string message) {
            // succeeded = false;
            if (string.IsNullOrEmpty(message)) {
                if (Schema.current.detailedDebugLog) Debug.LogWarning("Cannot send empty string as SQL, aborted!");
                yield break;
            }
            messageOut = message;
            WWWForm form = new WWWForm();
            Authorize(form);
            form.AddField("sql", messageOut);
            using (UnityWebRequest web = UnityWebRequest.Post(uri, form)) {
                if (Schema.current.detailedDebugLog) Debug.Log("Connecting: " + web.uri);
                web.timeout = timeout;
                operation = web.SendWebRequest();
                if (enableTimer) {
                    BeginForceUpdateInEditMode();
                    responseTime = 0f;
                    while(!web.isDone) {
                        responseTime += Time.deltaTime;
                        //Incase Enable Timer is turned off during timing process.
                        if (!enableTimer) {
                            EndForceUpdateInEditMode();
                            break;
                        }
                        yield return null;
                    }
                }            
                yield return operation;
                EndForceUpdateInEditMode();
                #if UNITY_2020_1_OR_NEWER
                if (web.result == UnityWebRequest.Result.ConnectionError || web.result == UnityWebRequest.Result.ProtocolError)
                #else 
                if (web.isNetworkError || web.isHttpError)
                #endif
                {
                    Debug.LogError("Failed sending Request: " + message + " with error: " + web.error + "  uri: " + uri);
                    yield break;
                }
                if (Schema.current.detailedDebugLog) Debug.Log("Request succeeded" + (enableTimer? " in " + responseTime + " s" : "") + ":\n" + message);
                messageIn = web.downloadHandler.text;
                
                onSendFinished.Invoke();
                yield break;
            }
        }
        public override IEnumerator VerifyConnection() {
            WWWForm form = new WWWForm();
            Authorize(form);
            Debug.Log("Starting request...");
            using (UnityWebRequest request = UnityWebRequest.Post(uriVerify, form)) {
                request.timeout = timeout;
                yield return request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else 
                if (request.web.isNetworkError || request.web.isHttpError)
#endif
                {
                    Debug.LogError("Connection to server failed" + " with error: " + request.error + ".  uri: " + uriVerify);
                    yield break;
                }
                messageIn = request.downloadHandler.text;
                if (messageIn != "1") {
                    Debug.LogError("Connection to MySQL failed with message:" + messageIn);
                    yield break;
                }
                Debug.Log("Connection to MySQL succeeded.");
            }
        }
        public static string SerializeValue(UnityEngine.Object obj, FieldInfo field, int typeIndex, int? textLength) {
            var fieldValue =  field.GetValue(obj);
            if (fieldValue == null) return "NULL";
            // int typeIndex = TDBridge.varType.IndexOf(field.FieldType);
            switch (typeIndex)
            {
                default: return fieldValue.ToString();
                case 4: return ((Int64)fieldValue).ToString("R");
                case 5: return ((Single)fieldValue).ToString("G9");
                case 6: return ((Double)fieldValue).ToString("G17");
                case 7: return SQL.Quote( ((DateTime)fieldValue).ToString("yyyy-MM-dd HH:mm:ss.fff") );
                case 8:
                    var stringValue = (string)fieldValue;
                    if (textLength != null)
                        if (stringValue.Length>textLength) Debug.LogWarning("Value overlength: " + stringValue + ". operation can fail!"); 
                    return SQL.Quote(stringValue);
                case 9: return ((Vector2)fieldValue).ToString("G9");
                case 10: return ((Vector3)fieldValue).ToString("G9");
                case 11: return ((Quaternion)fieldValue).ToString("G9");
                case 12:
                    var transformValue = (Transform)fieldValue;
                    if (!transformValue) return string.Empty;
                        return SQL.Quote(transformValue.localPosition.ToString("G9") + "," + transformValue.localEulerAngles.ToString("G9") + "," + transformValue.localScale.ToString("G9"));
            }
        }

    }
}
