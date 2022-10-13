#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Sean21.OneHMI
{
    public class OneHMIEditor : Editor
    {
        public class DrawField
        {
            public SerializedProperty serializedProperty { get; private set; }
            public GUIContent DisplayContent {
                get => displayContent?? new GUIContent(serializedProperty.displayName);
                private set => displayContent = value;
            }
            private GUIContent displayContent;

            public Action Draw;
            private OneHMIEditor editor;
            internal Editor cachedEditor;
            internal bool moved;
            internal DrawField fieldAbove;
            internal DrawField fieldBelow;
            internal string foldoutLabel;
            internal bool defaultfoldoutState;
            internal bool beginFoldoutFlag;
            internal bool endFoldoutFlag;
            private DrawField(OneHMIEditor editor) {
                this.editor = editor;
            }
            public DrawField(SerializedProperty prop, OneHMIEditor editor): this(editor){
                serializedProperty = prop;
                Draw = ()=> EditorGUILayout.PropertyField(serializedProperty, DisplayContent) ;
            }
            public DrawField(string fieldName, OneHMIEditor editor){
                serializedProperty = editor.serializedObject.FindProperty(fieldName);
                if (this.serializedProperty == null) Debug.LogError($"Cannot find field '{fieldName}' !");
                Draw = ()=> EditorGUILayout.PropertyField(serializedProperty, DisplayContent);
            }
            public DrawField Register() {
                editor.RegisterMod(this);
                return this;
            }
            public DrawField DisplayAs(string displayName) {
                DisplayContent = new GUIContent(displayName);
                return this;
            }
            public DrawField DisplayAs(GUIContent _displayContent) {
                DisplayContent = _displayContent;
                return this;
            }
            public DrawField DrawAs(Action newDraw) {
                if (serializedProperty == null) Debug.Log($"Cannot find serialize property: {serializedProperty.name}"); 
                Draw = newDraw ?? new Action(()=> EditorGUILayout.PropertyField(serializedProperty, DisplayContent));
                return this;
            }
            public DrawField AddBelow(Action content) {
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    _Draw();
                    if (content!=null) content();
                };
                return this;
            }
            public DrawField AddAbove(Action content) {
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    content();
                    _Draw();
                };
                return this;
            }
            public DrawField AddSpaceBelow(int amount = 10) {
                return this.AddBelow(() => EditorGUILayout.Space(amount));
            }
            public DrawField AddSpaceAbove(int amount = 10) {
                return this.AddAbove(() => EditorGUILayout.Space(amount));
            }
            public DrawField AddRight(Action content) {
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    EditorGUILayout.BeginHorizontal();
                    _Draw();
                    content();
                    EditorGUILayout.EndHorizontal();
                };
                return this;
            }
            public DrawField AddButtonR( GUIContent buttonContent, Action onClick, int? width=null ) {
                int _width = width ?? DefaultButtonWidth;
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    EditorGUILayout.BeginHorizontal();
                    _Draw();
                    if (GUILayout.Button(buttonContent, GUILayout.Width(_width))) onClick();
                    EditorGUILayout.EndHorizontal();
                };
                return this;
            }
            public DrawField AddButtonR( string label, Action onClick, int? width=null ) {
                int _width = width ?? DefaultButtonWidth;
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    EditorGUILayout.BeginHorizontal();
                    _Draw();
                    if (GUILayout.Button(label, GUILayout.Width(_width))) onClick();
                    EditorGUILayout.EndHorizontal();
                };
                return this;
            }
            public DrawField AddButtonR( Texture icon, Action onClick, int? width=null ) {
                int _width = width ?? DefaultButtonWidth;
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    EditorGUILayout.BeginHorizontal();
                    _Draw();
                    if (GUILayout.Button(icon, GUILayout.Width(_width))) onClick();
                    EditorGUILayout.EndHorizontal();
                };
                return this;
            }
            public DrawField MoveBelow(string anchorFieldName) {
                return MoveBelow(editor.Modify(anchorFieldName));
            }
            public DrawField MoveBelow(SerializedProperty anchorProp) {
                return MoveBelow(editor.Modify(anchorProp));
            }
            public DrawField MoveBelow(DrawField anchorField) {
                this.moved = true;
                // anchorField.bottomAnchor.Add(this);
                // anchorField.AddBelow(this.Draw);
                anchorField.fieldBelow = this;
                return this;
            }
            public DrawField MoveAbove(string fieldName) {
                return MoveAbove(editor.serializedObject.FindProperty(fieldName));
            }
            public DrawField MoveAbove(SerializedProperty prop) {
                return MoveAbove(editor.Modify(prop));
            }
            public DrawField MoveAbove(DrawField anchorField) {
                this.moved = true;
                // anchorField.topAnchor.Add(this);
                // anchorField.AddAbove(this.Draw);
                anchorField.fieldAbove = this;
                return this;
            }
            public DrawField MoveToEnd() {
                this.moved = true;
                if (editor.endFields.Contains(this)) return this;
                editor.endFields.Add(this);
                return this;
            }
            public DrawField MoveToBeginning() {
                this.moved = true;
                if (editor.beginningFields.Contains(this)) return this;
                editor.beginningFields.Add(this);
                return this;
            }
            /// <summary>
            /// Disable this <see cref="DrawField"/> if <paramref name="condition"/> is null or returns true;
            /// </summary>
            public DrawField Disable(Func<bool> condition = null) {
                Action _Draw = Draw;
                Draw = () => {
                    if (condition == null || condition()) {
                        GUI.enabled = false;
                        _Draw();
                        GUI.enabled = true;
                        return;                        
                    }
                    else _Draw();
                };
                return this; 
            }
            /// <summary>
            /// Hide this <see cref="DrawField"/> if <paramref name="condition"/> is null or returns true;
            /// </summary>
            public DrawField Hide(Func<bool> condition = null) {
                Action _Draw = Draw;
                Draw = () => {
                    if (condition == null || condition()) {
                        return;
                    } else {
                        _Draw();
                    }
                };
                return this;
            }
            public DrawField OnValueChangeAndDelayCall(Action before, params EditorApplication.CallbackFunction[] after) {
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    EditorGUI.BeginChangeCheck();
                    _Draw();
                    if (EditorGUI.EndChangeCheck()) {
                        before();
                        foreach (var task in after) EditorApplication.delayCall += task;
                    } 
                };
                return this;            
            }
            public DrawField OnValueChangeDelayCall (params EditorApplication.CallbackFunction[] tasks) {
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    EditorGUI.BeginChangeCheck();
                    _Draw();
                    if (EditorGUI.EndChangeCheck()) {
                        foreach (var task in tasks) EditorApplication.delayCall += task;
                    } 
                };
                return this;
            }
            public DrawField OnValueChange (params Action[] tasks) {
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    EditorGUI.BeginChangeCheck();
                    _Draw();
                    if (EditorGUI.EndChangeCheck()) {
                        foreach (var task in tasks) task();
                    } 
                };
                return this;
            }
            public DrawField Indent (int i=1) {
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    EditorGUI.indentLevel += i;
                    _Draw();
                    EditorGUI.indentLevel -= i;
                };
                return this;  
            }
            
            public DrawField BeginFoldoutGroup(string label, bool foldout) {
                return AddAbove(()=>{
                    foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
                });
            }
            public DrawField EndFoldoutGroup() {
                return AddBelow(() => {
                    EditorGUILayout.EndFoldoutHeaderGroup();
                });
            }
            public DrawField AddHeader (string label, params Action[] inHeaderLine) {
                GUIContent labelContent = new GUIContent(label);
                return AddAbove(() => {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(labelContent, EditorStyles.boldLabel, GUILayout.Width(
                    EditorStyles.boldLabel.CalcSize(labelContent).x));
                    foreach (Action task in inHeaderLine) task();
                    EditorGUILayout.EndHorizontal();
                });
            }
            public DrawField UnfoldSelfBelowBuiltin(Type editorType = null, bool disableContent = false) {
                Action _Draw = this.Draw;
                this.Draw = ()=> {
                    _Draw();
                    if (disableContent) GUI.enabled = false;
                    ObjectContentFoldedBuiltin(serializedProperty.objectReferenceValue, ref cachedEditor, editorType);
                    if (disableContent) GUI.enabled = true;
                };
                return this;
            }
        }
        public class Button
        {
            // public string label = null;
            // public Texture icon = null;
            // public string toolTip = null;
            public string label {
                get => content.text;
                set => content.text = value;
            }
            public Texture icon {
                get => content.image;
                set => content.image = value;    
            }
            public string toolTip {
                get => content.tooltip;
                set => content.tooltip = value;
            }
            public int? width = null;
            public int? height = null;
            // public float weight = 1f;
            public Action[] onClick;
            private GUIContent _content;
            public GUIContent content{
                get{
                    if (_content != null) return _content;
                    if (string.IsNullOrEmpty(label)) {
                        if (icon == null) {
                            _content = GUIContent.none;
                        }
                        else if (toolTip == null) {
                            _content = new GUIContent(icon);
                        }
                        else {
                            _content = new GUIContent(icon, toolTip);
                        }
                    }
                    else if (icon == null) {
                        _content = new GUIContent(label);
                    }
                    else {
                        _content = new GUIContent(label,icon);
                    }
                    return _content;
                }
                set => _content = value;
            }
            private GUILayoutOption[] _options;
            public GUILayoutOption[] options {
                get{
                    if (_options != null) return _options;
                    if (width == null)
                        if (height == null) _options = null;
                        else _options = new GUILayoutOption[]{GUILayout.Height(height??default)};
                    else if (height == null) {
                        _options = new GUILayoutOption[]{GUILayout.Width(width??default)}; 
                    }
                    else _options = new GUILayoutOption[]{GUILayout.Width(width??default), GUILayout.Height(height??default)};
                    return _options;
                }
                set => _options = value;
            }
            public void Draw() {
                if (GUILayout.Button(content, options))
                    foreach(Action task in onClick)
                        task();
            }
            public Button(GUIContent Content, params Action[] Onclick) {
                content = Content;
                onClick = Onclick;
            }
            public Button(string Label, params Action[] Onclick) : this(new GUIContent(Label), Onclick){}
            // public Button(string Label, float Weight, params Action[] Onclick) : this(Label, Onclick) {          
            //     weight = Weight;
            // }
            public Button(string Label, int Width, params Action[] Onclick) : this(Label, Onclick) {
                width = Width;
            }
            public Button(Texture Icon, params Action[] Onclick) : this(new GUIContent(Icon), Onclick) {}
            public Button(Texture Icon, string Tooltip, params Action[] Onclick) : this(new GUIContent(Icon, Tooltip), Onclick){}
            // public Button(Texture Icon, float Weight, params Action[] Onclick) : this(Icon, Onclick) {
            //     weight = Weight;        
            // }

            public Button(Texture Icon, int Width, params Action[] Onclick) : this(Icon, Onclick) {
                width = Width;
            }
            public Button(Texture Icon, int Width, int Height, params Action[] Onclick) : this(Icon, Width, Onclick) {
                height = Height;
            }
            public Button(string Label, Texture Icon, int Width , int Height, params Action[] Onclick) : this(Icon, Width, Height, Onclick)  {
                label = Label;
            }
        }
        public static int DefaultButtonWidth = 80;
        public bool requireConstantRepaint;
        protected SerializedProperty currentProp;
        protected DrawField currentField => GetModified(currentProp);
        protected Dictionary<string, DrawField> modifiedFields = new Dictionary<string, DrawField>();
        private List<DrawField> beginningFields = new List<DrawField>();
        private List<DrawField> endFields = new List<DrawField>();
        private Type thisType;
        #region Scrolls
        private Dictionary<string, Vector2> scrollStates;
        private static Dictionary<Type, Dictionary<string, Vector2>> scrollStatesRestore = new Dictionary<Type, Dictionary<string, Vector2>>();
        #endregion
        #region Foldouts
        private Dictionary<string, bool> foldoutStates;
        private static Dictionary<Type, Dictionary<string, bool>> foldoutStatesRestore = new Dictionary<Type, Dictionary<string, bool>>();
        #endregion
        #region Foldout Spans
        private Dictionary<string, bool> foldoutGroupStates = new Dictionary<string, bool>();
        public string currentSpan{ get; private set; }
        /// <summary>
        /// <typeparamref name="TKey"/>:Derived type of this editor. <typeparamref name="TValue"/>:foldouts resotre field of the editor type. 
        /// </summary>
        private static Dictionary<Type, Dictionary<string, bool>> foldoutGroupStatesRestore = new Dictionary<Type, Dictionary<string, bool>>();
        #endregion
        protected virtual void OnBeginning(){}
        protected virtual void OnEnd(){}
        protected virtual void Awake() {
            RetrieveStores();
        }
        protected virtual void OnValidate() {
            RetrieveStores();
        }
        protected virtual void OnDestroy() {
            foldoutGroupStatesRestore[thisType] = foldoutGroupStates;
            foldoutStatesRestore[thisType] = foldoutStates;
            scrollStatesRestore[thisType] = scrollStates;
        }
        private void RetrieveStores() {

            thisType = this.GetType();
            foldoutGroupStatesRestore.TryGetValue(thisType, out foldoutGroupStates);
            foldoutStatesRestore.TryGetValue(thisType, out foldoutStates);
            scrollStatesRestore.TryGetValue(thisType, out scrollStates);

            if (foldoutGroupStates == null) foldoutGroupStates = new Dictionary<string, bool>();
            if (foldoutStates == null) foldoutStates = new Dictionary<string, bool>();
            if (scrollStates == null) scrollStates = new Dictionary<string, Vector2>();
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            currentProp = serializedObject.GetIterator();
            currentProp.NextVisible(true);
            OnBeginning();
            for(int i=beginningFields.Count-1; i>=0; i--) DrawWithContext(beginningFields[i]);
            while(currentProp.NextVisible(false))
            {
                var field = GetModified(currentProp);
                if (field != null) {
                    //Cannot combine with the previous line because of the following "else if" statement;
                    if (!field.moved) DrawWithContext(field);
                }
                else if (string.IsNullOrEmpty(currentSpan) || foldoutGroupStates[currentSpan]) { 
                    EditorGUILayout.PropertyField(currentProp);
                }
            }
            for(int i=0; i<endFields.Count; i++) DrawWithContext(endFields[i]);
            OnEnd();
            serializedObject.ApplyModifiedProperties();  
            if (requireConstantRepaint) RequiresConstantRepaint();
        }
        private void DrawWithContext(DrawField field) {
            if (field == null) return;

            DrawWithContext(field.fieldAbove);

            if (field.beginFoldoutFlag) {
                EditorGUILayout.Space();
                currentSpan = field.foldoutLabel;
                foldoutGroupStates[currentSpan] = EditorGUILayout.Foldout(foldoutGroupStates[currentSpan], currentSpan, true, EditorStyles.foldoutHeader);
                EditorGUI.indentLevel++;
            }

            if(string.IsNullOrEmpty(currentSpan) || foldoutGroupStates[currentSpan] ) field.Draw();

            if (field.endFoldoutFlag) {
                EditorGUI.indentLevel--;
                currentSpan = null;
            }

            DrawWithContext(field.fieldBelow);
        }
        protected DrawField[] Modify(Func<DrawField, DrawField> newDraw, params string[] fieldNames) {
            return Array.ConvertAll(fieldNames, name => newDraw != null? newDraw(Modify(name)): Modify(name));
        }        
        protected DrawField[] Modify(Action newDraw, params string[] fieldNames) {
            return Array.ConvertAll(fieldNames, name => Modify(name, newDraw));
        }
        protected DrawField Modify(string fieldName, Action newDraw = null) {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop == null) {
                Debug.LogError($"Cannot Find Serialized Property with name '{fieldName}'");
                return null;
            }
            return Modify(prop, newDraw);
        }
        protected DrawField Modify(SerializedProperty prop, Action newDraw = null) {
            
            var modified = GetModified(prop);
            return modified != null ? modified.AddBelow(newDraw) : new DrawField(prop, this).DrawAs(newDraw).Register();
        }
        protected DrawField DrawDefault(string fieldName) {
            return DrawDefault(serializedObject.FindProperty(fieldName));
        }
        protected DrawField DrawDefault(SerializedProperty prop) {
            var modified = GetModified(prop);
            return modified != null ? modified.DrawAs(null) : new DrawField(prop, this).DrawAs(null).Register();
        }
        private DrawField GetModified(string fieldName) {
            if (modifiedFields == null) {
                Debug.LogError("modifiedFieldsDictionary is null!");
                return null;
            }
            return modifiedFields.TryGetValue(fieldName, out var field) ? field : null;
        }
        protected DrawField GetModified(SerializedProperty prop) {
            return GetModified(prop.name);
        }
        protected DrawField[] MoveToBeginning(params string[] fieldNames) {
            return Array.ConvertAll(fieldNames, prop => (Modify(prop)).MoveToBeginning());
        }
        protected DrawField[] MoveToBeginning(params SerializedProperty[] props) {
            return Array.ConvertAll(props, prop => (Modify(prop)).MoveToBeginning());
        }
        protected DrawField[] MoveToEnd(params string[] fieldNames) {
            return Array.ConvertAll(fieldNames, name => (Modify(name)).MoveToEnd());
        }
        protected DrawField[] MoveToEnd(params SerializedProperty[] props) {
            return Array.ConvertAll(props, prop => (Modify(prop)).MoveToEnd());
        }
        protected DrawField[] MoveToEnd(params DrawField[] fields) {
            return Array.ConvertAll(fields, field => field.MoveToEnd());
        }
        protected DrawField RegisterMod(DrawField field) {
            if (modifiedFields == null) {
                Debug.LogError("modifiedFields is null, register failed!");
                return field;
            }
            if (field.serializedProperty == null) {
                Debug.LogError("Serialized property is null, register failed!");
                return field;                
            }
            modifiedFields[field.serializedProperty.name] = field;
            return field;
        }
        protected DrawField[] Simplify(bool enabled = false, params string[] arrayNames) {
            return Array.ConvertAll(arrayNames, name => {
                var array = Modify(name);
                array.DrawAs(() => SimplifiedArrayPropertyField(array.serializedProperty, enabled));
                return array;
            });
        }
        protected DrawField Simplify(string arrayFieldName,  bool enabled = false, string label = null, Action inTitle = null) {
            DrawField arrayField = new DrawField(arrayFieldName, this);
            arrayField.Draw = ()=> SimplifiedArrayPropertyField(arrayField.serializedProperty, enabled, label, inTitle);
            RegisterMod(arrayField);
            return arrayField;
        }
        public static void SimplifiedArrayPropertyField(SerializedProperty prop,  Action inTitle = null) {
            SimplifiedArrayPropertyField(prop, false, null, inTitle);
        }
        public static void SimplifiedArrayPropertyField(SerializedProperty prop, string label = null, Action inTitle = null) {
            SimplifiedArrayPropertyField(prop, false, label, inTitle);
        }
        //******************** Need to Recheck
        public static void SimplifiedArrayPropertyField(SerializedProperty prop, bool enabled = false, string label = null, Action inTitle = null)
        {
            Action enabledTitle = ()=> {
                if (GUILayout.Button("Purge", GUILayout.Width(60))) {
                    for (int i=0; i<prop.arraySize; i++) {
                    var itemProp = prop.GetArrayElementAtIndex(i);
                    if (itemProp.objectReferenceValue == null) prop.DeleteArrayElementAtIndex(i);
                    }
                }
                if (GUILayout.Button("Clear", GUILayout.Width(60))) {
                    prop.ClearArray();
                }
                if (GUILayout.Button( (Texture)Resources.Load("plus"), GUILayout.Width(20) )) {
                    prop.InsertArrayElementAtIndex(0);
                }
                GUILayout.Space(23);
            };
            Action<int> enabledItem = i =>{
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(prop.GetArrayElementAtIndex(i));
                if (GUILayout.Button( (Texture)Resources.Load("plus"), GUILayout.Width(20) )) {
                    prop.InsertArrayElementAtIndex(i);
                }
                if (GUILayout.Button( (Texture)Resources.Load("minus"), GUILayout.Width(20) )) {
                    prop.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            };

            GUILayout.BeginHorizontal();
            string labelText = string.IsNullOrEmpty(label)? prop.name : label;
            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, labelText + ": " + prop.arraySize.ToString(), true);
            GUILayout.FlexibleSpace();
            if (inTitle != null) inTitle();
            if (enabled) enabledTitle();
            GUILayout.EndHorizontal();
            if (prop.isExpanded) {            
                if (!enabled) GUI.enabled = false;
                EditorGUI.indentLevel ++;
                for (int i=0; i<prop.arraySize; i++) {
                    if (enabled) enabledItem(i);
                    else EditorGUILayout.PropertyField(prop.GetArrayElementAtIndex(i));
                }
                EditorGUI.indentLevel --;
                if (prop.arraySize > 0) EditorGUILayout.Space(16);
                if (!enabled) GUI.enabled = true;
            }
        }
        public static void ButtonRow(params Button[] buttons) {
            if (buttons.Length < 1) return;
            // foreach(var button in buttons) totalWeight += button.weight;
            EditorGUILayout.BeginHorizontal();
            foreach (var button in buttons) {
                button.Draw();
            }            
            EditorGUILayout.EndHorizontal();            
        }
        // public static void ButtonRow(float width, params Button[] buttons) {
        //     if (buttons.Length < 1) return;
        //     float totalWeight = 0f;
        //     foreach(var button in buttons) totalWeight += button.weight;
        //     EditorGUILayout.BeginHorizontal();
        //     foreach (var button in buttons) {
        //         button.width = (int)(button.weight/totalWeight * width);
        //         button.Draw();
        //     }
        //     EditorGUILayout.EndHorizontal();
        // }
        // public static void ButtonRow(float width, params Button[] buttons) {
        //     if (buttons.Length < 1) return;
        //     float totalWeight = 0f;
        //     foreach(var button in buttons) totalWeight += button.weight;
        //     EditorGUILayout.BeginHorizontal();
        //     foreach (var button in buttons) {
        //         button.width = (int)(button.weight/totalWeight * width);
        //         button.Draw();
        //     }
        //     EditorGUILayout.EndHorizontal();            
        // }
        public static void ButtonRow(int leftSpace, int rightSpace , params Button[] buttons) {
            if (buttons.Length < 1) return;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(leftSpace);
            foreach (var button in buttons) {
                button.Draw();
            }
            EditorGUILayout.Space(rightSpace);
            EditorGUILayout.EndHorizontal();
        }
        public static void DrawLine(int height = 1) {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.3f));
        }
        public static void DrawLine(Color color, int height = 1) {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, color);
        }
        public static void AddSpace() {
            EditorGUILayout.Space();
        }
        public static void AddSpace(int amount) {
            EditorGUILayout.Space(amount);
        }
        internal Stack<int> RelativeIndent = new Stack<int>();
        public void BeginIndent(int level = 1) {
            EditorGUI.indentLevel += level;
            RelativeIndent.Push(level);
        }
        public void EndIndent() {
            try { EditorGUI.indentLevel -= RelativeIndent.Pop(); }
            catch (InvalidOperationException e) {
                Debug.LogException(e, this);
                Debug.LogError("EndIndent() must be called in pair with BeginIndent() before it.");
            }
        }
        internal int SingleLineHeight => Mathf.RoundToInt(EditorGUIUtility.singleLineHeight);
        protected DrawField CheckChange(string fieldName, Action onValueChange) {
            return CheckChange(serializedObject.FindProperty(fieldName), onValueChange);
        }
        protected DrawField CheckChange(SerializedProperty prop, Action onValueChange) {
            DrawField field = new DrawField(prop, this);
            var _Draw = field.Draw;
            field.Draw = ()=>{
            EditorGUI.BeginChangeCheck();
                _Draw();
                if (EditorGUI.EndChangeCheck()) {
                    onValueChange();        
                }
            };
            RegisterMod(field);
            return field;
        }
        protected static void OnButtonLeft(string buttonLabel, int buttonWidth, Action onClick, Action content) {
            EditorGUILayout.BeginHorizontal();
            content();
            if ( GUILayout.Button(buttonLabel, GUILayout.Width(buttonWidth)) ) onClick();
            EditorGUILayout.EndHorizontal();
        }   
        protected static void OnButtonLeft(Texture buttonIcon, int buttonWidth, Action onClick, Action content) {
            EditorGUILayout.BeginHorizontal();
            content();
            if ( GUILayout.Button(buttonIcon, GUILayout.Width(buttonWidth)) ) onClick();
            EditorGUILayout.EndHorizontal();
        }
        protected DrawField FieldContent(string fieldName, Type editorType = null) {
            DrawField field = new DrawField(fieldName, this);
            if (field.serializedProperty == null) Debug.LogError($"Cannot find field '{fieldName}' !");
            field.Draw = ()=> FieldContent(field.serializedProperty, ref field.cachedEditor, editorType);
            RegisterMod(field);
            return field;
        }
        public void FieldContent(SerializedProperty prop, ref Editor previousEditor, Type editorType = null) {
            if (prop == null) {
                Debug.LogError("Cannot find SerializedProperty!");
                return;
            }
            var value = prop.objectReferenceValue;
            if (!value) return;
            Editor.CreateCachedEditor(value, editorType, ref previousEditor);
            if (previousEditor) previousEditor.OnInspectorGUI();
        }
        public void ObjectContent(UnityEngine.Object obj, ref Editor previousEditor, Type editorType = null) {
            if (obj == null) return;
            Editor.CreateCachedEditor(obj, editorType, ref previousEditor);
            if (previousEditor) previousEditor.OnInspectorGUI();
        }
        public SerializedProperty FieldContentFolded(SerializedProperty prop, ref bool foldout, ref Editor previousEditor, Type editorType = null, string title = null) {
            foldout = EditorGUILayout.Foldout(foldout, 
                string.IsNullOrEmpty(title)? prop.name : title, true);
            if (foldout) {
                EditorGUI.indentLevel++;
                FieldContent(prop, ref previousEditor, editorType);
                EditorGUI.indentLevel--;
            }
            return prop;
        }
        public void ObjectContentFolded(UnityEngine.Object obj, ref bool foldout, ref Editor previousEditor, Type editorType = null, string title = null) {
            foldout = EditorGUILayout.Foldout(foldout, 
                string.IsNullOrEmpty(title)? obj.name : title, true);
            if (foldout) {
                EditorGUI.indentLevel++;
                ObjectContent(obj, ref previousEditor, editorType);
                EditorGUI.indentLevel--;
            }
        }
        public static void ObjectContentFoldedBuiltin(UnityEngine.Object obj, ref Editor previousEditor, Type editorType = null) {
            if (obj == null) { return; }
            Editor.CreateCachedEditor(obj, editorType, ref previousEditor);
            if (previousEditor == null) Debug.LogError("previousEditor is null!");
            Editor.DrawFoldoutInspector(obj, ref previousEditor);
        }
        public DrawField UnfoldSelfBelow(string propName, ref bool foldout, bool disableSelf = false, Type editorType = null) {
            return UnfoldSelfBelow(serializedObject.FindProperty(propName), ref foldout, disableSelf, editorType);
        }
        public  DrawField UnfoldSelfBelow(SerializedProperty prop, ref bool foldout, bool disableSelf = false, Type editorType = null) {
            DrawField field = Modify(prop);
            UnfoldBelow(prop, ref foldout, ()=>{
                EditorGUI.indentLevel++;
                FieldContent(prop, ref field.cachedEditor, editorType);
                EditorGUI.indentLevel--;
            }, disableSelf);
            return field;
        }
        // private 
        public DrawField UnfoldSelfBelowBuiltin(string fieldName, Type editorType = null, bool disableSelf = false) {
            currentProp = serializedObject.FindProperty(fieldName);
            if (currentProp == null) Debug.LogError($"Cannot Find Serialized Property '{fieldName}'"); 
            return UnfoldSelfBelowBuiltin(currentProp, editorType, disableSelf);
        }
        public DrawField UnfoldSelfBelowBuiltin(SerializedProperty prop, Type editorType = null, bool disableSelf = false) {
            var field = new DrawField(prop, this);
            var _Draw = field.Draw;
            field.Draw = () => {
                _Draw();
                if (disableSelf) GUI.enabled = false;
                ObjectContentFoldedBuiltin(prop.objectReferenceValue, ref field.cachedEditor, editorType);
                if (disableSelf) GUI.enabled = true;

            };
            RegisterMod(field);
            return field;
        }
        public  SerializedProperty UnfoldBelow(SerializedProperty prop, ref bool foldout, Action content, bool disableField = false) {
            if (disableField) {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(prop);
                GUI.enabled = true;
            }
            else EditorGUILayout.PropertyField(prop);
            Rect last = GUILayoutUtility.GetLastRect();
            foldout = EditorGUI.Foldout(last, foldout, GUIContent.none, true);
            if (foldout) content();
            return prop;
        }
        protected DrawField[] Disable(params string[] fieldNames) {
            return Array.ConvertAll(fieldNames, name => Modify(name).Disable());
        }
        protected DrawField Disable(string fieldName) {
            return Modify(fieldName).Disable();
        }
        protected DrawField Hide(string fieldName) {
            return Modify(fieldName).Hide();            
        }
        protected DrawField[] Hide(params string[] fieldNames) {
            return Array.ConvertAll(fieldNames, name => Modify(name).Hide());            
        }
        protected DrawField Hide(SerializedProperty prop) {
            return Modify(prop).Hide();
        }
        public static void DisabledSpan(Action action) {
            GUI.enabled = false;
            action();
            GUI.enabled = true;        
        }
        protected DrawField[] Indent(int level, params string[] fieldNames) {
            return Array.ConvertAll(fieldNames, name => {
                var field = Modify(name);
                return field.DrawAs( ()=>IndentedField(field.serializedProperty, level));
            });    
        }
        protected DrawField Indent( string fieldName, int indentLevel, params GUILayoutOption[] options) {
            DrawField field = new DrawField(fieldName, this);
            if (field.serializedProperty == null) Debug.LogError($"Cannot find field '{fieldName}' !");
            field.Draw = ()=> IndentedField(field.serializedProperty, indentLevel, options);
            RegisterMod(field);
            return field;
        }
        protected static int IndentedField(SerializedProperty prop, int indentLevel = 1, params GUILayoutOption[] options) {
            EditorGUI.indentLevel += indentLevel;
            EditorGUILayout.PropertyField(prop);
            EditorGUI.indentLevel -= indentLevel;
            return EditorGUI.indentLevel;
        }
        public static int IndentedSpan(Action action, int indentLevel = 1) {
            EditorGUI.indentLevel += indentLevel;
            action();
            EditorGUI.indentLevel -= indentLevel;
            return EditorGUI.indentLevel;
        }
        public static Rect HorizontalSpan(Action content, params GUILayoutOption[] options) {
            Rect start = EditorGUILayout.BeginHorizontal(options);
            content();
            EditorGUILayout.EndHorizontal();
            return start;
        }
        protected void CreateFoldoutGroup(string label, string start, string end, bool defaultState = true){
            if (string.IsNullOrEmpty(label)) {
                Debug.LogError("Empty string as Foldout Group label is not supported!");
                return;
            }
            var startField = Modify(start);
            startField.beginFoldoutFlag = true;
            startField.foldoutLabel = label;
            startField.defaultfoldoutState = defaultState;
            Modify(end).endFoldoutFlag = true;

            if (!foldoutGroupStates.TryGetValue(label, out var storedValue)) {
                foldoutGroupStates[label] = defaultState;
            }
        }
        public static T DrawObject<T>(T obj, params GUILayoutOption[] options) {
            if (obj is Bounds) {
                Bounds _obj = obj as Bounds? ?? default(Bounds);
                EditorGUILayout.BoundsField( _obj, options);           
            }
            else if (obj is BoundsInt) {
                BoundsInt _obj = obj as BoundsInt? ?? default(BoundsInt);
                EditorGUILayout.BoundsIntField( _obj, options);
            }
            else if (obj is Color) {
                Color _obj = obj as Color? ?? default(Color);
                EditorGUILayout.ColorField( _obj, options);
            }
            else if (obj is AnimationCurve) {
                EditorGUILayout.CurveField( obj as AnimationCurve, options);
            }        
            else if (obj is double) {
                double _obj = obj as double? ?? default(double);
                EditorGUILayout.DoubleField( _obj, options);
            }
            else if (obj is Enum) {
                EditorGUILayout.EnumFlagsField( obj as Enum, options);
            }
            else if (obj is float) {
                float _obj = obj as float? ?? default(float);
                EditorGUILayout.FloatField( _obj, options);
            }
            else if (obj is Gradient) {
                EditorGUILayout.GradientField( obj as Gradient, options);
            }
            else if (obj is int) {
                int _obj = obj as int? ?? default(int);
                EditorGUILayout.IntField( _obj, options);
            }   
            else if (obj is long) {
                long _obj = obj as long? ?? default(long);
                EditorGUILayout.LongField( _obj, options);
            }
            else if (obj is UnityEngine.Object) {
                EditorGUILayout.ObjectField( obj as UnityEngine.Object, typeof(T), true, options);
            }
            else {
                EditorGUILayout.LabelField(obj.ToString());
            }
            return obj;
        }
        public static T DrawDisabledObject<T>(T obj, params GUILayoutOption[] options) {
            GUI.enabled = false;
            DrawObject(obj, options);
            GUI.enabled = true;
            return obj;
        }
        public static T DrawObject<T>(string label, T obj, params GUILayoutOption[] options) 
        {                
            if (obj is Bounds) {
                Bounds _obj = obj as Bounds? ?? default(Bounds);
                EditorGUILayout.BoundsField(label, _obj, options);           
            }
            else if (obj is BoundsInt) {
                BoundsInt _obj = obj as BoundsInt? ?? default(BoundsInt);
                EditorGUILayout.BoundsIntField(label, _obj, options);
            }
            else if (obj is Color) {
                Color _obj = obj as Color? ?? default(Color);
                EditorGUILayout.ColorField(label, _obj, options);
            }
            else if (obj is AnimationCurve) {
                EditorGUILayout.CurveField(label, obj as AnimationCurve, options);
            }        
            else if (obj is double) {
                double _obj = obj as double? ?? default(double);
                EditorGUILayout.DoubleField(label, _obj, options);
            }
            else if (obj is Enum) {
                EditorGUILayout.EnumFlagsField(label, obj as Enum, options);
            }
            else if (obj is float) {
                float _obj = obj as float? ?? default(float);
                EditorGUILayout.FloatField(label, _obj, options);
            }
            else if (obj is Gradient) {
                EditorGUILayout.GradientField(label, obj as Gradient, options);
            }
            else if (obj is int) {
                int _obj = obj as int? ?? default(int);
                EditorGUILayout.IntField(label, _obj, options);
            }   
            else if (obj is long) {
                long _obj = obj as long? ?? default(long);
                EditorGUILayout.LongField(label, _obj, options);
            }
            else if (obj is  Vector2) {
                Vector2 _obj = obj as Vector2? ?? default(Vector2);
                EditorGUILayout.Vector2Field(label, _obj, options);
            }
            else if (obj is  Vector2Int) {
                Vector2Int _obj = obj as Vector2Int? ?? default(Vector2Int);
                EditorGUILayout.Vector2IntField(label, _obj, options);
            }
            else if (obj is  Vector3) {
                Vector3 _obj = obj as Vector3? ?? default(Vector3);
                EditorGUILayout.Vector3Field(label, _obj, options);
            }
            else if (obj is  Vector3Int) {
                Vector3Int _obj = obj as Vector3Int? ?? default(Vector3Int);
                EditorGUILayout.Vector3IntField(label, _obj, options);
            }
            else if (obj is  Vector4) {
                Vector4 _obj = obj as Vector4? ?? default(Vector4);
                EditorGUILayout.Vector4Field(label, _obj, options);
            }
            else if (obj is UnityEngine.Object) {
                EditorGUILayout.ObjectField(label, obj as UnityEngine.Object, typeof(T), true, options);
            }
            else {
                EditorGUILayout.LabelField(obj.ToString());
            }
            return obj;
        }
        public static T DrawDisabledObject<T>(string label, T obj, params GUILayoutOption[] options) {
            GUI.enabled = false;
            DrawObject(label, obj, options);
            GUI.enabled = true;
            return obj;
        }
        public static IReadOnlyList<TItem> SimplifiedArrayField<TItem>(IReadOnlyList<TItem> array, ref bool foldout, string label, bool enabled = false, int indent = 1)
        {
            GUILayout.BeginHorizontal();
            foldout = EditorGUILayout.Foldout(foldout, label + ": " + array.Count.ToString(), true);
            GUILayout.EndHorizontal();
            if (!enabled) GUI.enabled = false;
            if (foldout) {
                EditorGUI.indentLevel += indent;
                for (int i=0; i<array.Count; i++) {
                    DrawObject(array[i]);
                }
                EditorGUI.indentLevel -= indent;
                EditorGUILayout.Space(16);
            }
            if(!enabled) GUI.enabled = true;
            return array;
        }
        #region Display HashSet in Inspector
        public void HashSetField<T>(HashSet<T> set, string label, bool defaultState = true, Action inHeaderLine = null){
            if (set == null) {
                EditorGUILayout.LabelField(label + ": null");
                return;
            }
            //Retrieve states
            if (string.IsNullOrEmpty(label)) {
                Debug.LogError("Empty string as HashSet Field label is not supported!");
                return;                
            }
            if (!scrollStates.TryGetValue(label, out var storedScroll)) {
                scrollStates[label] = default;
            }
            if (!foldoutStates.TryGetValue(label, out var storedFoldout)) {
                foldoutStates[label] = defaultState;
            }            
            //Header
            EditorGUILayout.BeginHorizontal();
            foldoutStates[label] = EditorGUILayout.Foldout(foldoutStates[label], label + ": " + set.Count.ToString(), true);
            if(inHeaderLine != null) inHeaderLine();
            EditorGUILayout.EndHorizontal();
            //Body
            if(foldoutStates[label]) {
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight / 2);
                EditorGUI.indentLevel++;
                scrollStates[label] = EditorGUILayout.BeginScrollView(scrollStates[label]);
                GUI.enabled = false;
                foreach(var item in set) {
                    DrawObject(item);
                }
                EditorGUILayout.EndScrollView();
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }
        }
        public static void HashSetField<T>(HashSet<T> set, ref bool foldout, ref Vector2 scrollPos, string label, Action inHeaderLine = null) {
            if (set == null) {
                EditorGUILayout.LabelField(label + ": null");
                return;
            }
            DrawHashSetHeader(set, ref foldout, label, inHeaderLine);
            if(foldout) {
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight / 2);
                EditorGUI.indentLevel++;
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                GUI.enabled = false;
                foreach(var item in set) {
                    DrawObject(item);
                }
                EditorGUILayout.EndScrollView();
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }
        }
        public static void HashSetField<T>(HashSet<T> set, ref bool foldout, string label, Action inHeaderline = null) {
            DrawHashSetHeader(set, ref foldout, label, inHeaderline);
            if(foldout) {
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight / 2);
                EditorGUI.indentLevel++;
                GUI.enabled = false;
                foreach(var item in set) {
                    DrawObject(item);
                }
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }
        }
        static void DrawHashSetHeader<T>(HashSet<T> set, ref bool foldout, string label, Action inHeaderLine = null) {
            EditorGUILayout.BeginHorizontal();
            foldout = EditorGUILayout.Foldout(foldout, label + ": " + set.Count.ToString(), true);
            if(inHeaderLine != null) inHeaderLine();
            EditorGUILayout.EndHorizontal();
        }
