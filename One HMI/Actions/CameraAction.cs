using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Sean21.OneHMI
{
    using static Generics;
    [ExecuteInEditMode]
    [RequireComponent(typeof(HMICamera))]
    public class CameraBehaviour : HMIAction
    {
        
        // [SerializeField] private HMICamera _hmiCam;
        // private HMICamera hmiCam => this.SerializableGetComponent(ref _hmiCam);
        // public Camera cam => hmiCam.cam;
    }
}
