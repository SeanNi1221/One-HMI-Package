using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
namespace Sean21.OneHMI
{
    using static Generics;
    [RequireComponent(typeof(Selectable))]
    [ExecuteAlways, RequireComponent(typeof(Selectable))]
    public partial class Widget : MonoBehaviour
    {
        // [SerializeField] private HMIHub _hub;
        // public HMIHub hub {
        //     get => _hub;
        //     set => _hub = value;
        // }
        private HMIHub hub => HMIHub.i;
        [SerializeField] private Theme _theme;
        public Theme theme {
            get => _theme;
            private set => _theme = value;
        }
        // public Selectable element;
        // [SerializeField] private Selectable _element;
        // public Selectable element => _element;
        // public Button button => element as Button;
        // public Toggle toggle => element as Toggle;
        [SerializeField]
        private Button _button;
        public Button button => _button;
        [SerializeField]
        private Toggle _toggle;
        public Toggle toggle => _toggle;
        public HMIAction action;
        // [SerializeField] private HMIAction _action;
        // public HMIAction action {
        //     get => _action;
        //     set {
        //         _action = value;
        //         _action.widgets.Add(this);
        //     }
        // }
        partial void ed_Awake();
        void Awake(){
            ed_Awake();
        }
        partial void ed_OnEnable();
        void OnEnable() {
            ed_OnEnable();
            if (Application.isPlaying && action) {
                // action.hub = hub;
                action.widgets.Add(this);
                if (button) button.onClick.AddListener(action.Perform);
                if (toggle) toggle.onValueChanged.AddListener(action.PerformBool);
            }
        }
        void OnDisable() {
            if (Application.isPlaying && action) {
                action.widgets.Remove(this);
                if (button) button.onClick.RemoveListener(action.Perform);
                if (toggle) toggle.onValueChanged.RemoveListener(action.PerformBool);
            }
        }
        void OnDestroy() {
            if (theme) theme.widgets.Remove(this);
        }

    }
}
