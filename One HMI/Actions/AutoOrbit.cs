using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sean21.OneHMI
{
    [ExecuteInEditMode, CreateAssetMenu(fileName = "Auto Orbit", menuName = "One HMI/Actions/Auto Orbit")]
    public class AutoOrbit : HMIAction
    {
#region Options
        [Range(0f, 500f)] public float standbyTime = 10f;
        [Range(0.01f, 2f)] public float speed = 1f;
        public bool clockWise = false;
        public float viewExtension = 4f;
#endregion
        private Camera cam;
        private Controller controller;
        // public override void Register(HMIHub hub)
        // {
        //     base.Register(hub);
        //     cam = hub.cam;
        //     controller = hub.controller;
        // }
        // private IEnumerator TogglingAutoOrbit(bool isOn) {
        //     if (isOn) {
        //     }
        //     else {

        //     }

        // }
        // private IEnumerator OrbitAround() {

        // }
        // private IEnumerator ZoomTo()
    }
}
