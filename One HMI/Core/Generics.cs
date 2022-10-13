using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.IO;
using System.Linq;

namespace Sean21.OneHMI
{
    public static class Generics {
        #region Const Values
        public const string AreaLayerName = "HMI Area";
        #endregion
        #region Asset Management
        public const string createAssetMenuRoot = "One HMI/";
        public const string createGameObjectMenuRoot = "One HMI/";
        public static string GetStreamingFileLocation(string relativePath) {
            return Path.Combine(Application.streamingAssetsPath, relativePath);
        }
        public static string StreamingAssetsPath(string relativePath) {
            return Path.Combine(Application.streamingAssetsPath, relativePath);
        }
        public static IEnumerator LoadAddressableAsset<T>(T target, string path)
        where T : UnityEngine.Object {
            if (string.IsNullOrEmpty(path)) yield break;
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(path);
            yield return handle;
            if (handle.Result == null) {
                Debug.LogError("Failed loading Resource from path " + path);
                yield break;
            }
            target = handle.Result;
        }
        #endregion
        #region Reference Acquisition
        ///<summary>Find reference from the current scene</summary>
        public static T FindRef<T>(ref T member, bool overwriteExisting = false, bool enableDebugLog = false) where T : UnityEngine.Object {
            return GetReference<T>(ref member, GameObject.FindObjectOfType<T>(), overwriteExisting, enableDebugLog);
        }
        ///<summary>Find reference from transform path in the current scene</summary>
        public static T FindRef<T>(ref T member, string path, bool overwriteExisting = false, bool enableDebugLog = false) where T : Component {
            return GetReference<T>(ref member, GameObject.Find(path).GetComponent<T>(), overwriteExisting, enableDebugLog);
        }
        ///<summary>Find reference on the current GameObject</summary>
        public static T GetRef<T>(this MonoBehaviour _object, ref T member, bool overwriteExisting = false, bool enableDebugLog = false) where T : Component {
            return GetReference<T>(ref member, _object.GetComponent<T>(), overwriteExisting, enableDebugLog);
        }
        ///<summary>Find reference under the current transform</summary>
        public static T GetRefDown<T>(this MonoBehaviour _object, ref T member, bool overwriteExisting = false, bool enableDebugLog = false) where T : Component {
            return GetReference<T>(ref member, _object.GetComponentInChildren<T>(), overwriteExisting, enableDebugLog);
        }
        ///<summary>Find reference under the current transform specifying path</summary>
        public static T GetRefDown<T>(this MonoBehaviour _object, ref T member, string path, bool overwriteExisting = false, bool enableDebugLog = false) where T : Component {
            Transform targetTransform = _object.transform.Find(path);
            return GetReference<T>(ref member,
                targetTransform ? targetTransform.GetComponent<T>() : null,
                overwriteExisting, enableDebugLog
            );
        }

        public static T GetReference<T>(ref T member, T referneceTarget, bool overwriteExisting = false, bool enableDebugLog = false)
        where T : UnityEngine.Object //Must have this constraint, otherwise memeber will never be null
        {
            if (member != null && !overwriteExisting) return member;
            member = referneceTarget;
            if (member == null && enableDebugLog) Debug.LogWarning($"Cannot find reference target");
            return member;
        }
        // public static T SerializableGet<T>(ref T field, Func<T> getter = null) 
        // where T: UnityEngine.Object {
        //     if (field != null) return field;

