#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace Sean21.OneHMI
{
    using static EditorGenerics;
    using static Generics;
    [CustomEditor(typeof(Node), true)]
    [CanEditMultipleObjects]
    public class NodeEditor : OneHMIEditor
    {
        Node node;
        [MenuItem("GameObject/One HMI/Make Node", true)]
        [MenuItem("GameObject/One HMI/Make Container", true)]
        [MenuItem("GameObject/One HMI/Make Area", true)]
        [MenuItem("GameObject/One HMI/Make Arch", true)]
        protected static bool CanExecuteMake() {
            return Selection.gameObjects.Length > 0;
        }
        [MenuItem("GameObject/One HMI/Make Node", false, 101)]
        private static void MakeNode(){
            foreach (var go in Selection.gameObjects) {
                MakeNodeOf(go, typeof(Node));
            }
        }
        [MenuItem("GameObject/One HMI/Create Node", false, 1)]
        private static void CreateNode() {
            CreateNodeOf<Node>("Node");
        }
        /// <summary>
        /// Create Object of type <typeparamref name="T"/> with a unique Id and GameObject name.
        /// </summary>
        /// <param name="prefix">GameObject name or Id prefix</param>
        public static void CreateNodeOf<T>(string prefix) where T: Node {
            int i = 1;
            string goName;
            begin_searching:
            do {
                goName = prefix + " " + i;
                i++;
            }
            while (Node.Manifest.TryGetValue(goName, out var existing));
            if (GameObject.Find(goName)) {
                goto begin_searching;
            }
            GameObject go = new GameObject(goName);
            var active = Selection.activeGameObject;
            if (active) go.transform.SetParent(active.transform);
            go.AddComponent<T>();
            // EditorGUIUtility.PingObject(go);
            Selection.activeGameObject = go;
        }

        /// <returns>true:Succeeded, false:Failed</returns>
        public static bool MakeNodeOf(GameObject go, Type targetType) {
            var _node = go.GetComponent<Node>();
            if (_node) {
                var existingType = _node.GetType();
                if (existingType != targetType) {
                    Debug.LogError($"'{go.name}': Making {targetType.Name} failed! Reason: A component of {existingType.Name} already exists, cannot add {targetType.Name}");
                    return false;
                }
                return true;
            } 
            go.AddComponent(targetType);
            return true;
        }
        protected virtual void OnEnable() {
            node = target as Node;
            Modify("id").DrawAs(() => {
                if (node.autoSetId) {
                    HorizontalSpan(()=>{
                        EditorGUILayout.LabelField("Id", EditorStyles.label, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(EditorGUIUtility.labelWidth-16));
                        GUIStyle idStyle = new GUIStyle(EditorStyles.label);
                        idStyle.fontSize = 16;
                        EditorGUILayout.SelectableLabel(node.Id, idStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    });
                }
                else EditorGUILayout.PropertyField(currentProp);
            }).OnValueChangeAndDelayCall(node.Deregister, node.Register);
            Indent("autoSetId", 1);
            
            Disable("_parent", "_parentId", "_selectable", "_hierarchyItem", "spawnPoint", "containerId",
                "_latestArea", "latestAreaId", "iconPath", "model3dPath", "indicatorPath", "isInContainer"
            );
            Modify("_parentId").AddBelow(() => HashSetField(node.children, "Children", false));
            Modify("_container").OnValueChangeAndDelayCall(node.DeregisterFromContainer, node.UpdateContainerId, () => node.RegisterToContainer());
            Modify("socketRotation").AddBelow(()=>{
                AddSpace();
                ButtonRow(
                    new Button("Socket to Container", () => { 
                        Undo.RecordObject(node, $"Set node '{node.Id}' socket to container");
                        node.SetSocketWorldSpace(node.container.transform);
                        SceneView.RepaintAll();
                    }),
                    new Button("Socket to Node", () => {
                        Undo.RecordObject(node, $"Set node '{node.Id}' socket to self");
                        node.SetSocketWorldSpace(node.transform);
                        SceneView.RepaintAll();
                    })
                );
                ButtonRow(
                    new Button("Mount", node.MountToContainer),
                    new Button("Unmount", node.UnmountFromContainer)
                );
                SchemaEditor.DisplayDrawContainerSocketsField();

            });
            Modify(field=>field.Hide(()=>node.container == null), "socketPosition", "socketRotation");
            Modify("model3d").OnValueChangeDelayCall(node.SetModel3DPath);
            Modify("indicator").OnValueChangeDelayCall(node.SetIndicatorPath);
            Modify("hasImage").AddBelow(()=>{
                if (node.hasImage) {
                    BeginIndent();
                    EditorGUILayout.LabelField("Default Image Path:");
                    BeginIndent();
                    EditorGUILayout.SelectableLabel("StreamingAssets/" + node.StreamingImagePath1, EditorStyles.label, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    EditorGUILayout.SelectableLabel("StreamingAssets/" + node.StreamingImagePath2, EditorStyles.label, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    EndIndent();
                    EditorGUILayout.LabelField("Overriding (Optional):");
                    EndIndent();
                }
            });
            Modify(() => { 
                if(node.hasImage) IndentedField(currentProp, 2);
            }, "image", "overridingImagePath");

            Modify("hasDescription").AddBelow(()=>{
                if (node.hasDescription) {
                    BeginIndent();
                    EditorGUILayout.LabelField("Default Description Path:");
                    BeginIndent();
                    EditorGUILayout.SelectableLabel("StreamingAssets/" + node.StreamingDescPath, EditorStyles.label, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    EndIndent();
                    EditorGUILayout.LabelField("Overriding (Optional):");
                    EndIndent();
                }
            });
            Modify(() => { 
                if(node.hasDescription) IndentedField(currentProp, 2);
            }, "description", "overridingDescriptionPath");

            // Modify("icon").OnValueChangeDelayCall(node.SetIconPath);
            // Modify("model3d").OnValueChangeDelayCall(node.SetModel3DPath).AddButtonR("Set", node.SetModel3D);
            // Modify("indicator").OnValueChangeDelayCall(node.SetIndicatorPath);

            CreateFoldoutGroup("Base Info", "id", "isVirtual");
            CreateFoldoutGroup("Location and Rotation", "spawnPoint", "socketRotation");
            CreateFoldoutGroup("Resources", "model3d", "overridingDescriptionPath");
            CreateFoldoutGroup("Events", "onHide", "onTransformChangingStopped", false);
        }
        protected virtual void OnSceneGUI() {
            node.SocketHandle();
        }
    } 
    public partial class Node
    {
        #region Messages
        partial void ed_Awake() {
            // AutoSetIdIfNeeded();
        }
        partial void ed_OnEnable(){
            SetIcon();
            EditorApplication.hierarchyChanged += AutoSetIdIfNeeded;
            EditorApplication.hierarchyChanged += UpdateContainerID;
            EditorApplication.hierarchyChanged += UpdateLatestAreaId;
        }
        protected virtual void OnValidate() {
            // AutoSetIdIfNeeded();
            SetIconPath();
            SetSpawnPoint();
            SetModel3D();
            SetIndicatorPath();
            UpdateContainerID();
        }
        partial void ed_OnDisable() {
            EditorApplication.hierarchyChanged -= AutoSetIdIfNeeded;
            EditorApplication.hierarchyChanged -= UpdateContainerID;
            EditorApplication.hierarchyChanged -= UpdateContainerID;
        }
#endregion
#region Resources Operations
        public virtual void SetIcon() {
            if (isLoadingIcon) return;
            if (!icon) icon = LoadAsset<Sprite>(DefaultNodeIconFolderPath);
            SetIconPath();
        }
        public virtual void SetIconPath() {
            GetAddressablePath(icon, out iconPath);
        }
        public virtual void SetModel3D() {
            if (isLoadingModel3D || Application.isPlaying)
                return;
            meshRenderers = this.GetComponentsInChildrenFiltered<MeshRenderer>(child => !child.GetComponentInChildren<Node>());            
            if (meshRenderers != null && meshRenderers.Length > 0) {
                model3d = meshRenderers[0].gameObject;
                colliders = model3d.GetComponentsInChildren<MeshCollider>();
                goto end;
            }
            colliders = this.GetComponentsInChildrenFiltered<Collider>(child => !child.GetComponentInChildren<Node>());            
            if (colliders !=null && colliders.Length > 0) {
                model3d = colliders[0].gameObject;
                goto end;
            }
            model3d = null;
        end:
            SetBounds();
            SetModel3DPath();
            return;
        }
        public virtual void SetModel3DPath() {
            if (isLoadingModel3D) return;
            if (!model3d) {model3dPath = string.Empty; return;}
            GetAddressablePath(model3d, out model3dPath);
        }
        public void SetIndicatorPath() {
            if(isLoadingIndicator) return;
            GetAddressablePath(indicator, out indicatorPath);
        }
#endregion
        ///<summary>Get Component from the current transform and is direct children.</summary>
        protected T GetComponentInBelongings<T>() where T : Component {
            var component = GetComponent<T>();
            if (component) return component;
            for (int i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                if (child.GetComponent<Node>()) continue;
                component = child.GetComponent<T>();
                if (component) return component;
            }
            return null;
        }
        protected virtual void OnDrawGizmos() {
            if (container && schema && schema.drawContainerSockets) {
                if (IsInContainer) {
                    Handles.color = Color.cyan;
                    if (container.bindContents)
                        Handles.color = Color.green;
                }
                Handles.DrawDottedLine( container.transform.position, socketPositionWorldSpace, 2);
                Handles.DrawDottedLine( transform.position, socketPositionWorldSpace, 2);
                Gizmos.DrawIcon(socketPositionWorldSpace, null, false);
            }
        }
        internal virtual void SocketHandle() {
            if (container && schema && schema.drawContainerSockets) {
                Handles.Label(socketPositionWorldSpace, $"{id} Socket");
                EditorGUI.BeginChangeCheck();
                Vector3 newSocketPos = Handles.PositionHandle(socketPositionWorldSpace, socketRotationWorldSpace);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(this, "Change Socket Position");
                    socketPositionWorldSpace = newSocketPos;
                    this.Update();
                }
                EditorGUI.BeginChangeCheck();
                Quaternion newSocketRot = Handles.RotationHandle(socketRotationWorldSpace, socketPositionWorldSpace);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(this, "Change Socket Rotation");
                    socketRotationWorldSpace = newSocketRot;
                    this.Update();
                }
            }
        }
    }
}
#endif