#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Sean21.OneHMI
{
    [CustomEditor(typeof(HMIAction), true)]
    public class HMIActionEditor : OneHMIEditor
    {
        public HMIAction action;
        [SerializeField]
        Schema schema;
#if ENABLE_INPUT_SYSTEM
        SerializedProperty inputActionProp;
        SerializedProperty startedProp;
        SerializedProperty performedProp;
        SerializedProperty canceledProp;
#else
        // GUIStyle axisNameStyle = new GUIStyle(EditorStyles.boldLabel);
        SerializedProperty bindToProp;
        SerializedProperty keyProp;
        SerializedProperty downProp;
        SerializedProperty heldProp;
        SerializedProperty upProp;
#endif
        protected virtual void BindingButtons() {
            ButtonRow(
                new Button("Add to current schema", ()=>{
                    if (!schema.inputBindingActions.Contains(action)) {
                        schema.inputBindingActions.Add(action);
                        Debug.Log($"Successfully added '{action.name}' to the current schema");
                    } else Debug.LogWarning($"'{action.name}' is already in the current schema. Please add it in the schema's inspector if duplicated binding is intended!");
                }),
                new Button("Remove from current schema", ()=>{
                    if (schema.inputBindingActions.Remove(action)) {
                        Debug.Log($"Removed '{action.name}' from the current schema");
                    } else Debug.LogWarning($"'{action.name}' is not in the current schema, nothing removed!");

                })
            );
        }

        protected virtual void OnEnable() {
            if (modifiedFields == null) Debug.Log("started OnEnable, modfiedFields is null.");
            // if ( == null) Debug.Log("started OnEnable, modfiedFields is null.");
            action = target as HMIAction;
            schema ??= action.hub ? action.hub.schema : null;

#if ENABLE_INPUT_SYSTEM
            Modify("autoBindInput").AddSpaceAbove(10).MoveToEnd();
            inputActionProp = serializedObject.FindProperty("inputAction");
            startedProp = serializedObject.FindProperty("started");
            performedProp = serializedObject.FindProperty("performed");
            canceledProp = serializedObject.FindProperty("canceled");
            Modify(inputActionProp).MoveToEnd().DrawAs(() => { if (action.autoBindInput) EditorGUILayout.PropertyField(inputActionProp); else return; });
            Modify(startedProp).MoveToEnd().DrawAs(() => { if (action.autoBindInput) EditorGUILayout.PropertyField(startedProp); else return; });
            Modify(performedProp).MoveToEnd().DrawAs(() => { if (action.autoBindInput) EditorGUILayout.PropertyField(performedProp); else return; });
            Modify(canceledProp).MoveToEnd().DrawAs(() => { if (action.autoBindInput) EditorGUILayout.PropertyField(canceledProp);  else return; }).
                AddSpaceBelow(16).AddBelow(BindingButtons);
#else

            bindToProp = serializedObject.FindProperty("bindTo");
            keyProp = serializedObject.FindProperty("key");
            downProp = serializedObject.FindProperty("down");
            heldProp = serializedObject.FindProperty("held");
            upProp = serializedObject.FindProperty("up");
            Modify(bindToProp).AddSpaceAbove(10).MoveToEnd();
            Modify(keyProp).MoveToEnd().DrawAs( () => {
                switch (action.bindTo) {
                    default: break;
                    case HMIAction.BindingMethod.Button:
                        GUIStyle axisNameStyle = new GUIStyle(EditorStyles.boldLabel);                        
                        axisNameStyle.fontSize = 16;
                        EditorGUILayout.LabelField($"Go to 'Project Settings -> Input Manager -> Axes' and add the following axis:");
                        EditorGUILayout.SelectableLabel(action.name, axisNameStyle);
                        break;
                    case HMIAction.BindingMethod.key:
                        EditorGUILayout.PropertyField(keyProp);
                        break;

                }
            });
            Modify(downProp).MoveToEnd().DrawAs(() => { if (action.bindTo != HMIAction.BindingMethod.None) EditorGUILayout.PropertyField(downProp); else return; });
            Modify(heldProp).MoveToEnd().DrawAs(() => { if (action.bindTo != HMIAction.BindingMethod.None) EditorGUILayout.PropertyField(heldProp); else return; });
            Modify(upProp).MoveToEnd().DrawAs(() => { if (action.bindTo != HMIAction.BindingMethod.None) EditorGUILayout.PropertyField(upProp); else return; }).
                AddSpaceBelow(16).AddBelow(BindingButtons);
#endif
        }
    }
    public abstract partial class HMIAction
    {
        // protected virtual partial void ed_OnEnable() {
        //     if (string.IsNullOrEmpty(action.name))
        //         actionOrAxisName = this.GetType().Name;            
        // }
    }

}
#endif