        //     if (getter == null) field = UnityEngine.Object.FindObjectOfType<T>();
        //     else field = getter();
        //     return field;
        // }
        ///<summary>
        ///Try get <paramref name="field"/> from <paramref name="getter"/>. If <paramref name="getter"/> is null, find from the scene.
        ///</summary>
        /// <param name="onGet">To be called after <paramref name="field"/> is finally assigned</param>
        public static T SerializableGet<T>(ref T field, T getter = null, Action onGet = null, bool overwriteExisting = false)
        where T : UnityEngine.Object {
            if (field != null && !overwriteExisting) return field;
            // field = getter?? UnityEngine.Object.FindObjectOfType<T>();
            field = getter;
            if (field == null) field = UnityEngine.Object.FindObjectOfType<T>();
            if (onGet != null) onGet();
            return field;
        }
        public static T SerializableGetComponent<T>(this MonoBehaviour obj, ref T field, Action onGet = null)
        where T : Component {
            if (field != null) return field;

            field = obj.GetComponent<T>();
            if (onGet != null) onGet();
            return field;
        }
        public static T InstantiateIfNeeded<T>(ref T obj, string name = null, Action onInstatiate = null)
        where T : Component {
            if (!obj) obj = SerializableGet(ref obj);
            if (!obj) obj = new GameObject(name ?? typeof(T).Name).AddComponent<T>();

            // obj ??= SerializableGet(ref obj)?? 
            //     new GameObject(name?? typeof(T).Name).AddComponent<T>();
            if (onInstatiate != null) onInstatiate();
            return obj;
        }
        // public static T Instantiate<T>(string name) 
        // where T: Component {
        //     return new GameObject(name?? typeof(T).Name).AddComponent<T>();
        // }
        public static T GetComponentInDirectChildren<T>(this Component obj, Predicate<Component> condition = null)
        where T : Component {
            var component = obj.GetComponent<T>();
            if(component) return component;
            for (int i = 0; i < obj.transform.childCount; i++) {
                var child = obj.transform.GetChild(i);
                if (condition != null && !condition(obj)) continue;
                component = child.GetComponent<T>();
                if(component) return component;
            }
            return null;
        }
        public static T GetComponentInDirectChildren<T>(this GameObject obj, Predicate<GameObject> condition = null)
        where T : Component {
            var component = obj.GetComponent<T>();
            if(component) return component;
            for (int i = 0; i < obj.transform.childCount; i++) {
                var child = obj.transform.GetChild(i);
                if (condition != null && !condition(obj)) continue;
                component = child.GetComponent<T>();
                if(component) return component;
            }
            return null;
        }
        public static T ValidateComponent<T>(this MonoBehaviour obj, ref T component) 
        where T: Component
        {
            if (component != null) return component;
            component = obj.GetComponent<T>();
            if(!component) component = obj.gameObject.AddComponent<T>();
            return component;
        }
        #endregion
        #region Null Check
        ///<summary>Send error if Member is null</summary>
        public static T NullRef_Error<T>(this object _object, T member, string outputLog = null)
        where T : UnityEngine.Object {
            return NullRef(_object, member, outputLog, 3);
        }
        ///<summary>Send warning if Member is null</summary>
        public static T NullRef_Warning<T>(this object _object, T member, string outputLog = null)
        where T : UnityEngine.Object {
            return NullRef(_object, member, outputLog, 2);
        }
        ///<summary>Send log if Member is null</summary>
        public static T NullRef_Log<T>(this object _object, T member, string outputLog = null)
        where T : UnityEngine.Object {
            return NullRef(_object, member, outputLog, 1);
        }
        public static T NullRef<T>(this object _object, T member, string outputLog = null, int logLevel = 0)
        where T : UnityEngine.Object {
            if (member != null) return member;
            //Debug
            string objectName = _object is Component ? (_object as Component).name :
                _object is UnityEngine.Object ? (_object as UnityEngine.Object).name :
                    _object.GetType().Name;
            string typeName = $"<{typeof(T).Name}>";
            string output = $"{objectName}: lost reference of {typeName}! {outputLog}";
            switch (logLevel) {
                default: break;
                case 1: Debug.Log(output); break;
                case 2: Debug.LogWarning(output); break;
                case 3: Debug.LogError(output); break;
            }
            return null;
        }
        #endregion
        #region Extention Methods
        public static void SetLayerRecursively(this GameObject root, int layer) {
            root.layer = layer;
            for (int i = 0; i < root.transform.childCount; i++) {
                GameObject branch = root.transform.GetChild(i).gameObject;
                SetLayerRecursively(branch, layer);
            }
        }
        /// <summary>
        /// Get components in the children matching <paramref name="filter"/>. Performs recursively, not on <paramref name="obj"/> itself.
        /// </summary>
        public static T[] GetComponentsInChildrenFiltered<T>(this Component obj, Predicate<Transform> filter) 
        where T : Component
        {
            IEnumerable<T> comps = obj.GetComponents<T>();         
            for (int i=0; i<obj.transform.childCount; i++ ) {
                var child = obj.transform.GetChild(i);
                if (!filter(child)) continue;
                comps = comps.Concat(child.GetComponentsInChildrenFiltered<T>(filter));
            }
            return comps.ToArray();
        }
        /// <summary>
        /// Destroy all existing components of <typeparamref name="T"/> on <paramref name="obj"/>, then add a new one.
        /// </summary>
        public static T ClearComponentsBeforeAdd<T>(this GameObject obj)
        where T : Component {
        get:
            var comp = obj.GetComponent<T>();
            if (comp) {
                Component.DestroyImmediate(comp);
                goto get;
            }
            return obj.gameObject.AddComponent<T>();
        }
        ///<summary>If<paramref name = "character"/> is any of letter, digit or underscore</summary>
        public static bool IsSQLFriendly(this char character, bool lowerCaseOnly = true) {
            bool preDecide = (character >= 'a' && character <= 'z') || (character >= '0' && character <= '9') || (character == '_');
            return lowerCaseOnly ? preDecide : (preDecide || (character >= 'A' && character <= 'Z'));
        }