#endregion
#region Display Dictionary in Inspector
        public static void DictionaryField<TKey, TValue>(Dictionary<TKey, TValue> dict, ref bool foldout, ref Vector2 scrollPos, string label, 
        string keyLabel = "Key", string valueLabel = "Value", Action inTitle = null) 
        {
            DrawDictionaryTitle(dict, ref foldout, label, inTitle);
            if (foldout) {
                EditorGUI.indentLevel ++;
                DrawKeyValueTitle(keyLabel, valueLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                DrawKeyValuePairs(dict);             
                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel --;
            }
        }
        public static void DictionaryField<TKey, TValue>(Dictionary<TKey, TValue> dict, ref bool foldout, string label, 
        string keyLabel = "Key", string valueLabel = "Value", Action inTitle = null) 
        {
            DrawDictionaryTitle(dict, ref foldout, label, inTitle);
            if (foldout) {
                EditorGUI.indentLevel ++;
                DrawKeyValueTitle(keyLabel, valueLabel);
                DrawKeyValuePairs(dict);             
                EditorGUI.indentLevel --;
            }
        }
        static void DrawDictionaryTitle<TKey, TValue> (Dictionary<TKey, TValue> dict, ref bool foldout, string label, Action inTitle = null) {
            EditorGUILayout.BeginHorizontal();
            foldout = EditorGUILayout.Foldout(foldout, label + ": " + dict.Count.ToString(), true);
            if (inTitle != null) inTitle();
            EditorGUILayout.EndHorizontal();
        }
        static void DrawKeyValueTitle(string keyLabel = "Key", string valueLabel = "Value") {
            if (keyLabel.Length + valueLabel.Length < 1) return;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(keyLabel);
            EditorGUILayout.LabelField(valueLabel);        
            EditorGUILayout.EndHorizontal();
        }

        static void DrawKeyValuePairs<TKey, TValue> (Dictionary<TKey, TValue> dict) {
            GUI.enabled = false;
            foreach( KeyValuePair<TKey, TValue> item in dict) {
                GUILayout.BeginHorizontal();
                DrawObject(item.Key);
                DrawObject(item.Value);
                GUILayout.EndHorizontal();
            }            
            GUI.enabled = true;                
        }
        public static Dictionary<TKey, List<TValue>> DictionaryArrayField<TKey, TValue>(Dictionary<TKey, List<TValue>> dict, ref bool foldout,
        bool[] valueFoldout, string label, string keyLabel = "Key", string valueLabel = "Value", Action inTitle = null)
        {      
            GUIStyle keyLabelStyle = new GUIStyle();
            keyLabelStyle.clipping = TextClipping.Overflow;

            GUIStyle subItemIndexStyle = new GUIStyle();
            subItemIndexStyle.clipping = TextClipping.Overflow;
            subItemIndexStyle.alignment = TextAnchor.MiddleRight;
            subItemIndexStyle.contentOffset = new Vector2(20f,0f);

            EditorGUILayout.BeginHorizontal();
            foldout = EditorGUILayout.Foldout(foldout, label + ": " + dict.Count.ToString(), true);
            if (inTitle != null) inTitle();        
            EditorGUILayout.EndHorizontal();
            if (foldout) {
                EditorGUI.indentLevel ++;
                IDictionaryEnumerator item = dict.GetEnumerator();
                for (int i=0; i<dict.Count; i++) {
                    if (item.MoveNext()) {
                        //item block
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(keyLabel + " " + i + ":", 
                            keyLabelStyle, GUILayout.Width(Screen.width/9));
                        DrawDisabledObject( item.Key );
                        //value column
                        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width/2));
                        //1st line (value title)
                        valueFoldout[i] = EditorGUILayout.Foldout(valueFoldout[i], valueLabel + ": " + (item.Value as List<TValue>).Count.ToString(), true);
                        //2nd to last lines (value content)
                        if (valueFoldout[i]) {
                            List<TValue> valueList = item.Value as List<TValue>;
                            for (int j=0; j<valueList.Count; j++) {
                                EditorGUILayout.BeginHorizontal();
                                GUI.enabled = false;
                                //index
                                EditorGUILayout.LabelField(j.ToString(), 
                                    subItemIndexStyle, GUILayout.Width(20));                            
                                //item
                                DrawObject(valueList[j]);
                                GUI.enabled = true;
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.Space(16);
                        }
                        //End value colom
                        EditorGUILayout.EndVertical();
                        //End item block
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUI.indentLevel --;
                EditorGUILayout.Space(16);
            }      
            return dict;
        }
#endregion
    }
}
#endif