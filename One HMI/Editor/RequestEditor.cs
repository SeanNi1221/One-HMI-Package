#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace Sean21.OneHMI
{
    [CustomEditor(typeof(Request), true)]
    public class RequestEditor : OneHMIEditor
    {
        Request request;
        protected virtual void OnEnable() {
            request = target as Request;
            Modify("connector").UnfoldSelfBelowBuiltin();
            // Modify("createDatabaseCommand").AddBelow(()=>ExecutedButton());
        }
        protected virtual void ExecutedButton(Action onClick) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            new Button("Execute", onClick);
            EditorGUILayout.EndHorizontal();            
        }
    }
    public partial class Request
    {
        void Reset() {
            node = GetComponent<Node>();
            connector = Connector.Default;
            databaseName = "meta_hmi";
            nodeTableName = "node";
            connector.ResetCreateDatabaseCommand(this);
            connector.ResetCreateNodeTableCommand(this);
            connector.ResetGetNodeSQL(this);
        }
    }
}
#endif