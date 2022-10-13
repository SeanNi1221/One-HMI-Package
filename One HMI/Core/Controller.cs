using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Sean21.OneHMI
{
    using static Generics;
    [ExecuteInEditMode]
    public partial class Controller : MonoBehaviour
    {
        [Tooltip("On each loading of Nodes' Mesh Renderers, automatically expand Scene Bounds to enclosure the current one.")]
        public bool adaptiveSceneBounds = true;
        [Tooltip("Some behaviours of Camera Rig will take base on Scene Bounds")]
        public Bounds sceneBounds = new Bounds(new Vector3(0f, 2.5f, 0f), new Vector3(10f,5f,10f));
        // [SerializeField] private HMIHub _hub;
        // public HMIHub hub {
        //     get => this.NullRef_Error(_hub, "Cannot get Controller work when there's no HMIHub in the scene.");
        //     set => _hub = value;
        // }
        [SerializeField]
        [HideInInspector]
        private HMIHub _hub;
        private HMIHub hub {
            get {
                if (HMIHub.i) {
                    _hub = HMIHub.i;
                    return _hub;
                }
                else return _hub;
            }
        }
        private static readonly Bounds defaultSceneBounds = new Bounds(Vector3.zero, Vector3.one);
        private ControllerSetting setting => hub.schema.controllerSetting;
        private List<HMIAction> bindingActions => hub.schema.inputBindingActions;
        public Camera cam => hub.cam;
        public Canvas canvas => hub.theme.canvas;

#region Properties
        public HMISelectable activeSelectable{get; set;}
        public Coroutine viewTransition{get; set;}
        public Vector3 center{ get; set; }
        public Vector3 initialCenter => camForm.position + camForm.forward*setting.initialRadius;
        public Vector3 up => setting.useWorldOrientation? Vector3.up : transform.up;
        public Vector3 forward => setting.useWorldOrientation? Vector3.forward : transform.forward;
        public Transform camForm {
            get{
                if (hub == null) {
                    Debug.Log("hub is null!");
                }
                if (cam == null) {
                    Debug.Log("cam is null!");
                    return null;
                }
                return cam.transform;
            }
        }

        public Vector3 location => camForm.position - center;
        public Vector3 direction => location.normalized;
        public Vector3 directionHorizontal => Vector3.ProjectOnPlane(location, up).normalized
            * (upsideDown? -1 : 1);
        public float angleHorizontal => Vector3.SignedAngle(-forward, directionHorizontal, up);
        public float angleVertical => 90-Vector3.SignedAngle(direction, up, currentRight);
        public Vector3 currentRight => Vector3.Cross(location, up).normalized * (upsideDown? -1 : 1);
        public bool upsideDown => Vector3.Dot(up, camForm.up)>0? false : true;
        public virtual Ray pointerRay => cam.ScreenPointToRay(pointerPosition);
        public float currentRadius => Vector3.Distance(camForm.position, center);
        private float panRate => Time.deltaTime * currentRadius/setting.initialRadius;
        private float zoomRate => Time.deltaTime * currentRadius/setting.initialRadius;
        private float orbitRateH => Time.deltaTime;
        private float orbitRateV => Time.deltaTime;

        //Input variables
        protected virtual Vector2 pointerPosition {get; set;}
        protected virtual Vector2 panDelta {get; set;}
        protected virtual Vector2 orbitDelta {get; set;}
        protected virtual float zoomDelta{get; set;}
        protected virtual bool isPanning {get; set;}
        protected virtual bool isOrbiting {get; set;}
        protected virtual bool isZooming {get; set;}
        #endregion
        #region Messages
        partial void ed_Awake();
        void Awake() {
            ed_Awake();
        }
        ///<summary>OnEnable() for Input System</summary>
        partial void is_OnEnable();
        ///<summary>OnEnable() for Input Manager</summary>
        partial void im_OnEnable();
        ///<summary>OnEnable() for Editor</summary>
        partial void ed_OnEnable();
        protected virtual void OnEnable() {
            // if (Application.isPlaying) Debug.Log("is playing");
            // else Debug.Log("not playing");
            // Debug.Log("Controller OnEnable!");
            is_OnEnable();
            im_OnEnable();
            ed_OnEnable();
            InitializeCenter();
            AlignTransform();
        }
        partial void im_OnDisable();
        partial void is_OnDisable();
        protected virtual void OnDisable(){
            is_OnDisable();
            im_OnDisable();
        }
        void Start() {
            if (setting.fullScreenOnStart) Screen.fullScreen = true;
            PromptLine.Print("<size=150%><sprite=0 color=#96B3C9FF></size> Select    <size=150%><sprite=1 color=#96B3C9FF></size> Orbit    <size=150%><sprite=2 color=#96B3C9FF></size> Pan    <size=150%><sprite=4 color=#96B3C9FF></size> Zoom    <b>F11</b> Fullscreen    <b>X</b> Expert Mode");
        }
        ///<summary>Update() for Input Manager</summary>
        partial void im_Update();
        void Update() {
            //Check for transform change and re-align
            if (setting && !setting.useWorldOrientation) {
                if (transform.hasChanged) {
                    AlignTransform();
                    transform.hasChanged = false;
                }
            }
            im_Update();
        }
#endregion
#region Operations
        public virtual void ClearActive(){
            if (activeSelectable) activeSelectable.Deselect();
        }
        ///<summary>Make <see cref="sceneBounds"/> enclose all objects of type <see cref="Node"/> at minimum possible size.</summary>
        public virtual void ResetSceneBounds(){
            Bounds b = default;
            foreach(var node in Node.Manifest.Values)
                b.Encapsulate(node.bounds);
            sceneBounds = b;
        }
        protected virtual void Select() {
            if (IsPointerOverUI) return;
            if (!Physics.Raycast(pointerRay, out var pointerHit, setting.rayMaxDistance, ~(1<<hub.schema.areaLayer))) goto cancel;
            // Debug.Log($"hit: {pointerHit.transform.name}");
            HMISelectable selected = pointerHit.transform.GetComponent<HMISelectable>();
            if (!selected) goto cancel;
            Debug.Log($"selected: {selected.name}");
            selected.Select();
            return;
        cancel:
                ClearActive();
        }
#endregion
#region Camera Rig Delta Behavious
        public void AlignTransform() {
            camForm.rotation = Quaternion.LookRotation(camForm.forward, up);
        }
        protected virtual void UpdateViewZoom(float delta, bool isON = true) {
            if (!isON) return;
            if (IsPointerOverUI) return;
            delta = setting.invertZoom? delta : -delta;
            float zoomStep = delta * setting.viewZoomSpeed * zoomRate;
            var offset = direction * zoomStep;
            TranslateRig(offset);
        }

        protected virtual void UpdateViewPan(Vector2 delta, bool isOn = true) {
            if (!isOn) return;
            delta = new Vector2(
                setting.invertPanX? delta.x : -delta.x,
                setting.invertPanY? delta.y : -delta.y
            );
            var offset = camForm.TransformVector(delta) * setting.viewPanSpeed * panRate;
            TranslateRig(offset);
        }
        protected virtual void UpdateViewOrbit(Vector2 delta, bool isOn = true) {
            if (!isOn) return;
            delta = new Vector2(
                setting.invertOrbitH? -delta.x : delta.x,
                setting.invertOrbitV? delta.y : -delta.y
            );
            var angleOffsetH = delta.x * orbitRateH * setting.viewOrbitSpeedHorizontal;
            var angleOffsetV = delta.y * orbitRateV * setting.viewOrbitSpeedVertical;
            camForm.RotateAround(center, up, angleOffsetH);
            camForm.RotateAround(center, camForm.right, angleOffsetV);
        }
        ///<summary>Set center regarding to pointer position, ignoring <see cref="Schema.areaLayer"/>.</summary>
        protected virtual void SetCenterByPointer()
        {
            if (!setting.enableDynamicCenter) goto initialize;
            if (!Physics.Raycast(pointerRay, out var pointerHit, setting.rayMaxDistance, ~(1<<hub.schema.areaLayer))) goto initialize;
            center = pointerHit.point;
            // Debug.Log($"set center: {center}");
            return;
            initialize:
            InitializeCenter();
        }
        ///<summary>Translate Camera together with Center.</summary>
        protected virtual void TranslateRig(Vector3 delta) {
            center += delta;
            camForm.Translate(delta, Space.World);
        }
#endregion
#region Camera Rig Behavious
        public void InitializeCenter() {
            center = initialCenter;
        }
        ///<summary>Automatically ajust camera to fit to target object or the entire scene</summary>
        public virtual IEnumerator Frame(Bounds targetBounds, float duration = 0.3f, float expansion = 2.5f, Func<float, float> interpolationMethod = null) {
            Debug.Log($"bounds size:{targetBounds.size}");
            //Calculate viewport bound size
            float targetFrameExtend = Mathf.Max(Mathf.Abs(targetBounds.extents.x), Mathf.Abs(targetBounds.extents.y), Mathf.Abs(targetBounds.extents.z)) * expansion;
            //Calculate the distance between camera and center
            float targetDistance = targetFrameExtend / Mathf.Tan(cam.fieldOfView/2 * Mathf.Deg2Rad);
            
            Vector3 centerStart = center;
            Vector3 centerDest = targetBounds.center;
            
            Vector3 camStart = camForm.position;
            Vector3 camDest = centerDest - camForm.forward * targetDistance;

            interpolationMethod ??= Fitting.Linear;
            for(float time = 0; time< duration; time += Time.deltaTime) {
                float t = interpolationMethod(time/duration);
                center = Vector3.Lerp(centerStart, centerDest, t);
                camForm.position = Vector3.Lerp(camStart, camDest, t);
                yield return null;
            }
        }
        public IEnumerator ShiftLens(Vector2 target, float duration = 0.3f, Func<float, float> interpolationMethod = null) {
            Vector2 lensShiftStart = cam.lensShift;
            interpolationMethod ??= Fitting.Linear;
            for(float time = 0; time< duration; time += Time.deltaTime) {
                float t = interpolationMethod(time/duration);
                cam.lensShift = Vector2.Lerp(lensShiftStart, target, t);
                yield return null;
            }
        }
        ///<summary>Morph camera frustum</summary>
        public virtual IEnumerator Morph(Matrix4x4 start, Matrix4x4 end, float duration = 0.3f, Func<float, float> interpolationMethod = null) {
            // Debug.Log("starting morphting...");
            Matrix4x4 current = start;
            interpolationMethod ??= Fitting.Linear;
            float t = 0;
            for(float time = 0; time< duration; time += Time.deltaTime) {
                t = interpolationMethod(time/duration);
                for (int i=0; i<16; i++) current[i] = Mathf.Lerp(start[i], end[i], t);
                cam.projectionMatrix = current;
                // Debug.Log(cam.projectionMatrix);
                yield return null;
            }
            cam.projectionMatrix = end;
            // Debug.Log(cam.projectionMatrix);
        }
        public virtual IEnumerator MorphCanvasPlaneDistance(float start, float end, float restoreOnFinished, float duration = 0.3f, Func<float, float> interpolationMethod = null) {
            interpolationMethod ??= Fitting.Linear;
            float t = 0;
            for(float time = 0; time< duration; time += Time.deltaTime) {
                t = interpolationMethod(time/duration);
                canvas.planeDistance = Mathf.Lerp(start, end, t);
                yield return null;
            }
            canvas.planeDistance = restoreOnFinished;
        }
        ///<summary>Move Camera Rig to</summary>
        // public virtual IEnumerator MoveTo() {

        // }
#endregion
        // public void BindCamera() {
        //     hmiCamera??= cam.GetComponent<HMICamera>()??
        //         cam.gameObject.AddComponent<HMICamera>();
        //     hmiCamera.controller = this;
        // }
        // public void UnBindCamera() {
        //     if(!hmiCamera) return;
        //     DestroyImmediate(hmiCamera);
        // } 
    }
}
