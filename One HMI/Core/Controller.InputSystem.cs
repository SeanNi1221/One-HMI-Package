#if ENABLE_INPUT_SYSTEM
namespace Sean21.OneHMI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.EventSystems;

    public partial class Controller
    {
        protected virtual float panAdapter => 2;
        protected virtual float orbitAdapter => 40;
        protected virtual float zoomAdapter => 1.5f;
        public bool IsPointerOverUI => IsPointerOverGameObject();
        private bool IsPointerOverGameObject() {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = pointerPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
        [SerializeField] private PlayerInput _playerInput;
        public PlayerInput playerInput {
            get => _playerInput;
            set => _playerInput = value;
        }
        partial void is_OnEnable(){
            if (Application.isPlaying) {
                foreach (var action in bindingActions) {
                    if (!action.autoBindInput) continue;
                    if(action.started) action.inputAction.started += action.PassInput;
                    if(action.performed) action.inputAction.performed += action.PassInput;
                    if(action.canceled) action.inputAction.canceled += action.PassInput;
                    action.inputAction.Enable();
                }
            }
        }
        partial void is_OnDisable() {
            if (Application.isPlaying) {
                foreach (var action in bindingActions) {
                    if (!action.autoBindInput) continue;
                    action.inputAction.Disable();
                }                
            }
        }
        public virtual void PassSelect(InputAction.CallbackContext context) {
            if (context.performed) Select();
        }
        public virtual void PassPointerPosition(InputAction.CallbackContext context) {
            pointerPosition = context.ReadValue<Vector2>();
        }
        public virtual void PassPanDelta(InputAction.CallbackContext context) {
            // if (isPanning) panDelta = context.ReadValue<Vector2>();
            UpdateViewPan(context.ReadValue<Vector2>() * panAdapter, isPanning);
        }
        public virtual void PassOrbitDelta(InputAction.CallbackContext context) {
            // orbitDelta = context.ReadValue<Vector2>();
            UpdateViewOrbit(context.ReadValue<Vector2>() * orbitAdapter, isOrbiting);
        }
        public virtual void PassZoomDelta(InputAction.CallbackContext context) {
            zoomDelta = context.ReadValue<Vector2>().y * zoomAdapter;
            isZooming = Mathf.Abs(zoomDelta) > 0;
            if (isZooming) SetCenterByPointer();
            UpdateViewZoom(zoomDelta);
        }
        public virtual void PassSetCenter(InputAction.CallbackContext context) {
            if (context.started) SetCenterByPointer();
        }
        public virtual void PassViewPan(InputAction.CallbackContext context) {
            isPanning = context.ReadValueAsButton();
        }
        public virtual void PassViewOrbit(InputAction.CallbackContext context) {
            isOrbiting = context.ReadValueAsButton();
        }
    }
}
#endif