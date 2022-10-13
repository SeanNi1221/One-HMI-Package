using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Sean21.OneHMI
{
    using static Generics;
    [ CreateAssetMenu(fileName = "Toggle Projection", menuName = "One HMI/Actions/Toggle Projection")]
    public class ToggleProjection : HMIAction
    {
        public float duration = 0.3f;
        [Tooltip("Enable this to avoid UI abnormal behavious when toggling under [Screen Space - Camera] mode.\n"+
        "If enabled, Render Mode will be set to [Screen Space - Overlay] during the transition, and return to [Screen Space - Camera] when the transition is finished.")]
        [SerializeField]
        private bool doNotUseSSCCanvasDuringTransition = true;
        private bool usingSSCCanvas{ get; set; }
        private Controller controller => hub.controller;
        private Camera cam => controller.cam;
        private float fov => cam.fieldOfView;
        private float near => cam.nearClipPlane;
        private float far => cam.farClipPlane;
        private Canvas canvas => hub? hub.theme.canvas : null;
        private float[] borders {
            get {
                Vector3[] corners = new Vector3[4];
                cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), 
                    Vector3.Project(controller.location, controller.camForm.forward).magnitude,
                    Camera.MonoOrStereoscopicEye.Mono,
                    corners);
                var result = new float[4] {
                    corners[0].x, corners[2].x, corners[0].y, corners[2].y
                };
                cam.orthographicSize = result[3];
                return result;
            }
        }
        private Matrix4x4 ortho {
            get {
                var _borders = borders;
                return Matrix4x4.Ortho(_borders[0], _borders[1], _borders[2], _borders[3], near, far);
            }
        }
        private Matrix4x4 perspective {
            get {
                return Matrix4x4.Perspective(fov, cam.aspect, near, far);
            }
        }
        public override void PerformBool(bool value) {
            controller.StartCoroutine(ToggleOrtho(value));
        }
        private IEnumerator ToggleOrtho(bool isOrtho) {
            if (inProgress || isON == isOrtho ) yield break;
            inProgress = true;
            IEnumerator morph = isOrtho? 
                controller.Morph(cam.projectionMatrix, ortho, duration, Fitting.Sin ) :
                controller.Morph(cam.projectionMatrix, perspective, duration, Fitting.Sin );
                
            if (canvas) usingSSCCanvas = canvas.renderMode == RenderMode.ScreenSpaceCamera;
            SwitchSSCIfNeeded(false);
            yield return morph;
            cam.orthographic = isOrtho;
            SwitchSSCIfNeeded(true);
            isON = isOrtho;
            SyncToggles(isOrtho);
            inProgress = false;
        }
        private void SwitchSSCIfNeeded(bool SwitchToSSC) {
            if (!usingSSCCanvas || !doNotUseSSCCanvasDuringTransition ) return;
            canvas.renderMode = SwitchToSSC? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
        }
        void Reset() {
#if ENABLE_INPUT_SYSTEM
            autoBindInput = true;
            inputAction = new UnityEngine.InputSystem.InputAction(binding: "<Keyboard>/Tab");
#else
            bindTo = BindingMethod.key;
            key = KeyCode.Tab;
#endif
        }
    }
}
