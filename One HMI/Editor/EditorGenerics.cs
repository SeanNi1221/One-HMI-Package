#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Reflection;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.UIElements;

namespace Sean21.OneHMI
{
    public static class EditorGenerics {
        #region Paths
        public const string RuntimeFolder = "Packages/com.sean21.one-hmi/One HMI/";
        public const string EditorFolder = "Packages/com.sean21.one-hmi/One HMI/Editor/";
        public const string EditorIconFolder = "Packages/com.sean21.one-hmi/One HMI/Editor/Icons/";
        public const string DefaultActionsPath = "Packages/com.sean21.one-hmi/One HMI/Default Settings/HMI Input Actions.inputactions";
        public const string DefaultNodeIconFolderPath = "Packages/com.sean21.one-hmi/One HMI/Icons/object.png";
        public const string DefaultSchemaPath = "Packages/com.sean21.one-hmi/One HMI/Default Settings/Schema.asset";
        public const string DefaultInputManagerListenerPath = "Packages/com.sean21.one-hmi/One HMI/Default Settings/Input Manager Listener.asset";
        public const string DefaultControllerSettingPath = "Packages/com.sean21.one-hmi/One HMI/Default Settings/Controller Setting.asset";
        public const string DefaultConnectorPath = "Assets/One HMI/MySQL Connector.asset";
        #endregion

