#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Sean21.OneHMI
{
    using static Generics;
    using static EditorGenerics;
    [CustomEditor(typeof(Schema), true)]
    public partial class SchemaEditor : OneHMIEditor {
        Schema schema;
        protected virtual void OnEnable() {
            schema = target as Schema;
            Modify("drawContainerSockets").DrawAs(DisplayDrawContainerSocketsField);
#if ENABLE_INPUT_SYSTEM
            Modify("_inputActionAsset").MoveAbove("inputBindingActions");
#else
#endif
            Modify("areaLayer").DrawAs(schema.DisplayAreaLayerFiled);
        }
        internal static void DisplayDrawContainerSocketsField(){
            if (!Schema.current) return;
            EditorGUI.BeginChangeCheck();
            Schema.current.drawContainerSockets = EditorGUILayout.ToggleLeft("Draw Sockets", Schema.current.drawContainerSockets); 
            if (EditorGUI.EndChangeCheck()) {
                SceneView.RepaintAll();
            }            
        }
    }
    public partial class Schema {
        public bool drawContainerSockets = true;
        partial void im_OnValidate();
        partial void is_OnValidate();
        void OnValidate() {
        controllerSetting ??= LoadAsset<ControllerSetting>(DefaultControllerSettingPath);
            is_OnValidate();
            im_OnValidate();
        }
        internal void DisplayAreaLayerFiled() {
            EditorGUI.BeginChangeCheck();
            var _areaLayer = EditorGUILayout.IntField(new GUIContent("Area Layer", "Layer for Area, as the layer mask for pointer raycast."), Schema.current.areaLayer);            
            if (EditorGUI.EndChangeCheck()) {                
                if (_areaLayer <8 || _areaLayer>31) return;
                //With old value
                ClearLayerName(areaLayer, AreaLayerName);
                //With new value
                areaLayer = _areaLayer;
                EditorApplication.delayCall += () => SetLayerName(areaLayer, AreaLayerName);
            }
        }
#if ENABLE_INPUT_SYSTEM
        partial void is_OnValidate() {
            inputActionAsset ??= LoadAsset<UnityEngine.InputSystem.InputActionAsset>(DefaultActionsPath);
        }
#else
    partial void im_OnValidate() {
            inputManagerListener ??= LoadAsset<InputManagerListener>(DefaultInputManagerListenerPath);
            if (!inputManagerListener) {
                inputManagerListener = GetReference(ref inputManagerListener, LoadAsset<InputManagerListener>(DefaultInputManagerListenerPath));
            }
        }
#endif
    }
}
#endif