#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using UnityEditor.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace Sean21.OneHMI
{
    using static Generics;
    using static EditorGenerics;
    [CustomEditor(typeof(Controller), true)]
    public partial class ControllerEditor : OneHMIEditor
    {
        Controller ctr;
        HMIHub hub => HMIHub.i;
        ControllerSetting setting => hub == null ? null : hub.schema == null ? null : hub.schema.controllerSetting;
        Editor settingEditor;
        partial void is_OnEnable();
        partial void im_OnEnable();
        protected virtual void OnEnable(){
            ctr = target as Controller;
            is_OnEnable();
            im_OnEnable();
            if (!setting) return;
            Modify("adaptiveSceneBounds").AddAbove(() => {                
                ObjectContentFoldedBuiltin(setting, ref settingEditor); 
                DrawLine();
                AddSpace(SingleLineHeight);
            });
            Modify("drawGizmo").DisplayAs("Draw Gizmo (Editor Only)").AddBelow(()=>{
                if (currentProp.boolValue) {
                    EditorGUILayout.HelpBox("Enabling this will significantly reduce Editor Scene View's frame rate.", MessageType.Warning);
                    AddSpace();
                }
            });
        }
        protected override void OnBeginning() {
            base.OnBeginning();
            if (!hub) {
                EditorGUILayout.HelpBox("Cannot find HMIHub!", MessageType.Error);
                if (GUILayout.Button("Create HMIHub")) {
                    HMIHubEditor.Instantiate();
#if ENABLE_INPUT_SYSTEM
                    ctr.EditorResetPlayerInput();
#endif
                }
                return;
            }
        }
#if ENABLE_INPUT_SYSTEM
        partial void is_OnEnable(){
            Modify("_playerInput").MoveToEnd().
            AddBelow(() => { if (GUILayout.Button("Reset Player Input")) ctr.EditorResetPlayerInput(); });            
        }
#else
        partial void im_OnEnable() {
            UnfoldSelfBelowBuiltin("_imListener");
        }
#endif
        void InitializeCenter() {
            ctr.InitializeCenter();
        }
        void OnSceneGUI() {
            if (!setting || !ctr.drawGizmo) return;
            var center = ctr.center;
            var camForm = ctr.camForm;
            var up = ctr.up;
            var forward = ctr.forward;
            var currentRadius = ctr.currentRadius;
            var initialCenter = ctr.initialCenter;
            var initialRadiusBuffer = ctr.initialRadiusBuffer;
            var directionHorizontal = ctr.directionHorizontal;
            var currentRight = ctr.currentRight;
            var angleHorizontal = ctr.angleHorizontal;
            var upsideDown = ctr.upsideDown;
            var angleVertical = ctr.angleVertical;
            var location = ctr.location;
            var sceneBounds = ctr.sceneBounds;


            Handles.DrawLine(center, camForm.position, 1f);
            //Static
            Handles.color = Color.green;
            Handles.DrawLine(center, center + up*currentRadius);
            Handles.DrawLine(center, center + -forward*currentRadius);
            Handles.DrawWireDisc(center, up, currentRadius);
            //Dynamic
            Handles.color = Color.cyan;
            Vector3 _initialCenter = initialCenter;
            Handles.DrawLine(camForm.position, _initialCenter);
            Handles.Label(Vector3.Lerp(_initialCenter, camForm.position, 0.5f), setting.initialRadius.ToString("F2"));
            
            //update center if initialRadius changes in edit mode           
            if (!Application.isPlaying && setting.initialRadius != initialRadiusBuffer) {
                InitializeCenter();
                initialRadiusBuffer = setting.initialRadius;
            }
            if (!Application.isPlaying) {
                if (setting.initialRadius != initialRadiusBuffer) {
                    InitializeCenter();
                    initialRadiusBuffer = setting.initialRadius;
                }
                if (camForm.hasChanged) {
                    InitializeCenter();
                    camForm.hasChanged = false;                    
                }
            }
            Handles.color = Color.yellow;
            Handles.DrawLine(center, center + directionHorizontal * currentRadius);
            Handles.Label(center + directionHorizontal*currentRadius/2, 
                currentRadius.ToString("F2"));
            Handles.DrawLine(center, center+currentRight);

            //Horizontal
            Handles.DrawWireArc(center, up, -forward, angleHorizontal, currentRadius);
            Handles.Label(
                center + Quaternion.AngleAxis(angleHorizontal/2, up)*(-forward*currentRadius),
                angleHorizontal.ToString("F2")+"°");
            //Vertical
            if (upsideDown) Handles.color = Color.red;
            Handles.DrawWireArc(center, currentRight, 
                directionHorizontal,
                angleVertical, currentRadius);
            Handles.Label(
                center + Quaternion.AngleAxis(-angleVertical/2, currentRight)*location, 
                angleVertical.ToString("F2")+"°");
            Gizmos.color = Color.yellow;
            // //Scene bounds
            var boundsColor = Color.cyan;
            boundsColor.a = 0.3f;
            Handles.color = boundsColor;
            Handles.DrawWireCube(sceneBounds.center, sceneBounds.size);
        }
    }

    public partial class Controller
    {
        internal float initialRadiusBuffer;
        // [MenuItem("GameObject/One HMI/Controller")]
        // public static void Instantiate(){
        //     new GameObject("HMI Controller").AddComponent<Controller>();
        // }
        public bool drawGizmo;
        partial void ed_im_Awake();
        partial void ed_Awake() {
            PurgeInputModule();
            ed_im_Awake();
        }
        partial void ed_is_OnEnable();
        partial void ed_im_OnEnable();
        partial void ed_OnEnable() {
            ed_is_OnEnable();
            ed_im_OnEnable();
        }
        partial void is_Reset();
        partial void im_Reset();
        void Reset() {
            drawGizmo = false;
            PurgeInputModule();
            is_Reset();
            im_Reset();
        }
        partial void im_OnValidate();
        partial void is_OnValidate();
        void OnValidate(){
            im_OnValidate();
            is_OnValidate();
        }
        void PurgeInputModule() {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
            var eventSystem = FindObjectOfType<EventSystem>();
            if(eventSystem)
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(eventSystem.gameObject);
        }
#if ENABLE_INPUT_SYSTEM
        #region Input System
        partial void is_Reset()
        {
            EditorResetPlayerInput();
        }
        partial void ed_is_OnEnable() {
            if (!playerInput || !playerInput.actions || !playerInput.uiInputModule) EditorResetPlayerInput();
        }
        public virtual void EditorResetPlayerInput() {
            // Debug.Log("Reset Started!");
            StartCoroutine(EditorResetingPlayerInput());
        }
        IEnumerator EditorResetingPlayerInput() {
            BeginForceUpdateInEditMode();
            playerInput = GetComponent<PlayerInput>()?? gameObject.AddComponent<PlayerInput>();
            SetUpInputModule();

            //Wait for Editor to be instantiated
            Editor piEditor = null;
            yield return new WaitUntil(()=>{
                piEditor = playerInput.GetEditor();
                return piEditor !=null;
            }); 
            //Read Actions Asset
            playerInput.actions = hub.schema.inputActionAsset;
            if (playerInput.actions == null) Debug.LogError("Cannot Reset Input because no Input Action Asset is assigned in Schema!");
            EditorApplication.delayCall += ()=> piEditor.InvokeVoid("OnActionAssetChange");
            // Debug.Log("Asset Loaded!");

            //Set Behavior
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            EditorApplication.delayCall += ()=> piEditor.InvokeVoid("OnNotificationBehaviorChange");
            
            //Register Events
            EditorApplication.delayCall += AddPersistentListeners;
            
            //Expand Events Foldout
            piEditor.SetFieldValue("m_EventsGroupUnfolded", true);
            //Expand the first Action Map Foldout
            bool[] _foldouts = null;
            yield return new WaitUntil(()=>{
                _foldouts = piEditor.GetFieldValue<bool[]>("m_ActionMapEventsUnfolded");
                return _foldouts != null;
            }); 
            _foldouts[0] = true;
            piEditor.SetFieldValue("m_ActionMapEventsUnfolded", _foldouts);
            EndForceUpdateInEditMode();
        }
        ///<summary>Should be called after PlayerInput is Instantiated.</summary>
        protected virtual void SetUpInputModule() {
            GameObject moduleObject = null;
            //Destroy Standalone Input Module
            var inputManagerModule = FindObjectOfType<StandaloneInputModule>();
            if (inputManagerModule) {
                moduleObject = inputManagerModule.gameObject;
                DestroyImmediate(inputManagerModule);
            }
            //Find existing Input System Module
            playerInput.uiInputModule ??= FindObjectOfType<InputSystemUIInputModule>();
            if (playerInput.uiInputModule) return;
            //Add Input System Module
            var eventSystem = FindObjectOfType<EventSystem>();
            if(eventSystem) moduleObject ??= eventSystem.gameObject;
            moduleObject ??= new GameObject("EventSystem").AddComponent<EventSystem>().gameObject;
            playerInput.uiInputModule = moduleObject.AddComponent<InputSystemUIInputModule>();            
        }
        private void AddPersistentListeners() {
            // Debug.Log($"{name} - Events count:{playerInput.actionEvents.Count}");
            foreach(var aEvent in playerInput.actionEvents) {
                // Debug.Log(aEvent.actionName);
                //Remove All existing listeners to avoid duplications.
                while(aEvent.GetPersistentEventCount()>0)
                    UnityEventTools.RemovePersistentListener(aEvent, 0);
                //Add corresponding listener by action name prefix.
                switch(aEvent.actionName) {
                    default: break;
                    case string name when name.StartsWith("ViewPort/PointerPosition", StringComparison.Ordinal):
                        UnityEventTools.AddPersistentListener(aEvent, PassPointerPosition);
                        break;
                    case string name when name.StartsWith("ViewPort/PointerDelta", StringComparison.Ordinal):
                        UnityEventTools.AddPersistentListener(aEvent, PassPanDelta);
                        UnityEventTools.AddPersistentListener(aEvent, PassOrbitDelta);
                        break;
                    case string name when name.StartsWith("ViewPort/ViewZoom", StringComparison.Ordinal):
                        UnityEventTools.AddPersistentListener(aEvent, PassZoomDelta);
                        UnityEventTools.AddPersistentListener(aEvent, PassSetCenter);
                        break;
                    case string name when name.StartsWith("ViewPort/ViewPan", StringComparison.Ordinal):
                        UnityEventTools.AddPersistentListener(aEvent, PassViewPan);
                        UnityEventTools.AddPersistentListener(aEvent, PassSetCenter);
                        break;
                    case string name when name.StartsWith("ViewPort/ViewOrbit", StringComparison.Ordinal):
                        UnityEventTools.AddPersistentListener(aEvent, PassViewOrbit);
                        UnityEventTools.AddPersistentListener(aEvent, PassSetCenter);
                        break;
                    case string name when name.StartsWith("ViewPort/Select", StringComparison.Ordinal):
                        UnityEventTools.AddPersistentListener(aEvent, PassSelect);
                        break;
                }
            }
        }
        #endregion
#else
        #region Input Manager
        partial void ed_im_Awake() {
            // SetUpInputModule();
        }
        partial void ed_im_OnEnable() {
        }
        partial void im_OnValidate() {
            SerializableGet(ref _imListener, hub.schema.inputManagerListener);
        }
        partial void im_Reset() {
            _imListener = hub.schema.inputManagerListener;
            SetUpInputModule();
        }
        protected virtual void SetUpInputModule() {
            //Destroy PlayerInput
            var playerInput = GetComponent("PlayerInput");
            if (playerInput) 
                DestroyImmediate(playerInput);

            GameObject moduleObject = null;
            //Destroy InputSystem Input Module
            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem) {
                moduleObject = eventSystem.gameObject;
                var inputSystemModule = eventSystem.GetComponent("InputSystemUIInputModule");
                if (inputSystemModule) DestroyImmediate(inputSystemModule);
            }
            //Find existing Standalone Input Module
            var inputManagerModule = FindObjectOfType<StandaloneInputModule>();
            if (inputManagerModule) return;
            //Add Input System Module
            moduleObject ??= new GameObject("EventSystem").AddComponent<EventSystem>().gameObject;
            moduleObject.AddComponent<StandaloneInputModule>();
        }
#endregion
#endif
    }
}
#endif