        ///<summary>if<paramref name = "input"/> includes no characther other than letter, digit or underscore</summary>
        public static bool IsSQLFriendly(this string input) {
            foreach (char c in input) {
                if (c.IsSQLFriendly()) continue;
                else return false;
            }
            return true;
        }
        ///<summary>Process <paramref name="input"/> replacing ' ' , '@' and '.' with '_' , then pass to<paramref name="result"/>.</summary>
        public static bool IsSQLFriendly(this string input, out string result, bool lowerCaseOnly = true) {
            bool returned = true;
            var inputList = new List<char>(input.ToCharArray());
            for (int i = inputList.Count - 1; i >= 0; i--) {
                char c = inputList[i];
                //pass lowercase
                if (c.IsSQLFriendly(true)) {
                    continue;
                } else if (c >= 'A' && c <= 'Z') {
                    //set returned and convert to lowercase if needed.
                    if (lowerCaseOnly) {
                        returned = false;
                        inputList[i] = Char.ToLower(c);
                    } 
                    //pass uppercase
                    continue;
                }
                returned = false;
                if (c == ' ' || c == '.' || c =='@') inputList[i] = '_';
                //remove unsupported characters
                else inputList.RemoveAt(i);
            }
            result = string.Concat(inputList);
            return returned;
        }
        ///<summary>Get the max dimension value and axis index of <paramref name="bounds"/>.</summary>
        public static (float value, int axis) MaxDimension(this Bounds bounds) {
            Vector3 size = bounds.size;
            float maxValue = 0;
            int maxAxis = 0;
            for (int i = 0; i < 3; i++) {
                float value = Mathf.Abs(size[i]);
                if (value <= maxValue) continue;
                maxValue = value;
                maxAxis = i;
            }
            return (maxValue, maxAxis);
        }
        ///<summary>If <paramref name="position"/> is inside <paramref name="collider"/>.</summary>
        public static bool IsInside(this Vector3 position, Collider collider) {
            return position == collider.ClosestPoint(position);
        }
        public static bool IsInCameraView(this Vector3 position, Camera camera) {
            var viewPos = camera.WorldToViewportPoint(position);
            return !(viewPos.x<0 || viewPos.x>1 || viewPos.y<0 || viewPos.y>1 || viewPos.z<0);
        }
        #endregion
        #region Utilities
        ///<summary>Singleton Base Class</summary>
        public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
            private static T _i;
            public static T i {
                //In normal cases, call of FindObjectOfType here would not be executed at runtime.
                get => SerializableGet(ref _i, FindObjectOfType<T>());
                private set => _i = value;
            }
            protected virtual void Awake() {
                Register();
            }
            protected virtual void OnEnable() {
                Register();
            }
            protected void Register() {
                if (i == null) {
                    i = this as T;
                } else if (i != this) {
                    Debug.LogWarning($"Multiple instances of {typeof(T).Name} is running, this may cause unexpected behaviours!. The previous instances are ignored!");
                    i = this as T;
                }
                // Debug.Log($"{name}: Register finished.");
            }
        }
        // public static RemoveComponentByName() {
        // }
    #endregion
    #region Data Types
    [Serializable]
        public class Limited
        {
            public float min;
            public float max;
            [SerializeField]protected float _value;
            public float value
            {
                get { return _value;}
                set 
                { 
                    if (value < min || value > max) Debug.LogWarning(_value + ": unexpected value, has been clampped! ");
                    _value = Mathf.Clamp(value, min, max);
                    _t = Mathf.InverseLerp(min, max, _value);
                }
            }
            //Do Not use this field anywhere else
            protected float _t;
            public float t
            {
                get {return _t;}
                set
                {
                    _t = Mathf.Clamp(value, 0f, 1f);
                    _value = Mathf.Lerp(min, max, _t);
                }
            }
            public string unit;
            public float range
            {
                get{return max-min;}
            }
            public Limited(float _value = 0f, float _min = 0f, float _max = 1f, string _unit = null)
            {
                min = _min;
                max = _max;
                value = _value;
                unit = _unit;
            }
            public Limited Parse(string data)
            {
                Limited newValue = this;
                newValue.value = float.Parse(data);
                return newValue;
            }
            public override string ToString()
            {
                return value.ToString();
            }
        }
        [Serializable]
        public class Segmental: Limited
        {
            // public bool isInverse = false;
            public List<Segment> segments;
            [Serializable]
            public class Segment
            {
                public float start;
                // public float end;
                public string label;
                public Color color;
                public Segment( float _start, string _label = null)
                {
                    start = _start;
                    label = _label;
                    color = Color.green;
                }
            }
            protected Segment currentSeg
            {
                get
                {
                    for (int i=0; i<segments.Count-1; i++)
                    {
                        if (segments[i].start<=_value && _value<segments[i+1].start)
                        {
                            return segments[i];      
                        }
                    }
                    if ( segments[segments.Count-1].start <= _value)
                        return segments[segments.Count-1];
                    else return null;
                }
            }
            public string currentLabel
            {
                get{ return currentSeg.label; }
            }
            public Color currentColor
            {
                get { return currentSeg.color; }
            }
            public int currentIndex
            {
                get { return segments.IndexOf(currentSeg); }
            }
            public Segmental(float _value = 0f, float _min = 0f, float _max = 1f, int segCount = 2, string[] labels = null, Color[] colors = null, string _unit = null )
            {
                min = _min;
                max = _max;
                value = _value;
                unit = _unit;
                segments = new List<Segment>();
                for (int i=0; i<segCount; i++)
                {
                    segments.Add( new Segment (min + i * range/segCount) );
                    if ( !string.IsNullOrEmpty(labels[i]) ){
                        segments[i].label = labels[i];
                    }
                    if (colors[i] != null){
                        segments[i].color = colors[i];
                    }
                }
            }
            public new Segmental Parse(string data)
            {
                Segmental newValue = this;
                newValue.value = float.Parse(data);
                return newValue;
            }
        }
        [Serializable]
        ///<summary>A delegate for both UI.Text and TMPro.TextMeshProUGUI</summary>
        public class GeneralText {
            public bool isNull { get {
                return ui_text == null && tmp_text == null;
            } }
            public void DestroyContent() {
                if (ui_text) UnityEngine.Object.DestroyImmediate(ui_text);
                if (tmp_text) UnityEngine.Object.DestroyImmediate(tmp_text);
            }
            /// <summary>
            /// Switch type between <see cref="Text"/> (if <paramref name="tmp"/> is <c>true</c>) and <see cref="TextMeshProUGUI"/> (if <paramref name="tmp"/> is <c>false</c>), reserving the text content.
            /// </summary>
            public void SwitchTMP(bool tmp, GameObject target) {
                // string buffer = tmp_text ? tmp_text.text : ui_text ? ui_text.text : null;
                (string text, Color color, float fontSize) buffer = 
                tmp_text ? (tmp_text.text, tmp_text.color, tmp_text.fontSize) : 
                ui_text ? (ui_text.text, ui_text.color, ui_text.fontSize) : 
                (null, HMIHub.i.schema.labelColor, HMIHub.i.schema.labelfontSize);
                DestroyContent();
                if (tmp) tmp_text = target.AddComponent<TMPro.TextMeshProUGUI>();
                else ui_text = target.AddComponent<UnityEngine.UI.Text>();
                text = buffer.text;
                color = buffer.color;
                fontSize = buffer.fontSize;
            }
            [SerializeField]
            private UnityEngine.UI.Text ui_text;
            public string text {
                get => 
#if USE_TMP
                    tmp_text? tmp_text.text : 
#endif
                    ui_text? ui_text.text : null;
                set{
#if USE_TMP
                    if (tmp_text) tmp_text.text = value; 
#endif
                    if (ui_text) ui_text.text = value;
                }
            }

