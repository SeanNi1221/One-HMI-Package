using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sean21.OneHMI
{
    public class ScreenIndexLine : MonoBehaviour
    {
        // [SerializeField]private UserControl userControl;
        [SerializeField]
        [HideInInspector] 
        // private HMIHub hub;
        public Transform startPoint;
        public Transform endPoint;
        public float start_width = 0.5f;
        public float end_width = 0f;
        public Material material; 

        Vector3 endPosLocal;
        LineRenderer r;
        void Awake()
        {
            // this.GetRef(ref hub, HMIHub.i);
            this.enabled = false;
        }
        void OnEnable()
        {
            // UserControl.OnTransitionStarted += AdaptWidth;
            // UserControl.OnTransitionFinished += AdaptWidth;
            if (r == null)
            {
                r = this.GetComponent<LineRenderer>();
            }
            r.enabled =true;
            // Debug.Log(r.name + " line enabled!");
            r.SetPosition(0, transform.InverseTransformPoint(startPoint.position)); 
            // AdaptWidth();
            r.endWidth = end_width;
            r.material = material;       
        }
        void OnDisable()
        {
            // UserControl.OnTransitionStarted -= AdaptWidth;
            // UserControl.OnTransitionFinished -= AdaptWidth;
            if( r != null)
            {
                r.enabled = false;
                // Debug.Log(r.name + " line DISABLED!");
            }
        }
        void Update()
        {
            if (endPoint) {
                endPosLocal = transform.InverseTransformPoint(endPoint.position);
                if (r != null)
                {
                    r.SetPosition(1,endPosLocal);        
                }
            }
        }
        // void AdaptWidth()
        // {    
        //     if (userControl.mode2D.isOn && !userControl.firstPerson.isOn)
        //     {
        //         r.startWidth = start_width/UserControl.transitionRatio;
        //         if (UserControl.isInViewTransition) r.enabled = false;
        //         else r.enabled = true;
        //     } 
        //     else r.startWidth = start_width;
        // }
    }
}
