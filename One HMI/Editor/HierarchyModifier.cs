#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
namespace Sean21.OneHMI
{   
    using static EditorGenerics;
    [InitializeOnLoad]
    static class HierarchyModifier
    {   
        static readonly Type[] marked = new Type[]{
            typeof(HMIHub),
            typeof(Controller),
            typeof(HMICamera),
            typeof(Theme),
            typeof(Widget),
            typeof(Node),
            // typeof(Container),
            // typeof(Arch),
            // typeof(Area)
        };
        static readonly Texture archShellTexture = LoadAsset<Texture>(EditorIconFolder + "areaShell.png");
        private const int IconSize = 12;
        private const int IconSpacing = 6;
        private const int RightPadding = 12;
        static HierarchyModifier() {
            EditorApplication.hierarchyWindowItemOnGUI += ItemIconHandler;
        }
        static void ItemIconHandler(int instanceID, Rect selectionRect) {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (!go) return;

            int iconCount = 0;
            foreach (var type in marked) {
                var obj = go.GetComponent(type);
                if (obj) {
                    Texture texture;
                    Area area = obj as Area;
                    if (area && area.isShell) {
                        texture = archShellTexture;
                    } else { texture = EditorGUIUtility.ObjectContent(obj, type).image; }

                    GUI.DrawTexture(
                        new Rect(selectionRect.xMax - RightPadding - iconCount * (IconSize + IconSpacing),
                            selectionRect.yMin,
                            IconSize,
                            IconSize
                        ),
                        texture
                    );
                    iconCount++;                    
                }
            }
        }
    }
}
#endif