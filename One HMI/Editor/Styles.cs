#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sean21.OneHMI
{
    internal static class Styles
    {
        internal static class EditorColor {
            internal static Color32 Major = new Color32( 42, 131, 130, 255 );
            internal static Color Minor = new Color(0.2f, 0.226f, 0.267f, 1f);
        }
    }
}
#endif