#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sean21.OneHMI
{
    [CustomEditor(typeof(Theme), true)]
    public class ThemeEditor : OneHMIEditor
    {
        void OnEnable() {
            Simplify("widgets", false);
        }
    }
    public partial class Theme
    {
        partial void ed_Awake()
        {
            FetchWidges();
        }
        partial void ed_OnEnable() {
        }
        void FetchWidges() {
            foreach (var widget in GetComponentsInChildren<Widget>()) {
                widget.ValidateTheme();
            }            
        }
    }
}
#endif