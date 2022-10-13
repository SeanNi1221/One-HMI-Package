#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
namespace Sean21.OneHMI
{
    using static EditorGenerics;
    [CustomEditor(typeof(Arch), true)]
    public class ArchEditor : NodeEditor
    {
        Arch arch;
        [MenuItem("GameObject/One HMI/Make Arch", false, 104)]
        private static void MakeArch() {
            foreach (var go in Selection.gameObjects) {
                if (MakeNodeOf(go, typeof(Arch))) {
                    var renderer = go.GetComponentInDirectChildren<MeshRenderer>(go => !go.GetComponent<Node>());
                    if (!renderer) {
                        Debug.LogWarning($"'{go}': Arch Component needs MeshRenderer to function properly!");
                        continue;
                    }
                }
            }
        }
        [MenuItem("GameObject/One HMI/Create Arch", false, 4)]
        private static void CreateArch() {
            CreateNodeOf<Arch>("Arch");
        }
        protected override void OnEnable() {
            base.OnEnable();
            arch = target as Arch;
            //***************Need to check again
            Modify("highlighting").OnValueChangeDelayCall(arch.SetHighlightingPath);
            Modify("area0").OnValueChangeAndDelayCall(arch.DeregisterFromArea0,
                arch.RegisterToArea0, arch.UpdateArea0ID).
                AddAbove(() => {
                    AddSpace();
                    ButtonRow(new Button("Reset", () => arch.ResetAreas()), new Button("Clear", arch.ClearAreas));
                }
            );
            Modify("area1").OnValueChangeAndDelayCall(arch.DeregisterFromArea1,
                arch.RegisterToArea1, arch.UpdateArea1ID);
            Modify("area2").OnValueChangeAndDelayCall(arch.DeregisterFromArea2,
                arch.RegisterToArea2, arch.UpdateArea2ID);
            Modify("area3").OnValueChangeAndDelayCall(arch.DeregisterFromArea3,
                arch.RegisterToArea3, arch.UpdateArea3ID);

            Modify(field => field.Disable().Indent(), "area0Id", "area1Id", "area2Id", "area3Id");

            CreateFoldoutGroup("Arch", "highlighting", "area3Id");
        }
    }
    public partial class Arch
    {
        partial void ed_OnEnable() {
            EditorApplication.hierarchyChanged += UpdateAreasId;
        }
        partial void ed_OnDisable() {
            EditorApplication.hierarchyChanged -= UpdateAreasId;
        }
        public void SetHighlightingPath() {
            GetAddressablePath(highlighting, out highlightingPath);
        }
        protected virtual void Reset() {
            showInHierarchy = false;
        }
    }
}
#endif