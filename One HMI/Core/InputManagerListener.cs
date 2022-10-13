#if !ENABLE_INPUT_SYSTEM
using UnityEngine;
namespace Sean21.OneHMI
{
    // public class InputManagerHelper : MonoBehaviour, IInputManagerListener
    [CreateAssetMenu(fileName = "Input Manager Listener",
    menuName = Generics.createAssetMenuRoot + "Input Manager Listener")]
    public class InputManagerListener : ScriptableObject
    {
        [Header("Input Manager Axes")]
        public string pointerDeltaX = "Mouse X";
        public string pointerDeltaY = "Mouse Y";
        public string viewPan = "Fire3";
        public string viewOrbit = "Fire2";
        public string viewZoom = "Mouse ScrollWheel";
        public string select = "Fire1";
        
        protected virtual float panAdapter => 150;
        protected virtual float orbitAdapter => 1500;
        protected virtual float zoomAdapter => 2000;
        private Vector2 mouseDelta => new Vector2(Input.GetAxis(pointerDeltaX), Input.GetAxis(pointerDeltaY));
        public virtual Vector2 PassPointerPosition() {
            return Input.mousePosition;
        }
        public virtual Vector2 PassPanDelta() {
            return mouseDelta * panAdapter;
        }
        public virtual Vector2 PassOrbitDelta() {
            return mouseDelta * orbitAdapter;
        }  
        public virtual float PassZoomDelta() {
            return Input.GetAxis(viewZoom) * zoomAdapter;
        }
        public virtual bool PassViewPan() {
            return Input.GetButton(viewPan);
        }
        public virtual bool PassViewPanStarted() {
            return Input.GetButtonDown(viewPan);
        }
        public virtual bool PassViewOrbit() {
            return Input.GetButton(viewOrbit);
        }
        public virtual bool PassViewOrbitStarted() {
            return Input.GetButtonDown(viewOrbit);
        }
        public virtual bool PassSelect() {
            return Input.GetButtonDown(select);
        }
    }
    // public interface IInputManagerListener {
    //     public Vector2 PassPointerPosition();
    //     public Vector2 PassPanDelta();
    //     public Vector2 PassOrbitDelta();
    //     public float PassZoomDelta();
    //     public bool PassViewPan();
    //     public bool PassViewPanStarted();
    //     public bool PassViewOrbit();
    //     public bool PassViewOrbitStarted();
    // }
}
#endif
