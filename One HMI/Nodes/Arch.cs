using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Sean21.OneHMI
{
    using static Generics;
    public partial class Arch : Node {
        public Material highlighting;
        public string highlightingPath;
        //A Arch can be included by at most 4 Areas.
        public override Area area => area0 ?? area1 ?? area2 ?? area3;
        public Area area0;
        public string area0Id;
        public Area area1;
        public string area1Id;
        public Area area2;
        public string area2Id;
        public Area area3;
        public string area3Id;
        Material[] normalMat;
        Material[] currentMat;
        //Hide this Arch if all it's areas aren't visible
        public bool shouldHide {
            get {
                if (area0 && area0.isVisible) return false;
                if (area1 && area1.isVisible) return false;
                if (area2 && area2.isVisible) return false;
                if (area3 && area3.isVisible) return false;
                return true;
            }
        }
        partial void ed_OnEnable();
        protected override void OnEnable() {
            base.OnEnable();
            RegisterToAreas();
            StoreNormalMat();
            ed_OnEnable();
        }
        partial void ed_OnDisable();
        protected override void OnDisable() {
            base.OnDisable();
            DeregisterFromAreas();
            ed_OnDisable();
        }
        IEnumerator LoadHighlighting() {
            yield return LoadAddressableAsset<Material>(highlighting, highlightingPath);
        }
        public override IEnumerator LoadModel3D() {
            yield return base.LoadModel3D();
            StoreNormalMat();
        }
        private void StoreNormalMat() {
            if ((!highlighting) || (!model3d)) return;
            var renderer = model3d.GetComponent<MeshRenderer>();
            if (!renderer) return;
            currentMat = renderer.sharedMaterials;
            normalMat = new Material[currentMat.Length];
            currentMat.CopyTo(normalMat, 0);
        }
        public override void HighlightingOn() {
            base.HighlightingOn();
            if (highlighting == null) return;
            for (int i = 0; i < currentMat.Length; i++)
                currentMat[i] = highlighting;
        }
        public override void HighlightingOff() {
            base.HighlightingOff();
            if (highlighting == null) return;
            for (int i = 0; i < currentMat.Length; i++)
                currentMat[i] = normalMat[i];
        }

        private void UpdateAreasId() {
            UpdateArea0ID();
            UpdateArea1ID();
            UpdateArea2ID();
            UpdateArea3ID();
        }
        internal void UpdateArea0ID() {
            if (!area0) { area0Id = string.Empty; return; }
            area0Id = area0.Id;
        }
        internal void UpdateArea1ID() {
            if (!area1) { area1Id = string.Empty; return; }
            area1Id = area1.Id;
        }
        internal void UpdateArea2ID() {
            if (!area2) { area2Id = string.Empty; return; }
            area2Id = area2.Id;
        }
        internal void UpdateArea3ID() {
            if (!area3) { area3Id = string.Empty; return; }
            area3Id = area3.Id;
        }
        public override bool RegisterToLatestArea() {
            return false;
        }
        private void RegisterToAreas() {
            RegisterToArea0();
            RegisterToArea1();
            RegisterToArea2();
            RegisterToArea3();
        }
        internal void RegisterToArea0() {
            if (!area0) return;
            area0.archs.Add(this);
        }
        internal void RegisterToArea1() {
            if (!area1) return;
            area1.archs.Add(this);
        }
        internal void RegisterToArea2() {
            if (!area2) return;
            area2.archs.Add(this);
        }
        internal void RegisterToArea3() {
            if (!area3) return;
            area3.archs.Add(this);
        }
        public void ClearAreas() {
            DeregisterFromAreas();
            area0 = area1 = area2 = area3 = null;
            area0Id = area1Id = area2Id = area3Id = null;
        }
        private void DeregisterFromAreas() {
            DeregisterFromArea0();
            DeregisterFromArea1();
            DeregisterFromArea2();
            DeregisterFromArea3();
        }
        internal void DeregisterFromArea0() {
            if (!area0) return;
            area0.archs.Remove(this);
        }
        internal void DeregisterFromArea1() {
            if (!area1) return;
            area1.archs.Remove(this);
        }
        internal void DeregisterFromArea2() {
            if (!area2) return;
            area2.archs.Remove(this);
        }
        internal void DeregisterFromArea3() {
            if (!area3) return;
            area3.archs.Remove(this);
        }
        internal bool OwnedBy(Area area) {
            if (area0 == area) return true;
            if (area1 == area) return true;
            if (area2 == area) return true;
            if (area3 == area) return true;
            return false;
        }
        /// <returns>0-3：The index of the newly added Area; -2：The target Area was NOT added because all of the four areas has been occupied</returns>
        public int AddArea(Area area) {
            if (!area0) {
                area0 = area;
                RegisterToArea0();
                UpdateArea0ID();
                return 0;
            }
            if (!area1) {
                area1 = area;
                RegisterToArea1();
                UpdateArea1ID();
                return 1;
            }
            if (!area2) {
                area2 = area;
                RegisterToArea2();
                UpdateArea2ID();
                return 2;
            }
            if (!area3) {
                area3 = area;
                RegisterToArea3();
                UpdateArea3ID();
                return 3;
            }
            return -2;
        }
        /// <summary>
        /// Automatically set areas according to <see cref="colliders"/> or <see cref="meshRenderers"/>. This method is performance-costly, 
        /// calling it in Update is not recommended.
        /// </summary>
        /// <returns>areas count after reset</returns>
        public virtual int ResetAreas() {
            ClearAreas();
            if (!model3d || Arch.Manifest.Count < 1) return 0;
            List<Collider> benchmarks = new List<Collider>();
            bool shouldDistroyBenchmarks = false;
            //Look for existing colliders
            if (colliders != null && colliders.Length > 0) {
                benchmarks = new List<Collider>(colliders);
                goto start_set;
            }
            //No existing colliders, add one with meshRenderers -> MeshFilter.
            if (meshRenderers != null && meshRenderers.Length > 0) {
                shouldDistroyBenchmarks = true;
                foreach (var renderer in meshRenderers) {
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (!filter) continue;
                    Mesh mesh = filter.sharedMesh;
                    if (!mesh) continue;
                    MeshCollider collider = renderer.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = mesh;
                    benchmarks.Add(collider);
                }
            } else return 0;
            start_set:
            int areasCount = 0;
            foreach (var benchmark in benchmarks) {
                //Find areas intercecting with benchmark.
                foreach (var area in Area.Manifest.Values) {
                    if (area.isShell) continue;
                    //for each area
                    foreach (var envelope in area.envelopes) {
                        bool shouldAdd = Physics.ComputePenetration(
                            envelope, envelope.transform.position, envelope.transform.rotation,
                            benchmark, benchmark.transform.position, benchmark.transform.rotation,
                            out var direction, out float distance
                        );
                        if (shouldAdd) {
                            areasCount = AddArea(area) + 1;
                            if (areasCount < 0) Debug.LogError("ResetAreas malfunctioned!");
                            if (areasCount > 3) return areasCount;
                            break;
                        }
                    }
                }
            }
            //Destroy all Colliders generated by meshRenderers
            if (shouldDistroyBenchmarks) {
                while(benchmarks.Count > 0 ) {
                    DestroyImmediate(benchmarks[0]);
                    benchmarks.RemoveAt(0);
                }
            }
            return areasCount;
        }
    }
}
