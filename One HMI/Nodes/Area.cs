using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Sean21.OneHMI
{
    using static Generics;
    public partial class Area : Node
    {   
        ///<summary><paramref name="TKey"/>:id, <paramref name="TValue"/>:area</summary>
        public new static readonly Dictionary<string, Area> Manifest = new Dictionary<string, Area>();
        ///<summary><para><paramref name="TKey"/>:id, <paramref name="TValue"/>:contentsBuffer</para>
        ///Cache already instantiated contents when this Area is not yet instantiated.</summary>
        public new static readonly Dictionary<string, HashSet<Node>> Buffer = new Dictionary<string, HashSet<Node>>();

        /// <summary>If this Area is a Shell. Being A shell means it has no <see cref="envelopes"/> and is supposded to be used as a "folder".
        public bool isShell { get; private set; }
        //Area has a different rule of assigning it's area from Node.
        public override Area area => parent as Area;
        public HashSet<Arch> archs = new HashSet<Arch>();
        public HashSet<Node> contents = new HashSet<Node>();
        // public Collider[] envelopes;
        public HashSet<Collider> envelopes;
        public LabelObject Label => label;
        [SerializeField]
        private LabelObject label;
        partial void ed_OnEnable();
        protected override void OnEnable() {
            base.OnEnable();
            // if (!Application.isPlaying) {
            //     SetEnvelopes();
            // }
            onLoadResourcesFinished.AddListener(() => SetEnvelopes());
            LoadLabel();
            if (model3d) model3d.SetLayerRecursively(Schema.current.areaLayer);
            ed_OnEnable();
        }
        protected override void OnTransformParentChanged() {
            base.OnTransformParentChanged();
            UpdateLatestArea();
        }
        protected override void OnTransformChildrenChanged() {
            base.OnTransformChildrenChanged();
            // SetEnvelopes();
        }
        internal override void Register() {
            base.Register();
            if(!Manifest.TryAdd(Id, this)) {
                // Debug.Log($"{Id}: Registering Area failed: The area is already in manifest.");
            }
        }
        internal override void Deregister() {
            base.Deregister();
            if(!Manifest.Remove(Id)) {
                // Debug.LogWarning($"{Id}: Withdrawing Area failed: The node dosn't exist in manifest.");
            }
        }
        /// <summary>
        /// Find envelopes based on Model3D, not Node
        /// </summary>
        public virtual bool SetEnvelopes() {
            //Purge before set
            // envelopes = envelopes.Where(envelope => envelope != null).ToArray();
            if (!model3d) goto make_shell;
            envelopes = new HashSet<Collider>(model3d.GetComponentsInChildren<Collider>());
            if (envelopes == null || envelopes.Count < 1) goto make_shell;
            // Force convert to triggers
            // foreach (Collider col in envelopes) {
            //     var meshCollider = col as MeshCollider;
            //     if (meshCollider) {
            //         meshCollider.convex = true;
            //     }
            //     col.isTrigger = true;
            // }
            isShell = false;
            return true;
            make_shell:
                envelopes = null;
                isShell = true;
                return false;
        }
        public virtual void LoadLabel(bool forceCreate = false) {
            if (!label) {
                label = GetComponentInBelongings<LabelObject>();
                if (!forceCreate) return;
                if (!label) {
                    var go = new GameObject($"{Id}_label");
                    go.transform.SetParent(transform);
                    label = go.AddComponent<LabelObject>();
                }
            }
            UpdateLabelText();
        }
        public virtual void UpdateLabelText() {
            if (!label) return;
            label.text = this.displayName;
        }
        public override void HighlightingOn() {
            base.HighlightingOn();
            foreach (var arch in archs) arch.HighlightingOn();
        }
        public override void HighlightingOff() {
            base.HighlightingOff();
            foreach (var arch in archs) arch.HighlightingOff();
        }
        
        public override void Hide() {
            isVisible = false;
            DisableLabelObject();
            HideContents();
            HideArchs();
            foreach (Area child in children) child.Hide();
            onHide.Invoke();
            //Hide parentArea's visable contents if it is a Shell.
            if (area) area.HideShell();
            
        }
        public override void Unhide() {
            isVisible = true;
            EnableLabelObject();
            UnhideContents();
            UnhideArchs();
            foreach (Area child in children) child.Unhide();
            onUnhide.Invoke();
        }
        public virtual void EnableLabelObject() {
            if (label) label.gameObject.SetActive(true);
        }
        public virtual void DisableLabelObject() {
            if (label) label.gameObject.SetActive(false);
        }
        public virtual void HideContents() {
            foreach (var node in contents) {
                node.Hide();
            }
        }
        public virtual void UnhideContents() {
            foreach (var node in contents) {
                node.Unhide();
            }
        }
        public virtual void HideArchs() {
            foreach (Arch arch in archs) {
                if(arch.shouldHide) arch.Hide();
            }
        }
        public virtual void UnhideArchs() {
            foreach (Arch arch in archs) {
                arch.Unhide();
            }
        }
        public virtual void HideShell() {
            if (!this.isShell) return;
            isVisible = false;
            DisableLabelObject();
            HideArchs();
            var parentArea = area;
            if (parentArea && parentArea.isShell) {
                parentArea.HideShell();
            }
        }
        void PurgeArchs() {
            // for(int i=0; i<archs.Count; i++)
            //     if (!archs[i].OwnedBy(this)) archs.Remove(archs[i]);
            archs.RemoveWhere(arch => !arch || !arch.OwnedBy(this));
        }
        public override bool RegisterToLatestArea() {
            //Proccess Buffer
            if (Buffer.TryGetValue(Id, out var contentsBuffer)) {
                foreach(var node in contentsBuffer) {
                    node.RegisterToLatestArea();
                }
                Buffer.Remove(Id);
                return true;
            }
            return false;
            // return base.RegisterToLatestArea();
        }
        // void PurgeContents() {
        //     contents.RemoveWhere(node => node == null || node.area != this);
        // }
        // protected override void OnEnable() {
        //     base.OnEnable();
        //     Debug.Log($"count: {transform.hierarchyCount}, capacity: {transform.hierarchyCapacity}");
        // }
        ///<summary>If this Area encloses <paramref name="subArea"/></summary>
        ///<param name="recursive">If child areas are taken account in</param>
        public virtual bool Contains(Area subArea, bool recursive = false) {
            if (subArea.area == this) return true;
            if (recursive)
                foreach (Area child in children)
                    if (child.Contains(subArea))
                            return true;
            return false;
        }
        public virtual bool Contains(Node node, bool recursive = false) {
            if (envelopes == null) return false;
            foreach (var envelope in envelopes)
                if (node.transform.position.IsInside(envelope))
                    return true;
            if (recursive)
                foreach (Area child in children)
                    if (child.Contains(node))
                        return true;
            return false;
        }
    }
}
