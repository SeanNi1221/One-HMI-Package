#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Sean21.OneHMI
{
    [CustomEditor(typeof(LabelObject))]
    public class LabelObjectEditor : OneHMIEditor
    {
        LabelObject label;
        void OnEnable() {
            label = target as LabelObject;
#if USE_TMP
            Modify("useTMP").OnValueChangeDelayCall(
                () => label.generalText.SwitchTMP(label.useTMP, label.gameObject)
            );
#endif
        }
    }
}
#endif