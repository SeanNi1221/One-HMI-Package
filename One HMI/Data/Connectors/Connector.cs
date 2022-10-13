using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
namespace Sean21.OneHMI
{
    public abstract partial class Connector : ScriptableObject
    {
        [Header("Server")]
        [Tooltip("The Server IP to be used if running in Unity Editor.")]
        public string editorUri = "127.0.0.1";
        [Tooltip("The Server IP to be used if running in Built App.")]
        public string buildUri = "127.0.0.1";
        public virtual string uri => Application.isEditor ? editorUri : buildUri;
        [Header("Authorization")]
        public string userName;
        public string password;        
        
        [Header("Settings")]
        public int timeout = 15;
        public bool enableTimer;
        public float responseTime;
        [Header("Messages")]
        [TextArea(1,60)]
        public string messageOut;
        public UnityEvent onSendFinished;
        [TextArea(1,60)]
        public string messageIn;
        public UnityWebRequestAsyncOperation operation;
        public const string DefaultDatabaseName = "one_hmi";
        public static readonly List<System.Type> varType = new List<System.Type>{ 
            typeof(System.Boolean), //0
            typeof(System.Byte), //1
            typeof(System.Int16), //2
            typeof(System.Int32), //3
            typeof(System.Int64), //4
            typeof(System.Single), //5
            typeof(System.Double), //6
            typeof(System.DateTime), //7
            typeof(System.String), //8
            typeof(UnityEngine.Vector2), //9
            typeof(UnityEngine.Vector3), //10
            typeof(UnityEngine.Quaternion), //11
            typeof(UnityEngine.Transform) //12
        };
        public virtual void ResetCreateDatabaseCommand(Request request) {
            if (!request.node) return;
        }
        public virtual void ResetCreateNodeTableCommand(Request request) {
            if (!request.node) return;
        }
        public virtual void ResetGetNodeSQL(Request request) { 
            if (!request.node) return;
        }
        public virtual IEnumerator Send() {
            Debug.LogWarning("Not implemented!");
            yield return null;            
        }
        public virtual IEnumerator Send(string message) {
            Debug.LogWarning("Not implemented!");
            yield return null;
        }
        public virtual IEnumerator VerifyConnection() {
            Debug.LogWarning("Not implemented！");
            yield return null;
        }
        public virtual void Clear() {
            messageOut = null;
            messageIn = null;
            responseTime = 0;
        }
    }
}
