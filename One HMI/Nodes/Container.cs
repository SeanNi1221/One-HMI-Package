using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sean21.OneHMI
{
    public partial class Container : Node
    {
        ///<summary><paramref name="TKey"/>:id, <paramref name="TValue"/>:container</summary>
        public new static readonly Dictionary<string, Container> Manifest = new Dictionary<string, Container>();
        ///<summary><para><paramref name="TKey"/>:id, <paramref name="TValue"/>:contentsBuffer</para>
        ///Cache already instantiated contents if current Container is not yet instantiated.</summary>
        public new static readonly Dictionary<string, HashSet<Node>> Buffer = new Dictionary<string, HashSet<Node>>();
        // public Dictionary<Node, Transform> inventory = new Dictionary<Node, Transform>();
        public readonly HashSet<Node> contents = new HashSet<Node>();
        [Tooltip("Automatically locate to the Sockets' position on contents register.")]
        public bool autoMount = true;
        [Tooltip("Moving this Container will also move all its contents.")]
        public bool bindContents = true;
        protected override void OnEnable() {
            base.OnEnable();
            onTransformChange.AddListener(() => { if (bindContents) MoveMountedContentsTogether(); });
        }
        internal override void Register() {
            base.Register();
            if(!Manifest.TryAdd(Id, this)) {Debug.Log($"{Id}: Registering Container failed: The area is already in manifest.");}
        }
        public override bool RegisterToContainer() {
            //Proccess buffer before register
            if (Buffer.TryGetValue(Id, out var contentsBuffer)) {
                foreach(var node in contentsBuffer) {
                    node.RegisterToContainer();
                }
            }
            Buffer.Remove(Id);
            return base.RegisterToContainer();
        }
        internal override void Deregister() {
            base.Deregister();
            if(!Manifest.Remove(Id)) {Debug.LogWarning($"{Id}: Withdrawing Container failed: The node dosn't exist in manifest.");}
        }
        public virtual void MountContents() {
            foreach (var content in contents) content.MountToContainer();
        }
        public virtual void MountContents(bool recursive = false) {
            foreach (var content in contents) { 
                content.MountToContainer();
                if (recursive) {
                    Container subContainer = content as Container;
                    if (subContainer) subContainer.MountContents(true);
                }
            }
        }
        public virtual void UnmountContents() {
            foreach (var content in contents) content.UnmountFromContainer();
        }
        public virtual void UnmountContents(bool recursive) {
            foreach (var content in contents) {
                content.UnmountFromContainer();
                if (recursive) {
                    Container subContainer = content as Container;
                    if (subContainer) subContainer.UnmountContents(true);
                }
            }
        }
        protected virtual void MoveMountedContentsTogether() {
            foreach (var content in contents) {
                if (content.IsInContainer) content.MoveToSocket();
            }
        }
    }
}
