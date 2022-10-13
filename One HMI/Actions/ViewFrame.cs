using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sean21.OneHMI
{
    using static Generics;
    [ExecuteInEditMode, CreateAssetMenu(fileName = "View Frame", menuName = "One HMI/Actions/View Frame")]
    public class ViewFrame : HMIAction
    {
        private Controller controller => hub.controller;
        public float duration = 0.3f;
        [Tooltip("Viewport border expansion")]
        public float expansion = 2.5f;
        public override void Perform() {
            controller.StartCoroutine(PerformFrame());
        }
        private IEnumerator PerformFrame() {
            if (inProgress) yield break;
            inProgress = true;
            var active = controller.activeSelectable;
            yield return controller.Frame(
                //Some node selected?
                active? active.viewFrameBounds:
                //No node selected
                controller.sceneBounds,
                duration, expansion, Fitting.Sin
            );
            inProgress = false;
        }
        private IEnumerator FrameTo(Node node) {
            if (inProgress) yield break;
            inProgress = true;            
            inProgress = false;
        }
        void Reset() {
#if ENABLE_INPUT_SYSTEM
            autoBindInput = true;
            inputAction = new UnityEngine.InputSystem.InputAction(binding: "<Keyboard>/F");
#else
            bindTo = BindingMethod.key;
            key = KeyCode.F;
#endif
        }
    }
}
