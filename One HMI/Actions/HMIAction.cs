using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace Sean21.OneHMI
{
    // using static Generics;
    // [ExecuteInEditMode, CreateAssetMenu(fileName = "HMI Action", menuName = "One HMI/Action")]
    public abstract partial class HMIAction : ScriptableObject
    {
        // public HMIHub hub{get; set;}
        // public string axisName;
        // public HMIHub hub{get; set;}
        public HMIHub hub => HMIHub.i;
        public HashSet<Widget> widgets = new HashSet<Widget>();
        public bool inProgress{get; protected set;}
        public bool isON{get; protected set;}
        // protected virtual partial void ed_OnEnable();
        protected virtual void OnEnable(){
            // ed_OnEnable();
        }
        public virtual void Perform(){
            PerformBool(!isON);
        }
        public virtual void PerformBool(bool value){
            isON = value;
            SyncToggles(value);
        }
#if ENABLE_INPUT_SYSTEM
        public virtual void PassInput(InputAction.CallbackContext context) {
            Perform();
        }
#endif
        protected virtual void SyncToggles(bool value) {
            foreach(var widget in widgets) {
                var toggle = widget.toggle;
                if (toggle) toggle.SetIsOnWithoutNotify(value);
            }
        }
#if ENABLE_INPUT_SYSTEM
#region Input System
        public bool autoBindInput;
        public InputAction inputAction;
        [Header("Bind to phases")]
        public bool started = false;
        public bool performed = true;
        public bool canceled = false;
#endregion
#else
#region Input Manager
        public enum BindingMethod {
            None, key, Button
        }
        public BindingMethod bindTo;
        public KeyCode key;
        [Header("Bind to phases")]
        public bool down = true;
        public bool held = false;
        public bool up = false;
        public virtual void PassKeyDown() {
            if (Input.GetKeyDown(key)) Perform();
        }
        public virtual void PassKey() {
            if (Input.GetKey(key)) Perform();
        }
        public virtual void PassKeyUp() {
            if (Input.GetKeyUp(key)) Perform();
        }
        public virtual void PassButtonDown() {
            if (Input.GetButtonDown(this.name)) Perform();
        }        
        public virtual void PassButton() {
            if (Input.GetButtonDown(this.name)) Perform();
        }        
        public virtual void PassButtonUp() {
            if (Input.GetButtonDown(this.name)) Perform();
        }        
#endregion
#endif
    }
}
