using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Sean21.OneHMI
{
    using static Generics;
    [ExecuteInEditMode]
    public partial class HMIHub : Singleton<HMIHub>
    {        
        public Controller controller;
        [SerializeField]
        private Camera _cam;
        public Camera cam {
            get => SerializableGet(ref _cam, Camera.main?? new GameObject("HMI Camera").AddComponent<Camera>(), () => BindCamera(_cam));
            set {
                ReleaseCamera(_cam);
                BindCamera(value);
                _cam = value;
            }
        }
        [SerializeField] [HideInInspector]
        private HMICamera _hmiCam;
        public HMICamera hmiCam {
            get => _hmiCam;
            private set => _hmiCam = value;
        }
        [SerializeField] private Theme _theme;
        public Theme theme {
            get => SerializableGet(ref _theme);
            set => _theme = value;
        }
        public Schema schema;
        partial void ed_OnEnable();
        protected override void OnEnable()
        {
            base.OnEnable();
            ed_OnEnable();
        }
        protected virtual void OnDestroy() {
            ReleaseCamera(_cam);
        }
        public void BindCamera(Camera cam) {
            if (!cam) { hmiCam = null; return; }
            _cam = cam;
            cam.gameObject.SetActive(false);
            hmiCam = cam.GetComponent<HMICamera>();
            hmiCam ??= cam.gameObject.AddComponent<HMICamera>();
            cam.gameObject.SetActive(true);
        }
        public void ReleaseCamera(Camera cam) {
            if (!cam) return;
            var __hmiCam = cam.GetComponent<HMICamera>();
            if (__hmiCam) DestroyImmediate(__hmiCam);
            _cam = null;
        }
    }
}
