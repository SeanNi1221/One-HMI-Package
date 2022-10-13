using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sean21.OneHMI
{
    using static Generics;
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public partial class Theme : MonoBehaviour
    {
        public Canvas canvas;
        public List<Widget> widgets = new List<Widget>();
        partial void ed_Awake();
        void Awake() {
            this.GetRef(ref canvas);
            ed_Awake();
        }
        partial void ed_OnEnable();
        void OnEnable(){
            ed_OnEnable();
        }
        void OnTransformChildrenChanged()
        {
            Debug.Log("Children changed...");
            PurgeWidges();
        }
        void PurgeWidges() {
            Debug.Log("Purging...");
            if (widgets.Count > 0) {
                widgets.RemoveAll(widget => widget == null);
                widgets.RemoveAll(widget => widget.theme != this);
            }
        }

    }
}
