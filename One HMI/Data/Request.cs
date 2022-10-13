using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sean21.OneHMI
{
    public partial class Request : MonoBehaviour
    {
        public Node node;
        public Connector connector;
        public string databaseName;
        [TextArea(1,30)]
        public string createDatabaseCommand;
        public string nodeTableName;
        [TextArea(1,30)]
        public string createNodeTableCommand;
        [TextArea(1,30)]
        public string getNodeCommand;
    }
}
