using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sean21.OneHMI
{
    using static Generics;
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class HMICamera : MonoBehaviour
    {
        // [HideInInspector]
        [SerializeField]
        private Camera _cam;
        public Camera cam => this.SerializableGetComponent(ref _cam);
        private HMIHub hub => HMIHub.i;
        void Awake() {
            _cam = GetComponent<Camera>();
        }
        void OnEnable() {
            // Debug.Log($"{name}: hub:{hub}, hub.hmiCam:{hub.hmiCam}, _cam:{_cam}");
            // if(hub.hmiCam == this) hub.BindCamera(cam);
            // else {
            if(hub.hmiCam != this) {
                var _name = name;
                DestroyImmediate(this);
                Debug.Log($"{_name}: No HMIHub binding, removed HMICamera.");
            }
        }
    }
}
