using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Sean21.OneHMI
{
    [ExecuteAlways]
    [CreateAssetMenu(fileName = "Schema", menuName = "One HMI/Schema")]
    public partial class Schema : ScriptableObject
    {
        public static Schema current => HMIHub.i ? HMIHub.i.schema : null;
        [Header("Input Bindings")]
        public ControllerSetting controllerSetting;
        public List<HMIAction> inputBindingActions = new List<HMIAction>();
        [Header("Node Settings")]
        public ResourceFileNaming resourceFileNaming;
        public string streamingImageFolder;
        public string streamingDescFolder;
        [Tooltip("Layer for Area, used as pointer raycast filter.")]
        public int areaLayer = 21;
        [Header("Label Default Settings")]
#if USE_TMP
        public bool labelUseTMP = true;
#endif
        public float labelMinSize = 0.025f;
        public float labelMaxSize = 0.06f;
        public float labelfontSize = 14f;
        public Color labelColor = new Color(0.003921569f, 0.8078431f, 0.8156863f, 1f);
        [Header("Miscellaneous")]
        public bool detailedDebugLog;
        partial void im_OnEnable();
        public virtual void Reset() {
            streamingImageFolder = "NodeImages/";
            streamingDescFolder = "NodeDescriptions/";
            resourceFileNaming = ResourceFileNaming.Displayname;
        }
        void OnEnable(){
            im_OnEnable();
        }
        public virtual string NodeResourceFileName(Node node) {
            switch (resourceFileNaming) {
                default: return string.Empty;
                case ResourceFileNaming.Displayname: return node.displayName;
                case ResourceFileNaming.Id: return node.Id;
            }            
        }
        public enum ResourceFileNaming { None, Displayname, Id };
    }
}
