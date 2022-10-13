#if ENABLE_INPUT_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Sean21.OneHMI
{
    using static Generics;
    public partial class Schema
    {
        [SerializeField]private InputActionAsset _inputActionAsset;
        public InputActionAsset inputActionAsset{
            get => _inputActionAsset;
            set => _inputActionAsset = value;
        }
    }
}
#endif