#if !ENABLE_INPUT_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Sean21.OneHMI
{
    using static Generics;
    public partial class Controller
    {
        // private IInputManagerListener _imHelper;
        // public IInputManagerListener imHelper{
        //     get {
        //         if (_imHelper != null) return _imHelper;
        //         _imHelper = GetComponent<IInputManagerListener>()?? gameObject.AddComponent<InputManagerHelper>();
        //         return _imHelper;
        //     }
        //     set { _imHelper = value; }
        // }
        public bool IsPointerOverUI => EventSystem.current.IsPointerOverGameObject();
        private UnityEvent InputsFromActions = new UnityEvent();
        [SerializeField] private InputManagerListener _imListener;
        public InputManagerListener imListener => 
            SerializableGet(ref _imListener, hub.schema.inputManagerListener);
        partial void im_OnEnable() {
            if (Application.isPlaying) {
                foreach (var action in bindingActions) {
                    switch(action.bindTo) {
                        default: break;
                        case HMIAction.BindingMethod.key: 
                            if(action.down) InputsFromActions.AddListener(action.PassKeyDown); 
                            if(action.held) InputsFromActions.AddListener(action.PassKey); 
                            if(action.up) InputsFromActions.AddListener(action.PassKeyUp); 
                            break;
                        case HMIAction.BindingMethod.Button: 
                            if(action.down) InputsFromActions.AddListener(action.PassButtonDown);
                            if(action.held) InputsFromActions.AddListener(action.PassButtonDown);
                            if(action.up) InputsFromActions.AddListener(action.PassButtonDown);
                            break;
                    }
                }                
            }

        }
        partial void im_OnDisable() {
            if (Application.isPlaying) {
                InputsFromActions.RemoveAllListeners();                
            }
        }
        partial void im_Update()
        {
            if (Application.isPlaying)
                ControllerInputs();
        }
        ///<summary>Supposed to be called each frame inside Update().</summary>
        protected virtual void ControllerInputs() {
            pointerPosition = imListener.PassPointerPosition();
            
            panDelta = imListener.PassPanDelta();
            orbitDelta = imListener.PassOrbitDelta();
            zoomDelta = imListener.PassZoomDelta();

            isPanning = imListener.PassViewPan();
            isOrbiting = imListener.PassViewOrbit();
            isZooming = Mathf.Abs(zoomDelta) > 0;
            
            if (imListener.PassViewPanStarted() || imListener.PassViewOrbitStarted() || isZooming)
                SetCenterByPointer();

            UpdateViewPan(panDelta, isPanning);
            UpdateViewOrbit(orbitDelta, isOrbiting);
            UpdateViewZoom(zoomDelta, isZooming);

            if(imListener.PassSelect()) Select();

            InputsFromActions.Invoke();
        }
    }
}
#endif