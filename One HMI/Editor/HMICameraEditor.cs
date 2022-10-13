#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Sean21.OneHMI
{
    [CustomEditor(typeof(HMICamera), true)]
    public class HMICameraEditor : OneHMIEditor
    {
        HMICamera hmiCamera;
        protected virtual void OnEnable() {
            hmiCamera = target as HMICamera;
        }
    }
}
#endif