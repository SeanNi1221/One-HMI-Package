#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System;
namespace Sean21.OneHMI
{
    public class DependencyManagerWindow : EditorWindow {
        /// <summary><paramref name="TKey"/>: url,  <paramref name="TValue"/>: display name</summary>
        public static readonly Dictionary<string, string> installed = new Dictionary<string, string>();
        /// <summary><paramref name="TKey"/>: url,  <paramref name="TValue"/>: display name</summary>
        public static readonly Dictionary<string, string> absent = new Dictionary<string, string>();
        const string menuPath = "One HMI/Dependency Manager";
        static string accessTip = $"This window can be opened by Menu - {menuPath}.";
        const string doneTip = "All dependent packagges installed.";
        const string installedTip = "Installed packages: ";
        const string absentTip = " Absent packages: ";
        const string installButtonTip = "Install Absent Packages";
        const string cancelButtonTip = "Cancel";
        static AddRequest Request;
        static DependencyManagerWindow window;
        bool allDone => absent.Count < 1;
        [MenuItem(menuPath)]
        public static void Open() {
            window = GetWindow<DependencyManagerWindow>();
            window.minSize = new Vector2(400, 400);
            window.titleContent = new GUIContent("One HMI Dependency Manager");
            window.ShowUtility();
        }
        void OnInspectorUpdate()
        {
            Repaint();
        }
        void OnGUI() {
            // Debug.Log($"installed:{installed.Count}");
            EditorGUILayout.HelpBox(installedTip, MessageType.Info, true);
            EditorGUI.indentLevel++;
            foreach(var item in installed)
                EditorGUILayout.SelectableLabel($"{item.Value} - {item.Key}", GUILayout.MinWidth(500));
            EditorGUI.indentLevel--;
            
            Action DrawAccessTip = () => { 
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(accessTip);
                EditorGUILayout.Space();                
            };

            if (allDone) goto draw_all_done;
            
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox(absentTip, MessageType.Error, true);
            EditorGUI.indentLevel++;
            foreach (var item in absent) {
                EditorGUILayout.SelectableLabel($"{item.Value} - {item.Key}", GUILayout.MinWidth(500));
            }
            EditorGUI.indentLevel--;
            DrawAccessTip();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(installButtonTip)){
                foreach(var item in absent)
                    InstallPackage(item.Key, item.Value);
            }
            if (GUILayout.Button(cancelButtonTip)) { 
                if (window) window.Close(); 
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            return;
            
            draw_all_done:
            EditorGUILayout.LabelField(doneTip);
            DrawAccessTip();
            if (GUILayout.Button("OK")) { 
                if (!window) window = GetWindow<DependencyManagerWindow>();
                window.Close(); 
            }                
        }

        // [DidReloadScripts]
        public static bool CheckDependency() {
            if (absent.Count<1) {
                return true;
            }
            Open();
            return false;
        }

        void InstallPackage(string url, string displayName = null) {
            if (!url.StartsWith("com.unity")) {
                EditorUtility.DisplayDialog("Custom package found!", $"Package {displayName} - {url} is not in Unity Registry, please install it with Unity Package Manager.\n" +
                "If it has been installed, make sure it is NOT under any directory of Assets, Library, ProjectSettings or UserSettings, and was installed with Unity Package Manager.", "OK");
                return;
            }
            Request = Client.Add(url);
            EditorApplication.update += InstallProgress;
        }
        static void InstallProgress() {
            if (Request.IsCompleted) {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);
                
                EditorApplication.update -= InstallProgress;
            }            
        }
    }
}
#endif