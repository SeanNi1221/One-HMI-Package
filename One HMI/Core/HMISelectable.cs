using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Sean21.OneHMI
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Node))]
    public class HMISelectable : MonoBehaviour {
        public bool enableIndexLine = true;
        [Tooltip("If this is not 0, native bounds size of Nodes will be ignored and use this as bounds' size instead.")]
        public float boundSizeOverriding;
        public UnityEvent onSelected;
        public UnityEvent onDeselected;
        public UnityEvent onSelectionRepeated;
        [SerializeField]
        [HideInInspector]
        private Node node;
        // private HMIHub hub => HMIHub.i;
        private Controller controller => HMIHub.i.controller;
        public bool isActive => controller.activeSelectable == this;

        public virtual Bounds viewFrameBounds => boundSizeOverriding == 0 ? 
            (node.bounds != null? node.bounds : new Bounds(transform.position, Vector3.one)) :
            new Bounds(transform.position, new Vector3(boundSizeOverriding, boundSizeOverriding, boundSizeOverriding));
        [HideInInspector]
        public Transform indexEnd;
        void OnEnable() {
            this.GetRef(ref node);
            node.selectable = this;
            SyncIndexEnd();
        }
        public void SyncIndexEnd() {
            if (enableIndexLine) {
                this.GetRefDown(ref indexEnd, "indexEnd");            
                if (indexEnd) return; 
                indexEnd = new GameObject("indexEnd").transform;
                indexEnd.SetParent(transform, false);
            }
            else {
                if (!indexEnd) return; 
                DestroyImmediate(indexEnd.gameObject);
            }
        }
        void OnDestroy() {
            if (indexEnd) DestroyImmediate (indexEnd.gameObject);
        }
        public void Select() {
            if (isActive) {
                Debug.Log("Selection Repeated!");
                onSelectionRepeated.Invoke();
            }
            else controller.ClearActive();
            controller.activeSelectable = this;
            onSelected.Invoke();
        }
        public void Deselect() {
            controller.activeSelectable = null;
            onDeselected.Invoke();
        }
        
    }
}
