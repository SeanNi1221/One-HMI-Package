#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sean21.OneHMI
{
    using static Generics;
    using static EditorGenerics;
    [CustomEditor(typeof(HMIHub), true)]
    public class HMIHubEditor : OneHMIEditor
    {
        HMIHub hub;
        protected virtual void OnEnable() {
            hub = target as HMIHub;
            UnfoldSelfBelowBuiltin("schema").OnValueChangeAndDelayCall(() => { 
                if (hub.schema) ClearLayerName(hub.schema.areaLayer, AreaLayerName); 
            }, () => { if (hub.schema) SetLayerName(hub.schema.areaLayer, AreaLayerName); }
            ).AddAbove(EditorGUILayout.Space);
            Modify("_cam").OnValueChange(() => hub.ReleaseCamera(hub.cam)).OnValueChangeDelayCall(() => hub.BindCamera(hub.cam));
        }
        [MenuItem("GameObject/One HMI/Create HMI Hub")]
        public static void Instantiate() {
            new GameObject("HMI Hub").AddComponent<HMIHub>();
        }
    }
    public partial class HMIHub 
    {
        partial void ed_OnEnable()
        {
            GetReference(ref schema,  LoadAsset<Schema>(DefaultSchemaPath));
            SetLayerName(schema.areaLayer, AreaLayerName);
            InstantiateIfNeeded(ref controller, "HMI Controller");
            cam.enabled = true;
            FindRef(ref _theme);
        }
    }
}
#endif