using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sean21.OneHMI
{
    [CreateAssetMenu(fileName = "Controller Setting", 
    menuName = Generics.createAssetMenuRoot + "Controller Setting")]
    public class ControllerSetting : ScriptableObject
    {
        [Header("User Options")]
        [Tooltip("True: use pointer position as center; false: use screen center as center")]
        public bool enableDynamicCenter = true;
        ///<summary>Initial camera orbit radius</summary>
        [Tooltip("Initial camera orbit radius")]
        [Range(1f,100f)] public float initialRadius = 10f;
        [Space]
        public float viewPanSpeed = 1f;
        public float viewZoomSpeed = 1f;
        public float viewOrbitSpeedVertical = 1f;
        public float viewOrbitSpeedHorizontal = 1f;
        [Space]
        public bool invertPanX;
        public bool invertPanY;
        public bool invertOrbitH;
        public bool invertOrbitV;
        public bool invertZoom;

        [Header("Developer Options")]
        public float rayMaxDistance = 4000f;
        [Tooltip("true: Use world space orientation; false: Use Controller.transform orientation.")]
        public bool useWorldOrientation = true;
        public bool fullScreenOnStart = true;
        protected virtual void Reset() {
            initialRadius = 10f;
            viewPanSpeed = 1f;
            viewZoomSpeed = 1f;
            viewOrbitSpeedHorizontal = 1f;
            viewOrbitSpeedVertical = 1f;
            invertPanX = false;
            invertPanY = false;
            invertOrbitH = false;
            invertOrbitV = false;
            invertZoom = false;
            rayMaxDistance = 4000f;
            useWorldOrientation = true;
            fullScreenOnStart = true;
        }
    }
}