            public Color color {
                get =>
#if USE_TMP
                    tmp_text ? tmp_text.color :
#endif
                    ui_text ? ui_text.color : Color.white;
                set {
#if USE_TMP
                    if (tmp_text) tmp_text.color = value;
#endif
                    if (ui_text) ui_text.color = value;
                }
            }
            public float fontSize {
                get =>
#if USE_TMP
                    tmp_text ? tmp_text.fontSize :
#endif
                    ui_text ? ui_text.fontSize : HMIHub.i.schema.labelfontSize;
                set {
#if USE_TMP
                    if (tmp_text) tmp_text.fontSize = value;
#endif
                    if (ui_text) ui_text.fontSize = (int)value;
                }
            }
            public GeneralText() { }
            private GeneralText (UnityEngine.UI.Text text) {
                ui_text = text;
            }
            public static implicit operator UnityEngine.UI.Text(GeneralText t) => t.ui_text;
            public static implicit operator GeneralText(UnityEngine.UI.Text ui_t) => new GeneralText(ui_t);
#if USE_TMP
            [SerializeField]
            private TMPro.TextMeshProUGUI tmp_text;
            private GeneralText (TMPro.TextMeshProUGUI text) {
                tmp_text = text;
            }
            public static implicit operator TMPro.TextMeshProUGUI(GeneralText t) => t.tmp_text;
            public static implicit operator GeneralText(TMPro.TextMeshProUGUI tmp_t) => new GeneralText(tmp_t);
#endif    
        }
        
