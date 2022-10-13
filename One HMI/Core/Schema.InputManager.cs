#if !ENABLE_INPUT_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sean21.OneHMI
{
    public partial class Schema
    {
        public InputManagerListener inputManagerListener;
        partial void im_OnEnable() {}
        
    }
}
#endif