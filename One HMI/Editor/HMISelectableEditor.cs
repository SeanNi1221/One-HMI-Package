#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Sean21.OneHMI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HMISelectable), true)]
    public class HMISelectableEditor : OneHMIEditor
    {
        HMISelectable s;
        protected virtual void OnEnable() {
            s = target as HMISelectable;
            Modify("enableIndexLine").OnValueChangeDelayCall(s.SyncIndexEnd);
        }
    }
}
#endif