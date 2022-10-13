#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Sean21.OneHMI
{
    [CustomEditor(typeof(Container), true)]
    public class ContainerEditor : NodeEditor
    {
        Container container;
        static bool drawSocketHandles = false;
        [MenuItem("GameObject/One HMI/Make Container", false, 102)]
        private static void MakeNode(){
            foreach (var go in Selection.gameObjects) {
                MakeNodeOf(go, typeof(Container));
            }
        }
        [MenuItem("GameObject/One HMI/Create Container", false, 2)]
        private static void CreateContainer() {
            CreateNodeOf<Container>("Container");
        }
        protected override void OnEnable() {
            base.OnEnable();
            container = target as Container;
            Modify("bindContents").AddBelow(()=>{
                HashSetField(container.contents, "Contents");
                ButtonRow(
                    new Button("Mount Contents", container.MountContents),
                    new Button("Mount Recursive", ()=>container.MountContents(true))
                );
                ButtonRow(
                    new Button("Unmount Contents", container.UnmountContents),
                    new Button("Umnount Recursive", ()=>container.UnmountContents(true))
                );
                SchemaEditor.DisplayDrawContainerSocketsField();
                EditorGUI.BeginChangeCheck();
                drawSocketHandles = EditorGUILayout.ToggleLeft("Draw Socket Handles", drawSocketHandles);
                if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
            });
            CreateFoldoutGroup("Container", "autoMount", "autoMount");
        }
        protected override void OnSceneGUI() {
            base.OnSceneGUI();
            if (drawSocketHandles) {
                foreach (var content in container.contents) {
                    content.SocketHandle();
                }
            }
        }
    }
    public partial class Container
    {
        // protected virtual void OnDrawGizmosSelected() {
        //     foreach(var content in contents) {
        //         content.SocketHandle();
        //     }
        // }
    }
}
#endif