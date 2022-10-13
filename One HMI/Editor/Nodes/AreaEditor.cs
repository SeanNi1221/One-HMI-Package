#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Sean21.OneHMI
{
    [CustomEditor(typeof(Area), true)]
    public class AreaEditor : NodeEditor
    {
        Area area;
        string header;
        [MenuItem("GameObject/One HMI/Make Area", false, 103)]
        private static void MakeArch() {
            foreach (var go in Selection.gameObjects) {
                MakeNodeOf(go, typeof(Area));
            }
        }
        [MenuItem("GameObject/One HMI/Create Area", false, 3)]
        private static void CreateArea() {
            CreateNodeOf<Area>("Area");
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            area = target as Area;
            header = area.isShell ? "Area (Shell)" : "Area";
            Modify("label").Disable().AddButtonR("Create", ()=>area.LoadLabel(true)).AddAbove(()=>{
                AddSpace();
                ButtonRow(new Button("Hide", area.Hide), new Button("Unhide", area.Unhide));
                HashSetField(area.contents, "Contents", true, () => {
                    GUILayout.FlexibleSpace();
                    ButtonRow(new Button("Hide Contents", area.HideContents), new Button("Unhide Contents", area.UnhideContents));
                });
                HashSetField(area.archs, "Archs");
                HashSetField(area.envelopes, "Envelopes", false, ()=>new Button("Update", area.SetModel3D).Draw());
            }).AddBelow( ()=>{
                if (Schema.current) {
                    Schema.current.DisplayAreaLayerFiled();
                }
            });
            Modify("autoSetLabelText").DrawAs(()=>{
                area.autoSetLabelText = EditorGUILayout.ToggleLeft("Auto Set Label Text (Editor Only)", area.autoSetLabelText);
            }).MoveBelow("label");
            Modify("displayName").OnValueChangeDelayCall(() => {
                if (area.autoSetLabelText) area.UpdateLabelText();
            });

            // Simplify("envelopes").AddAbove(() => {
            // // Disable("envelopes").AddAbove(() => {
            //     ButtonRow(new Button("Hide", area.Hide), new Button("Unhide", area.Unhide));
            //     HashSetField(area.contents, "Contents", true, () => {
            //         GUILayout.FlexibleSpace();
            //         ButtonRow(new Button("Hide Contents", area.HideContents), new Button("Unhide Contents", area.UnhideContents));
            //     });
            //     HashSetField(area.archs, "Archs");
            // });
            // Disable("areaTip");
            // CreateFoldoutGroup(header, "envelopes", "areaTip");
            CreateFoldoutGroup(header, "label", "autoSetLabelText");
        }        
    }
    public partial class Area
    {
        public bool autoSetLabelText = true;
        partial void ed_OnEnable() {
            PurgeArchs();
        }
        public override void SetModel3D() {
            base.SetModel3D();
            SetEnvelopes();
        }
    }

}
#endif