        #region AssetManagement
        /// <summary>
        /// If <parmaref name="target"/> is <c>null</c>，try to load from <paramref name="path"/> then assign, otherwise return<c>true</c>。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">Asset path</param>
        /// <param name="target">Reference target object</param>
        /// <returns><c>true</c>:<parmaref name="target"/> is not <c>null</c></returns>
        public static bool LoadAssetIfNull<T>(string path, ref T target) where T : UnityEngine.Object {
            if (target != null) return true;
            return LoadAsset(path, ref target);
        }
        /// <summary>
        /// If loading succeeded, assign <parmaref name="target"/> and return <c>true</c>, otherwise keep <parmaref name="target"/> untouched and return <c>false</c>
        /// </summary>
        /// <param name="path">Asset path</param>
        /// <param name="target">Reference target object</param>
        /// <returns></returns>
        public static bool LoadAsset<T>(string path, ref T target) where T : UnityEngine.Object {
            T _target = AssetDatabase.LoadAssetAtPath<T>(path);
            if (_target == null) return false;
            target = _target;
            return true;
        }
        public static T LoadAsset<T>(string path) where T : UnityEngine.Object {
            return (AssetDatabase.LoadAssetAtPath<T>(path));
        }
        public static Texture2D LoadIcon(string relativePath) {
            return LoadAsset<Texture2D>(EditorIconFolder + relativePath);
        }
        ///<summary>Get the addressable path of <paramref name = "asset"/> and pass to <paramref name = "path"/>.</summary>
        ///<returns>true if succeeded</returns>
        public static bool GetAddressablePath(UnityEngine.Object asset, out string path) {
            path = null;
            if (asset == null) return false;
            //Try get Asset Path by Object
            string _assetPath = AssetDatabase.GetAssetPath(asset);
            //Try get Asset Path by Prefab Root
            if (string.IsNullOrEmpty(_assetPath)) _assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(asset);
            if (string.IsNullOrEmpty(_assetPath)) return false;

            //Try get Addressable Path upward by Asset Path
            string[] levels = _assetPath.Split('/');
            string _assetParentPath = _assetPath;
            string _assetRelativePath = null;
            for (int i = levels.Length; i > 0; i--) {
                path = GetAddressablePathByAssetPath(_assetParentPath);
                if (!string.IsNullOrEmpty(path)) {
                    path = string.IsNullOrEmpty(_assetRelativePath) ? path : path + "/" + _assetRelativePath;
                    break;
                }
                string[] parentLevels = new string[i];
                Array.Copy(levels, parentLevels, i);
                _assetParentPath = string.Join("/", parentLevels);

                string[] relLevels = new string[levels.Length - i];
                Array.Copy(levels, i, relLevels, 0, levels.Length - i);
                _assetRelativePath = string.Join("/", relLevels);

                // Debug.Log(_assetParentPath + "     " + _assetRelPath);
            }
            //Add Sub Asset prefix
            if (!string.IsNullOrEmpty(path) && !AssetDatabase.IsMainAsset(asset) && !(asset is GameObject))
                path = path + "[" + asset.name + "]";
            return true;
        }
        ///<summary>Get Addressable Path by Asset Path</summary>
        public static string GetAddressablePathByAssetPath(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return null;
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid)) return null;
            AddressableAssetSettings addrSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (!addrSettings) return null;
            AddressableAssetEntry entry = addrSettings.FindAssetEntry(guid);
            if (entry == null) return null;
            return entry.AssetPath;
        }
        public static T LoadOrCreate<T>(string path, bool overwriteExisting = false) where T : ScriptableObject {
            T asset;
            //Already existing
            if (!overwriteExisting) {
                asset = LoadAsset<T>(path);
                if (asset != null) return asset;
            }
            //Create and return
            string[] hierarchy = path.Split('/');
            //Validate folder recursively, create if not existing
            for (int i = 0; i < hierarchy.Length - 1; i++) {
                //eg. Assets/parent/current
                string currentPath = string.Join('/', hierarchy, 0, i + 1);
                //eg. Assets/parent
                string parentPath = string.Join('/', hierarchy, 0, i);
                //eg. current
                string currentFolder = hierarchy[i];
                
                if (!AssetDatabase.IsValidFolder(currentPath)) {
                    AssetDatabase.CreateFolder(parentPath, currentFolder);
                }
            }
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
            asset = LoadAsset<T>(path);
            return asset;
        }
        #endregion
        ///<summary>
        ///Get the associated Editor instance(known type) from an object.
        ///</summary>
        public static T GetEditor<T>(this UnityEngine.Object obj)
        where T : Editor {
            // string editorTypeName = obj.GetType().Name + "Editor";
            // Debug.Log($"Editor count: {Resources.FindObjectsOfTypeAll<T>().Length}");
            foreach (var ed in Resources.FindObjectsOfTypeAll<T>())
                // if (ed.GetType() == typeof(T))
                if (ed.target == obj)
                    return ed;
            return null;
        }
        ///<summary>
        ///Get the associated Editor instance(unknown type) from an object.
        ///</summary>
        public static Editor GetEditor(this UnityEngine.Object obj) {
            // string editorTypeName = obj.GetType().Name + "Editor";

            return obj.GetEditor<Editor>();
        }
        ///<summary>
        ///Invoke arbitrary method in arbitrary class, including non-public.
        ///</summary>
        public static void InvokeVoid(this object targetObject, string methodName) {
            targetObject.GetType().GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            ).Invoke(targetObject, null);
        }
        public static void SetFieldValue(this object targetObject, string fieldName, object value) {
            targetObject.GetType().GetField(
                fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            ).SetValue(targetObject, value);
        }
        public static T GetFieldValue<T>(this object targetObject, string fieldName) {
            object value = targetObject.GetType().GetField(
                fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            ).GetValue(targetObject);
            return (T)value;
        }
        public static SerializedProperty GetSerializedChild(this SerializedProperty prop, string childName) {
            var child = new SerializedObject(prop.objectReferenceValue).FindProperty(childName);
            if (child == null) Debug.LogError("Cannot find child property " + childName);
            return child;
        }
        /// <summary>
        /// Set Layer <paramref name="layer"/>'s name to <paramref name="layerName"/>。
        /// </summary>
        /// <param name="forceOverwrite">Overwrite the name if existing</param>
        /// <returns>Whether the name was changed</returns>
        public static bool SetLayerName(int layer, string layerName, bool forceOverwrite = false) {
            if (layer < 8 || layer > 31 ) {
                return false;
            }
            var layerProp = GetLayerProp(layer, out var tagManager);
            if (!string.IsNullOrEmpty(layerProp.stringValue)) {
                if (!forceOverwrite) {
                    if (layerProp.stringValue != layerName) {
                        Debug.LogWarning($"Attempt modifying Layer {layer} failed because layer name '{layerProp.stringValue}' has been taken.");
                    }
                    return false;
                }
                if (layerProp.stringValue != layerName) {
                    Debug.LogWarning($"Changed Layer {layer} from '{layerProp.stringValue}' to '{layerName}'.");
                }
            } else {
                Debug.Log($"Set Layer {layer} to '{layerName}'.");
            }
            layerProp.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            return true;
        }
        /// <summary>
        /// Clear Layer <paramref name="layer"/>'s name.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns>Whether target name is changed</returns>
        public static bool ClearLayerName(int layer, string nameToClear = null) {
            if (layer < 8 || layer > 31 ) {
                // Debug.LogError($"Unable to Change Layer {layer}'s name, available layers are Layer 8 ~ 31");
                return false;
            }
            var layerProp = GetLayerProp(layer, out var tagManager);
            if (string.IsNullOrEmpty(layerProp.stringValue)) return false;
            if (string.IsNullOrEmpty(nameToClear) || layerProp.stringValue == nameToClear) {
                Debug.LogWarning($"Cleared Layer {layer} '{layerProp.stringValue}'.");
                layerProp.stringValue = null;
                tagManager.ApplyModifiedProperties();
                return true;
            }
            Debug.Log($"Layer {layer}'s name is not cleared because it's current name '{layerProp.stringValue}' does not match target name '{nameToClear}'.");
            return false;
        }
        private static SerializedProperty GetLayerProp(int layerIndex, out SerializedObject tagManagerHandler) {
            tagManagerHandler = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManagerHandler.FindProperty("layers");
            return layersProp.GetArrayElementAtIndex(layerIndex);
        }
    }
}

#endif