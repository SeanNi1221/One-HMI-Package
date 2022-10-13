#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Sean21.OneHMI
{
    [CustomEditor(typeof(MySQLConnector), true)]
    public class MySQLConnectorEditor : ConnectorEditor
    {
        MySQLConnector msConnector;
        protected override void OnEnable() {
            base.OnEnable();
            msConnector = target as MySQLConnector;
            Modify("backendLocation").MoveBelow("buildUri");
            Modify("serverName").MoveBelow("backendLocation");

        }
    }
    public partial class MySQLConnector
    {
        void Reset() {
            userName = "root";
        }
    }
}
#endif