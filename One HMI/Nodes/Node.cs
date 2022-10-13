using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;

namespace Sean21.OneHMI
{
    using static Generics;
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public partial class Node : MonoBehaviour {
        public static Dictionary<string, Node> Manifest = new Dictionary<string, Node>();
        ///<summary><para><paramref name="TKey"/>:id, <paramref name="TValue"/>:chidrenBuffer</para>
        ///Cache already instantiated children when the parent is not yet instantiated.</summary>
        public static readonly Dictionary<string, HashSet<Node>> Buffer = new Dictionary<string, HashSet<Node>>();
        public string Id => id;
        [SerializeField]
        private string id;
        public bool autoSetId = true;
        public bool showInHierarchy = true;
        public Node parent => _parent;
        [SerializeField]
        private Node _parent;
        [SerializeField]
        private string _parentId;
        public readonly HashSet<Node> children = new HashSet<Node>();
        public int depth => GetComponentsInParent<Node>().Length;
        public HierarchyItem hierarchyItem {
            get => _hierarchyItem;
            internal set { _hierarchyItem = value; }
        }
        [SerializeField]
        private HierarchyItem _hierarchyItem;
        public HMISelectable selectable {
            get => _selectable;
            internal set { _selectable = value; }
        }
        [SerializeField]
        private HMISelectable _selectable;
        public string displayName = "Untitled";
        public bool isVisible = true;

        [Tooltip("If this Node can interact with real world devices.")]
        public bool isVirtual = true;
        //mesh_renderer
        // public MeshRenderer nodeRenderer { get; private set; }
        public MeshRenderer[] meshRenderers { get; private set; }
        public Collider[] colliders { get; private set; }
        // public Collider nodeCollider { get; private set; }
        public Transform spawnPoint;
        public Vector3 outsideContainerPosition { get; private set; }
        public Quaternion outsideContainerRotation { get; private set; }
        /// <summary>The Area this Node is locating at.</summary>
        /// <remarks>Note: Use this property cautiously, for it will iterate through all <see cref="Area"/>s 
        /// and update the value of <see cref="latestArea"/> and relavent <see cref="Area"/>s' <see cref="Area.contents"/>.
        /// If this is not required, use <see cref="latestArea"/> instead.
        /// </remarks>
        public virtual Area area {
            get {
                Area currentArea = null;
                int depth = 0;
                //From all areas enclosing this Node, choose the one with the deepest hierarchy level.
                foreach (var possibleArea in Area.Manifest.Values) {
                    if (!possibleArea.Contains(this)) continue;
                    int currentDepth = possibleArea.transform.hierarchyCount;
                    if (currentDepth <= depth) continue;
                    depth = currentDepth;
                    currentArea = possibleArea;
                }
                if (_latestArea != currentArea) {
                    //Deregister from old area.
                    if (_latestArea) _latestArea.contents.Remove(this);
                    //Register to new area.
                    if (currentArea) currentArea.contents.Add(this);

                }
                _latestArea = currentArea;
                return currentArea;
            }
        }
        public Area latestArea => _latestArea;
        [SerializeField]
        private Area _latestArea;
        [SerializeField]
        private string latestAreaId;
        public Container container {
            get => _container;
            set {
                DeregisterFromContainer();
                _container = value;
                UpdateContainerId();
                RegisterToContainer();
            }
        }
        [SerializeField]
        private Container _container;
        [SerializeField]
        private string containerId;
        public bool IsInContainer => isInContainer;
        [SerializeField]
        private bool isInContainer;
        public Vector3 socketPositionWorldSpace{
            get => container ? container.transform.position + socketPosition : socketPosition;
            set => socketPosition = container ? value - container.transform.position : value;
        }
        public Vector3 socketPosition;
        public Quaternion socketRotationWorldSpace{
            get => container ? container.transform.rotation * socketRotation : socketRotation;
            set => socketRotation = container ? Quaternion.Inverse(container.transform.rotation) * value : value;
        }
        public Quaternion socketRotation;
        public GameObject model3d;
        public string model3dPath;
        public Bounds bounds{ get; private set; }
        //volume
        public GameObject indicator;
        //volumePath
        public string indicatorPath;
        public Sprite icon;
        public string iconPath;
        public bool hasImage;
        public Texture2D image;
        public string overridingImagePath;
        /// <summary>
        /// Default png path
        /// </summary>
        public virtual string StreamingImagePath1 => (schema ? schema.streamingImageFolder : "NodeImages/") + (schema ? schema.NodeResourceFileName(this) : displayName) + ".png";
        /// <summary>
        /// Default jpg path
        /// </summary>
        public virtual string StreamingImagePath2 => (schema ? schema.streamingImageFolder : "NodeImages/") + (schema ? schema.NodeResourceFileName(this) : displayName) + ".jpg";
        public bool hasDescription;
        //intro
        public string description;
        //customIntroPath;
        internal virtual string StreamingDescPath => (schema ? schema.streamingDescFolder : "NodeDescriptions/") + (schema ? schema.NodeResourceFileName(this) : displayName) + "txt";
        public string overridingDescriptionPath;

        public UnityEvent onHide = new UnityEvent();
        public UnityEvent onUnhide = new UnityEvent();
        public UnityEvent onLoadResourcesFinished = new UnityEvent();
        public UnityEvent onTransformChange = new UnityEvent();
        public UnityEvent onTransformChangingStopped = new UnityEvent();

        private HMIHub hub => HMIHub.i;
        private Schema schema => hub? hub.schema : null;
        private Controller controller => hub? hub.controller : null;
        public bool isLoadingIcon { get; private set; }
        public bool isLoadingDescription { get; private set; }
        public bool isLoadingImage { get; private set; }
        public bool isLoadingModel3D { get; private set; }
        public bool isLoadingIndicator { get; private set; }
        public bool isTransformChanging { get; private set; }
        #region Messages
        partial void ed_Awake();
        protected virtual void Awake() {
            ed_Awake();
        }
        partial void ed_OnEnable();
        protected virtual void OnEnable() {
            if (string.IsNullOrEmpty(Id)) {
                OverwriteIdWithDefault();
            } else Register();
            UpdateParent();
            if (Application.isPlaying) StartCoroutine(LoadResources());
            // LoadArea();
            RegisterToLatestArea();
            RegisterToContainer();
            ed_OnEnable();
            onTransformChangingStopped.AddListener(UpdateLatestArea);
        }
        protected virtual void Update() {
            if (transform.hasChanged) {
                isTransformChanging = true;
                onTransformChange.Invoke();
                transform.hasChanged = false;
            } else {
                //Value of the previous frame
                if (isTransformChanging) {
                    onTransformChangingStopped.Invoke();
                }
                isTransformChanging = false;
            }
        }
        partial void ed_OnDisable();
        protected virtual void OnDisable() {
            Deregister();
            DeregisterFromLatestArea();
            ed_OnDisable();
            onTransformChangingStopped.RemoveAllListeners();
        }
        protected virtual void OnDestroy() {
            DeregisterFromContainer();
            DeregisterFromParent();
        }
        protected virtual void OnTransformParentChanged() {
            UpdateParent();
        }
        protected virtual void OnTransformChildrenChanged() {
            // PurgeChildren();
        }
        #endregion
        #region Basic Operations
        internal virtual void Register() {
            if (string.IsNullOrEmpty(id)) Debug.LogError("ID is null!");
            if (!Manifest.TryAdd(id, this)) {
                if (Manifest[id] != this) {
                    Debug.LogWarning($"Node {id} registering failed: There is another Node with the same ID!");
                }
            }
        }
        internal virtual void Deregister() {
            if (!Manifest.Remove(id)) { 
            }
        }
        protected virtual void AutoSetIdIfNeeded() {
            if (autoSetId) {
                OverwriteIdWithDefault();
            }
        }
        protected virtual void OverwriteIdWithDefault() {
            if(!string.IsNullOrEmpty(id)){
                Deregister();
            }
            gameObject.name.IsSQLFriendly(out id);
            Register();
        }
        protected virtual void UpdateParent() {
            var oldParent = _parent;
            Action processOld = () => {
                if (oldParent && oldParent != _parent)
                    oldParent.children.Remove(this);
            };
            if (!transform.parent) goto clear;
            _parent = transform.parent.GetComponent<Node>();
            if (!_parent) goto clear;
            _parentId = _parent.id;
            _parent.children.Add(this);
            processOld();
            return;
            clear:
            _parent = null;
            _parentId = string.Empty;
            processOld();
        }
        protected virtual void DeregisterFromParent() {
            if (_parent) _parent.children.Remove(this);
        }
        protected virtual void PurgeChildren() {
            children.RemoveWhere(child => child._parent != this || child == null);
        }
        public virtual void Hide() {
            HideIndicator();
            isVisible = false;
            if (meshRenderers != null) {
                foreach (var renderer in meshRenderers) {
                    renderer.enabled = false;
                }
            }
            if (colliders != null) {
                foreach (var collider in colliders) {
                    collider.enabled = false;
                }
            }
            onHide.Invoke();
            foreach (var child in children) child.Hide();
        }
        public virtual void Unhide() {
            ShowIndicator();
            isVisible = true;
            if (meshRenderers != null) {
                foreach (var renderer in meshRenderers) {
                    renderer.enabled = true;
                }
            }
            if (colliders != null) {
                foreach (var collider in colliders) {
                    collider.enabled = true;
                }
            }
            onUnhide.Invoke();
            foreach (var child in children) child.Unhide();
        }
        public virtual void HighlightingOn() { }
        public virtual void HighlightingOff() { }
        public virtual void ShowIndicator() {
            if (indicator) indicator.SetActive(true);
        }
        public virtual void HideIndicator() {
            if (indicator) indicator.SetActive(false);
        }
        #endregion
        #region Spacial Operations
        protected virtual void SetSpawnPoint() {
            spawnPoint = transform;
        }
        private void UpdateLatestAreaId() {
            if (!_latestArea) {
                latestAreaId = null;
            } else {
                latestAreaId = _latestArea.id;
            }
        }
        public void UpdateLatestArea() {
            _latestArea = area;
            UpdateLatestAreaId();
        }

        ///<returns><c>true</c>:Register succeeded. <c>false</c>:Register failed or this node was added to Buffer</returns>
        public virtual bool RegisterToLatestArea() {
            if (string.IsNullOrEmpty(latestAreaId)) return false;
            //Add to contents if latestArea is already referenced or latestArea is already in Manifest.
            if (_latestArea || (Area.Manifest.TryGetValue(latestAreaId, out _latestArea) && _latestArea)) {
                _latestArea.contents.Add(this);
                return true;
            }
            //latestArea is already in Buffer
            if (Area.Buffer.TryGetValue(latestAreaId, out var contentsBuffer)) {
                contentsBuffer.Add(this);
            } else {//latestArea not in Buffer
                contentsBuffer = Area.Buffer[latestAreaId] = new HashSet<Node> { this };
            }
            // Debug.Log($"node {displayName}({id}) is added to Area Buffer");
            return false;            
        }
        protected virtual void DeregisterFromLatestArea() {
            if (_latestArea) _latestArea.contents.Remove(this);
            _latestArea = null;
        }
        public void UpdateContainerID() {
            if (!_container) {containerId = string.Empty; return;}
            containerId = _container.id;
        }
        ///<summary>Store the relative position and the relative rotation from <see cref="container.transform"/> to <paramref name="worldSpaceTransform"/>.</summary>
        public void SetSocketWorldSpace(Transform worldSpaceTransform) {
            if (!container) {
                Debug.LogWarning($"{id}: Container is null, SetSocketWorldSpace may not work as expected");
            }
            socketPositionWorldSpace = worldSpaceTransform.position;
            socketRotationWorldSpace = worldSpaceTransform.rotation;
        }

        internal void UpdateContainerId() {
            if (!_container) {
                containerId = null;
            } else {
                containerId = _container.id;
            }
        }
        ///<returns><c>true</c>:Register succeeded. <c>false</c>:Register failed or this node is added into Buffer</returns>
        public virtual bool RegisterToContainer() {
            if (string.IsNullOrEmpty(containerId)) return false;
            //Add to contents if container is already referenced or container is already in Manifest.
            if (_container || ( Container.Manifest.TryGetValue(containerId, out _container) && _container)) {
                _container.contents.Add(this);
                if (_container.autoMount) MountToContainer();
                return true;
            }
            //container is already in Buffer
            if (Container.Buffer.TryGetValue(containerId, out var contentsBuffer)) {
                contentsBuffer.Add(this);
            } else {//container not in Buffer
                contentsBuffer = Container.Buffer[containerId] = new HashSet<Node> { this };
            }
            Debug.Log($"node {displayName}({id}) is added to Container Buffer");
            return false;
        }
        public virtual void DeregisterFromContainer() {
            if (_container) {
                UnmountFromContainer();
                _container.contents.Remove(this); 
            }
            _container = null;
        }
        public virtual void MountToContainer() {
            if (!_container ) return;
            if (!isInContainer) {
                outsideContainerPosition = transform.position;
                outsideContainerRotation = transform.rotation;
            }
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(transform, $"Mount node '{Id}' to container");
            UnityEditor.Undo.RecordObject(this, $"Set node '{Id}' IsInContainer to true");
#endif
            MoveToSocket();
            isInContainer = true;
        }
        internal virtual void MoveToSocket() {
            transform.position = _container.transform.position + socketPosition;
            transform.rotation = _container.transform.rotation * socketRotation;
        }
        public virtual void UnmountFromContainer() {
            if (!_container || !isInContainer) return;
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(transform, $"Mount node '{Id}' to container");
            UnityEditor.Undo.RecordObject(this, $"Set node '{Id}' IsInContainer to false");
#endif
            transform.position = outsideContainerPosition;
            transform.rotation = outsideContainerRotation;
            isInContainer = false;
        }
        #endregion
        #region Assets Operations
        public virtual IEnumerator LoadResources() {
            Coroutine loadIcon = StartCoroutine(LoadIcon());
            Coroutine loadModel3D = StartCoroutine(LoadModel3D());
            Coroutine loadIndicator = StartCoroutine(LoadIndicator());
            yield return loadIcon;
            yield return loadModel3D;
            yield return loadIndicator;
            onLoadResourcesFinished.Invoke();
        }
        ///<summary>If hasImage is true ，try loading image from the following locations by sequence untill succeeded:
        ///<para>1. Image field value</para>
        ///<para>2. overridingImagePath (if not null)</para>
        ///<para>3. StreamingAssets{streamingImageFolder}/{NodeResourceFileName}.png</para>
        ///<para>4. StreamingAssets/{streamingImageFolder}/{NodeResourceFileName}.jpg</para>
        ///</summary>
        public virtual IEnumerator LoadImage()
        {   
            if (!hasImage || image) yield break;
            Debug.Log($"Node {id}:Loading image...");
            bool triedDefaultPng = false;
            bool triedDefaultJpg = false;
            string filePath;
            if (!string.IsNullOrEmpty(overridingImagePath))
            {
                filePath = overridingImagePath;
            }
            else
            {
                filePath = StreamingAssetsPath(StreamingImagePath1);
                triedDefaultPng = true;
            } 
            getImage:
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath))
            {
                yield return request.SendWebRequest();
    #if UNITY_2020_1_OR_NEWER
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    #else 
                if (uwr.isNetworkError || uwr.isHttpError)
    #endif
                {
                    if (!triedDefaultPng)
                    {
                        filePath = StreamingAssetsPath(StreamingImagePath1);
                        triedDefaultPng = true;
                        goto getImage;
                    }
                    else if (!triedDefaultJpg)
                    {
                        filePath = StreamingAssetsPath(StreamingImagePath2);
                        triedDefaultJpg = true;
                        goto getImage;
                    }
                    else
                    {
                        Debug.LogError(request.error + ", " + displayName + "'s image dosn't exists!");
                        Console.Error(request.error + ", " + filePath + "  Image loading failed!");
                    }
                }
                else
                {
                    Console.Info("Loading image from path: " + filePath);
                    image = DownloadHandlerTexture.GetContent(request);
                    Debug.Log("image: " + image.name + ", path: " + filePath);
                    yield return null;
                }
            }
        }
        ///<summary>If hasDescription is true ，try loading description from the following locations by sequence untill succeeded:
        ///<para>1. Description field value</para>
        ///<para>2. StreamingAssetsPath/customDescriptionPath</para>
        ///<para>3. StreamingAssetsPath/{displayName}.txt</para>
        ///</summary>
        public virtual IEnumerator LoadDescription()
        {
            if (!hasDescription || !string.IsNullOrEmpty(description)) yield break;
            string filePath;
            if (!string.IsNullOrEmpty(overridingDescriptionPath)) filePath = StreamingAssetsPath(overridingDescriptionPath);
            else filePath = StreamingAssetsPath(StreamingDescPath);
            using (UnityWebRequest request = UnityWebRequest.Get(filePath))
            {
                yield return request.SendWebRequest();
    #if UNITY_2020_1_OR_NEWER
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    #else 
                if (request.isNetworkError || request.isHttpError)
    #endif
                {
                    Debug.LogError(request.error + "," + displayName + " loading text file failed!");
                    Console.Error(request.error + "," + displayName + " Text loading failed!");
                }
                else
                {
                    description = request.downloadHandler.text;
                    yield break;
                }
            }
        }
        public virtual IEnumerator LoadIcon()
        {
            if (string.IsNullOrEmpty(iconPath)) yield break;
            BeginForceUpdateInEditMode();
            isLoadingIcon = true;
            Sprite buffer = null;
            yield return LoadAddressableAsset<Sprite>(buffer, iconPath);
            if (!buffer) goto end;     
            icon = buffer;
            end:
            isLoadingIcon = false;
            EndForceUpdateInEditMode();
        }
        public virtual IEnumerator LoadModel3D()
        {
            isLoadingModel3D = true;
            if (string.IsNullOrEmpty(model3dPath)) goto end;
            // Debug.Log($"Started Loading model 3D asset of {id} ({name})...");
            GameObject buffer = null;
            yield return LoadAddressableAsset(buffer, model3dPath);
            if (!buffer) goto end;
            
            //Destroy old.
            if(model3d) DestroyImmediate(model3d);
            var filter = GetComponent<MeshFilter>();
            var render = GetComponent<MeshRenderer>();
            var collider = GetComponent<Collider>();
            if (filter) DestroyImmediate(filter);
            if (render) DestroyImmediate(render);
            if (collider) DestroyImmediate(collider);

            //Create new.
            model3d = Instantiate(buffer, transform);
            meshRenderers = model3d.GetComponentsInChildren<MeshRenderer>();
            colliders = model3d.GetComponentsInChildren<Collider>();
            Debug.Log($"Finished instantiating and setting up Model 3D of {id} {displayName}");
            end:
            SetBounds();
            if (controller.adaptiveSceneBounds) {
                controller.sceneBounds.Encapsulate(bounds);
            }
            isLoadingModel3D = false;
        }
        public virtual IEnumerator LoadIndicator()
        {
            if (string.IsNullOrEmpty(indicatorPath)) yield break;
            BeginForceUpdateInEditMode();
            isLoadingIndicator = true;
            GameObject buffer = null;
            yield return LoadAddressableAsset<GameObject>(buffer, indicatorPath);
            if (!buffer) goto end;
            indicator = Instantiate(buffer, transform);
            end:
            isLoadingIndicator = false;
        }
        /// <summary>
        /// Preffer <see cref="meshRenderers"/>，if null, try <see cref="colliders"/>。
        /// </summary>
        private void SetBounds() {
            bounds = new Bounds(transform.position, Vector3.zero);
            if (meshRenderers != null && meshRenderers.Length > 0) {
                foreach (var renderer in meshRenderers) {
                    bounds.Encapsulate(renderer.bounds);
                }
            } else if (colliders != null && colliders.Length > 0) {
                foreach (var collider in colliders) {
                    bounds.Encapsulate(collider.bounds);
                }
            }

        }
        #endregion
        #region Data IO
        public bool serializeData;
        public string tableName = "node";
        public string gettingSQL;
        #endregion
    }
}