        public static class Fitting {
            public static float Linear(float t) {
                return t;
            }
            public static float Sin(float t) {
                return Mathf.Sin((Mathf.PI / 2) * t);
            }
        }
#endregion
#region IEnumerators     
        ///<summary>
        ///Provide a support for coroutines to run properly in Edit Mode
        ///<example>
        ///<code>
        ///IEnumerator coroutine() { 
        ///    Generics.BeginForceUpdate();
        ///    while(some_condition) {
        ///        yield return null;
        ///    }
        ///    Generics.EndForceUpdate();
        ///}
        ///</code>
        ///</example>
        ///The coroutine will work properly in build as well as in edit mode.
        ///</summary>
        public static void BeginForceUpdateInEditMode()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.EditorApplication.update +=
                    UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
            }
#endif
        }
        ///<summary>
        ///End Editor force updating started with <see cref="BeginForceUpdateInEditMode"/>.
        ///</summary>
        public static void EndForceUpdateInEditMode()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.EditorApplication.update -=
                    UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
            }
#endif
        }
        public static IEnumerator StartCoroutinesParallel(this MonoBehaviour obj, params IEnumerator[] coroutines) {
            HashSet<Coroutine> runnings = new HashSet<Coroutine>();
            BeginForceUpdateInEditMode();
            foreach(var coroutine in coroutines) {
                runnings.Add(obj.StartCoroutine(coroutine));
            }
            foreach(var running in runnings) {
                yield return running;
            }
            EndForceUpdateInEditMode();
        }
#endregion
#region Data Processing
        public class Field: PropertyAttribute {
            public int length;
            public Field(int length) {
                this.length = length < 1 ? -1 : length;
            }
            public Field() : this(-1){}
        }
#endregion
    }
}
