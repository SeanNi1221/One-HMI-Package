#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.UI;
using System;
namespace Sean21.OneHMI
{
    [CustomEditor(typeof(Widget), true)]
    public class WidgetEditor : OneHMIEditor
    {
        Editor actionEditor;
        Widget tg;
        SerializedProperty actionProp;

        protected virtual void OnEnable() {
            tg = target as Widget;
            Disable("_theme");
            var buttonField = Modify("_button");
            if (buttonField.serializedProperty.objectReferenceValue != null) buttonField.Disable();
            else buttonField.Hide();
            var toggleField = Modify("_toggle");
            if (toggleField.serializedProperty.objectReferenceValue != null) toggleField.Disable();
            else toggleField.Hide();
            UnfoldSelfBelowBuiltin("action");
        }
    }
    public partial class Widget{
        
        partial void ed_Awake() {
            this.SerializableGetComponent(ref _button);
            this.SerializableGetComponent(ref _toggle);
        }
        partial void ed_OnEnable()
        {
            if (!theme) ValidateTheme();
        }
        void OnTransformParentChanged()
        {
            ValidateTheme();
        }
        public void ValidateTheme() {
            // Debug.Log("Validating...");
            theme = GetComponentInParent<Theme>();
            if (!theme) return;
            if (!theme.widgets.Contains(this))
                theme.widgets.Add(this);
        }
    }
}
#endif