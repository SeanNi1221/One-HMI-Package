# if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sean21.OneHMI
{
    using static EditorGenerics;
    [CustomEditor(typeof(Connector), true)]
    public class ConnectorEditor : OneHMIEditor {
        protected Connector connector;
        protected HMIHub hub { 
            get {
                if (!HMIHub.i) Debug.LogError("Cannot find HMIHub Instance!");
                return HMIHub.i;
            }
        }
        protected virtual void OnEnable() {
            Modify("password").DrawAs(()=>EditorGUILayout.PasswordField("Password", connector.password));
            Modify("responseTime").Hide(() => !connector.enableTimer);
            connector = target as Connector;
            var send = new GUIContent(" Send Message", LoadIcon("terminal.png"), "Send messageOut to uri");
            var clear = new GUIContent(LoadIcon("clear.png"), "Clear Message");
            Modify("messageOut").AddAbove(()=>{
                new Button("Verify Connection", ()=>hub.StartCoroutine(connector.VerifyConnection())).Draw();
            }).AddBelow(()=>{
                GUILayout.BeginHorizontal();  
                if (GUILayout.Button(send, GUILayout.Height(32))) hub.StartCoroutine(connector.Send());           
                if ( GUILayout.Button(clear, GUILayout.Width(32), GUILayout.Height(32))) connector.Clear();
                GUILayout.EndHorizontal();
            });
            Modify("messageIn").AddBelow(()=>{
                DrawLine();
                AddSpace(SingleLineHeight);
            });

        }
    }
    public abstract partial class Connector
    {
        public static MySQLConnector Default => LoadOrCreate<MySQLConnector>(DefaultConnectorPath);
    }
}
#endif