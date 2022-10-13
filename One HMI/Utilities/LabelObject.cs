using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if USE_TMP
using TMPro;
#endif
namespace Sean21.OneHMI
{
    using static Generics;
    [ExecuteInEditMode]
    public class LabelObject : MonoBehaviour {
#if USE_TMP
        public bool useTMP = true;
#endif
        public float minSize = 0.025f;
        public float maxSize = 0.06f;
        public float currentSize { get; private set; }
        // private float frameSize{ get; set; }
        private const float multiplier = 10;
        // private const float multiplier2dMode = 1000;
        public string text{ get { ValidateGeneralText(); return generalText.text; } set { ValidateGeneralText(); generalText.text = value; } }
        [SerializeField]
        internal GeneralText generalText = new GeneralText();
        public RectTransform RectTr{get { this.ValidateComponent(ref rectTransform); return rectTransform; } }
        [SerializeField]
        private RectTransform rectTransform;
        [SerializeField]
        private Canvas canvas;
        private Camera cam => HMIHub.i ? HMIHub.i.cam : null;
        protected virtual void Awake() {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        protected virtual void OnEnable() {
            // Debug.Log("LabelObject OnEnable Executed...");
            this.ValidateComponent(ref rectTransform);
            transform.localPosition = Vector3.zero;
            ValidateCanvas();
            ValidateGeneralText();
            UpdateObjectState();
        }
        void Update() {
            if (Application.isPlaying && cam && cam.enabled == true && transform.position.IsInCameraView(cam) && !generalText.isNull) {
                UpdateObjectState();
            }
        }
        void UpdateObjectState() {
            rectTransform.LookAt(cam.transform, cam.transform.up);
            var frameSize = Mathf.Tan( Mathf.Deg2Rad * (cam.fieldOfView/2f) ) * Vector3.Distance(rectTransform.position, cam.transform.position);
            // Debug.Log($"Tan(30):{Mathf.Tan(30f)}, Tan(20):{Mathf.Tan(20f)}");
            // Debug.Log($"FoV:{cam.fieldOfView}");
            // Debug.Log($"Tan:{Mathf.Tan( Mathf.Deg2Rad * (cam.fieldOfView/2f) )}, Dis:{Vector3.Distance(rectTransform.position, cam.transform.position)}");
            // Debug.Log($"Frame Size:{frameSize}");
            //Add Mode2D Compatibility here
            currentSize = rectTransform.localScale.y / frameSize * multiplier;
            // Debug.Log($"current Size:{currentSize}");
            if (currentSize > maxSize) {
                rectTransform.localScale *= maxSize / currentSize;
            }
            if (currentSize < minSize) {
                rectTransform.localScale *= minSize / currentSize;
            }
        }
        internal void ValidateGeneralText() {
            if (!generalText.isNull) return;
            // Debug.Log("Preparing to create...");
#if USE_TMP
            if (useTMP) {
                generalText = GetComponent<TextMeshProUGUI>()?? gameObject.AddComponent<TextMeshProUGUI>();
            } else {
                generalText = GetComponent<Text>()?? gameObject.AddComponent<Text>();
            }
#else
            generalText = GetComponent<Text>();
            if (!generalText) generalText = gameObject.AddComponent<Text>();
#endif
            generalText.color = HMIHub.i.schema.labelColor;
            generalText.fontSize = HMIHub.i.schema.labelfontSize;
        }
        protected virtual void ValidateCanvas() {
            if (canvas) goto set_mode;
            canvas = GetComponentInParent<Canvas>();
            if (!canvas) canvas = rectTransform.root.gameObject.AddComponent<Canvas>();
            set_mode:
            canvas.renderMode = RenderMode.WorldSpace;
        }
    }
}
