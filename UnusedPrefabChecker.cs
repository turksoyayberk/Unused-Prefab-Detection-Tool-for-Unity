using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using System.Text;

public class UnusedPrefabChecker : EditorWindow
{
    private readonly HashSet<Object> _selectedScenes = new();
    private readonly HashSet<string> _searchScenes = new();

    private readonly HashSet<Object> _selectedFolders = new();

    private readonly HashSet<GameObject> _userSelectedPrefabs = new();

    private readonly HashSet<string> _allPrefabPathsInAssets = new(); //Stores prefabs in the asset folder.

    private readonly HashSet<string>
        _allScriptablePathsInAssets = new(); //Stores ScriptableObjects in the asset folder.

    private readonly HashSet<string> _unusedPrefabs = new();

    private Vector2 _sceneScrollPosition; // Scroll for scene selection.
    private Vector2 _prefabScrollPosition; // Scroll for prefab selection.

    private bool _isSelectedScene;

    private string _searchFilter = "";

    [MenuItem("Tools/Unused Prefab Checker")]
    public static void UnusedPrefabWindow()
    {
        GetWindow<UnusedPrefabChecker>("Unused Prefab Checker");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Unused Prefab Checker", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        DrawPrefabSection();
        EditorGUILayout.Space(10);
        DrawFolderSection();
        EditorGUILayout.Space(10);
        DrawSceneSection();
        EditorGUILayout.Space(10);
        DrawActionsSection();
        if (GUILayout.Button("Check Prefabs"))
        {
            FindPrefabs();
        }

        EditorGUILayout.Space(10);
        UnusedPrefabsGui();
    }

    private void DrawPrefabSection()
    {
        GUILayout.Label("Selected Prefabs", EditorStyles.boldLabel);

        _prefabScrollPosition = EditorGUILayout.BeginScrollView(_prefabScrollPosition, GUILayout.Height(150));

        GameObject prefabToRemove = null;

        foreach (var prefab in new HashSet<GameObject>(_userSelectedPrefabs))
        {
            EditorGUILayout.BeginHorizontal();

            var updatedPrefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);

            if (updatedPrefab != prefab)
            {
                if (updatedPrefab != null && _userSelectedPrefabs.Contains(updatedPrefab))
                {
                    Debug.LogWarning($"Prefab '{updatedPrefab.name}' is already in the list!");
                }
                else
                {
                    _userSelectedPrefabs.Remove(prefab);
                    if (updatedPrefab != null)
                        _userSelectedPrefabs.Add(updatedPrefab);
                }
            }

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                prefabToRemove = prefab;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (prefabToRemove != null)
        {
            _userSelectedPrefabs.Remove(prefabToRemove);
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add New Prefab"))
        {
            if (!_userSelectedPrefabs.Contains(null))
            {
                _userSelectedPrefabs.Add(null);
            }
        }
    }

    private void DrawFolderSection()
    {
        GUILayout.Label("Selected Folders", EditorStyles.boldLabel);

        foreach (var folder in new HashSet<Object>(_selectedFolders))
        {
            EditorGUILayout.BeginHorizontal();
            var updatedFolder = EditorGUILayout.ObjectField(folder, typeof(DefaultAsset), false);
            if (updatedFolder != folder)
            {
                _selectedFolders.Remove(folder);
                if (updatedFolder != null)
                    _selectedFolders.Add(updatedFolder);
            }

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                _selectedFolders.Remove(folder);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add New Folder"))
        {
            _selectedFolders.Add(null);
        }

        if (GUILayout.Button("Load Prefabs from Folders"))
        {
            foreach (var folder in _selectedFolders)
            {
                if (folder != null && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(folder)))
                {
                    LoadPrefabsFromFolder(AssetDatabase.GetAssetPath(folder));
                }
            }
        }
    }

    private void DrawSceneSection()
    {
        GUILayout.Label("Selected Scenes", EditorStyles.boldLabel);

        List<Object> scenesToRemove = new List<Object>();
        List<Object> scenesToAdd = new List<Object>();

        foreach (var scene in _selectedScenes.ToList())
        {
            EditorGUILayout.BeginHorizontal();

            var updatedScene = EditorGUILayout.ObjectField(scene, typeof(SceneAsset), false);

            if (updatedScene != scene)
            {
                scenesToRemove.Add(scene);

                if (updatedScene != null)
                {
                    var scenePath = AssetDatabase.GetAssetPath(updatedScene);
                    _searchScenes.Add(scenePath);
                    scenesToAdd.Add(updatedScene);
                }
            }

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                scenesToRemove.Add(scene);
            }

            EditorGUILayout.EndHorizontal();
        }

        foreach (var scene in scenesToRemove)
        {
            _selectedScenes.Remove(scene);

            if (scene != null)
            {
                var scenePath = AssetDatabase.GetAssetPath(scene);
                _searchScenes.Remove(scenePath);
            }
        }

        foreach (var scene in scenesToAdd)
        {
            _selectedScenes.Add(scene);
        }

        if (GUILayout.Button("Add New Scene"))
        {
            _selectedScenes.Add(null);
        }
    }

    private void DrawActionsSection()
    {
        GUILayout.Label("Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Print Selected Prefabs"))
        {
            if (_userSelectedPrefabs.Count == 0)
            {
                Debug.LogWarning("No prefabs selected");
                return;
            }

            Debug.Log("Selected Prefabs:");
            foreach (var prefab in _userSelectedPrefabs)
            {
                Debug.Log(prefab != null ? prefab.name : "Empty Prefab!");
            }
        }

        if (GUILayout.Button("Print Selected Scenes"))
        {
            if (_selectedScenes.Count == 0)
            {
                Debug.LogWarning("No scenes selected");
                return;
            }

            Debug.Log("Selected Scenes:");
            foreach (var scene in _selectedScenes)
            {
                Debug.Log(scene != null ? scene : "Empty Scene!");
            }
        }

        if (GUILayout.Button("Clear All"))
        {
            _userSelectedPrefabs.Clear();
            _selectedFolders.Clear();
            _selectedScenes.Clear();
            _searchScenes.Clear();
            _unusedPrefabs.Clear();
        }

        if (GUILayout.Button("Export Results"))
        {
            var path = EditorUtility.SaveFilePanel(
                "Save Unused Prefabs Report",
                "",
                "UnusedPrefabsReport.txt",
                "txt");

            if (!string.IsNullOrEmpty(path))
            {
                var report = new StringBuilder();
                report.AppendLine($"Unused Prefabs Report - {DateTime.Now}");
                report.AppendLine("----------------------------------------");

                foreach (var prefab in _unusedPrefabs)
                {
                    report.AppendLine(prefab);
                }

                File.WriteAllText(path, report.ToString());
                Debug.Log($"Report saved to: {path}");
            }
        }
    }

    private void LoadPrefabsFromFolder(string folderPath)
    {
        string[] prefabPaths = Directory.GetFiles(folderPath, "*.prefab", SearchOption.AllDirectories);
        foreach (var prefabPath in prefabPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                if (!_userSelectedPrefabs.Contains(prefab))
                {
                    _userSelectedPrefabs.Add(prefab);
                }
                else
                {
                    Debug.LogWarning($"Prefab '{prefab.name}' is already in the list!");
                }
            }
        }
    }

    private void UnusedPrefabsGui()
    {
        GUILayout.Label($"Unused Prefabs ({_unusedPrefabs.Count})", EditorStyles.boldLabel);

        _searchFilter = EditorGUILayout.TextField("Filter", _searchFilter);

        _sceneScrollPosition = EditorGUILayout.BeginScrollView(_sceneScrollPosition, GUILayout.Height(150));

        if (_unusedPrefabs.Count > 0)
        {
            foreach (var prefab in _unusedPrefabs.Where(p => string.IsNullOrEmpty(_searchFilter) ||
                                                             p.ToLower().Contains(_searchFilter.ToLower())))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(prefab, EditorStyles.wordWrappedLabel);

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    var prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(
                        AssetDatabase.FindAssets($"t:prefab {prefab}")
                            .Select(AssetDatabase.GUIDToAssetPath)
                            .First(p => p.Contains(prefab)));
                    Selection.activeObject = prefabObject;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void FindPrefabs()
    {
        //Stopwatch stopwatch = new Stopwatch();
        //stopwatch.Start();

        if (_userSelectedPrefabs.Count == 0)
        {
            Debug.LogWarning("Prefab list is empty! Please add prefabs by pressing the 'Add Prefabs' button first.");
            return;
        }

        _isSelectedScene = _searchScenes.Count > 0;

        var totalPrefabs = _userSelectedPrefabs.Count;
        var processedPrefabs = 0;

        _unusedPrefabs.Clear();
        _allPrefabPathsInAssets.Clear();
        _allScriptablePathsInAssets.Clear();

        CacheAssetPaths();

        foreach (var prefab in _userSelectedPrefabs)
        {
            if (prefab == null) continue;

            EditorUtility.DisplayProgressBar("Checking Prefabs",
                $"Checking: {prefab.name} ({processedPrefabs}/{totalPrefabs})",
                (float)processedPrefabs / totalPrefabs);

            var assetPath = AssetDatabase.GetAssetPath(prefab);

            if (IsPrefabUsed(assetPath))
                continue;

            _unusedPrefabs.Add(prefab.name);
            processedPrefabs++;
        }

        EditorUtility.ClearProgressBar();

        //stopwatch.Stop();
        //float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;
        //Debug.Log($"The FindPrefabs process took {elapsedSeconds:F2} seconds.");
    }

    // Helper function to cache asset paths at once
    private void CacheAssetPaths()
    {
        _allPrefabPathsInAssets.UnionWith(
            AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath));

        _allScriptablePathsInAssets.UnionWith(
            AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath));
    }

    private bool IsPrefabUsed(string prefabPath)
    {
        return CheckAssetPrefabInOtherPrefabs(prefabPath) ||
               CheckAssetPrefabInOtherPrefabScripts(prefabPath) ||
               CheckPrefabInScriptableObjects(prefabPath) ||
               (_isSelectedScene && CheckPrefabInScenes(prefabPath));
    }

    private bool CheckAssetPrefabInOtherPrefabs(string prefabPath)
    {
        foreach (var path in _allPrefabPathsInAssets)
        {
            if (path == prefabPath) continue;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (Transform child in prefab.transform)
            {
                if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child.gameObject) == prefabPath)
                {
                    var selectedObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    Debug.Log($"Prefab '{selectedObject.name}' is used inside another prefab: {prefab.name}");
                    return true;
                }
            }
        }

        return false;
    }

    private bool CheckAssetPrefabInOtherPrefabScripts(string prefabPath)
    {
        foreach (var path in _allPrefabPathsInAssets)
        {
            if (path == prefabPath) continue; // Prevent self-matching

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            if (IsPrefabReferencedInScripts(prefab, prefabPath))
            {
                var selectedObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Debug.Log($"Prefab '{selectedObject.name}' is used in the script of another prefab: {prefab.name}");
                return true;
            }
        }

        return false;
    }

    private static bool IsPrefabReferencedInScripts(GameObject obj, string prefabPath)
    {
        var components = obj.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var component in components)
        {
            if (component == null) continue;

            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.GetIterator();

            while (property.Next(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    property.objectReferenceValue != null)
                {
                    if (AssetDatabase.GetAssetPath(property.objectReferenceValue) == prefabPath)
                    {
                        Debug.Log(
                            $"Prefab is referenced in component: {component.GetType().Name} on GameObject: {obj.name}");
                        return true;
                    }

                    if (property.objectReferenceValue is MonoBehaviour referencedComponent)
                    {
                        var referencedGameObject = referencedComponent.gameObject;
                        var referencedPrefabPath = AssetDatabase.GetAssetPath(referencedGameObject);

                        if (referencedPrefabPath == prefabPath)
                        {
                            Debug.Log($"Prefab is referenced through MonoBehaviour in component:" +
                                      $" {component.GetType().Name} on GameObject: {obj.name}");
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private bool CheckPrefabInScriptableObjects(string prefabPath)
    {
        foreach (var path in _allScriptablePathsInAssets)
        {
            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so == null) continue;

            if (CheckPrefabInSerializedObject(new SerializedObject(so), prefabPath, so.name))
            {
                var selectedObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Debug.Log($"Prefab '{selectedObject.name}' is used inside ScriptableObject '{so.name}'");
                return true;
            }
        }

        return false;
    }

    private static bool CheckPrefabInSerializedObject(SerializedObject serializedObject, string prefabPath,
        string soName)
    {
        SerializedProperty iterator = serializedObject.GetIterator();
        while (iterator.Next(true))
        {
            // Check for direct GameObject reference
            if (iterator.propertyType == SerializedPropertyType.ObjectReference &&
                iterator.objectReferenceValue != null)
            {
                // Check if the object reference is the prefab we're looking for
                if (AssetDatabase.GetAssetPath(iterator.objectReferenceValue) == prefabPath)
                {
                    Debug.Log($"Found direct reference in {soName} at property: {iterator.propertyPath}");
                    return true;
                }

                // Check for prefab reference through MonoBehaviour
                if (iterator.objectReferenceValue is MonoBehaviour mono)
                {
                    string monoPath = AssetDatabase.GetAssetPath(mono.gameObject);
                    if (monoPath == prefabPath)
                    {
                        Debug.Log(
                            $"Found reference through MonoBehaviour in {soName} at property: {iterator.propertyPath}");
                        return true;
                    }
                }

                // Check for prefab reference through Component
                if (iterator.objectReferenceValue is Component comp)
                {
                    string compPath = AssetDatabase.GetAssetPath(comp.gameObject);
                    if (compPath == prefabPath)
                    {
                        Debug.Log(
                            $"Found reference through Component in {soName} at property: {iterator.propertyPath}");
                        return true;
                    }
                }
            }

            if (iterator.isArray)
            {
                for (int i = 0; i < iterator.arraySize; i++)
                {
                    SerializedProperty element = iterator.GetArrayElementAtIndex(i);

                    // Check if array element is an object reference
                    if (element.propertyType == SerializedPropertyType.ObjectReference &&
                        element.objectReferenceValue != null)
                    {
                        // Prefab
                        if (AssetDatabase.GetAssetPath(element.objectReferenceValue) == prefabPath)
                        {
                            Debug.Log($"Found reference in array at {soName}, property: {iterator.propertyPath}[{i}]");
                            return true;
                        }

                        // MonoBehaviour
                        if (element.objectReferenceValue is MonoBehaviour mono)
                        {
                            string monoPath = AssetDatabase.GetAssetPath(mono.gameObject);
                            if (monoPath == prefabPath)
                            {
                                Debug.Log(
                                    $"Found reference through MonoBehaviour in array at {soName}, property: {iterator.propertyPath}[{i}]");
                                return true;
                            }
                        }

                        // Component
                        if (element.objectReferenceValue is Component comp)
                        {
                            var compPath = AssetDatabase.GetAssetPath(comp.gameObject);
                            if (compPath == prefabPath)
                            {
                                Debug.Log(
                                    $"Found reference through Component in array at {soName}, property: {iterator.propertyPath}[{i}]");
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    private bool CheckPrefabInScenes(string prefabPath)
    {
        var processedScenes = 0;
        var totalScenes = _searchScenes.Count;

        foreach (var scenePath in _searchScenes)
        {
            EditorUtility.DisplayProgressBar("Scene Check",
                $"Checking scene: {Path.GetFileNameWithoutExtension(scenePath)}",
                (float)processedScenes / totalScenes);

            if (CheckPrefabInScene(prefabPath, scenePath))
            {
                EditorUtility.ClearProgressBar();
                return true;
            }

            processedScenes++;
        }

        EditorUtility.ClearProgressBar();
        return false;
    }

    private static bool CheckPrefabInScene(string prefabPath, string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        if (!scene.isLoaded)
        {
            Debug.LogWarning($"Scene '{scene.name}' could not be loaded.");
            return false;
        }

        try
        {
            var found = scene.GetRootGameObjects().Any(rootObject => CheckPrefabInHierarchy(rootObject, prefabPath));
            if (found)
            {
                var selectedObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Debug.Log($"Prefab '{selectedObject.name}' is used in scene: {scene.name}");
            }

            return found;
        }
        finally
        {
            if (scene.isLoaded)
            {
                UnloadSceneIfPossible(scene);
            }
        }
    }

    private static bool CheckPrefabInHierarchy(GameObject obj, string prefabPath)
    {
        if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj) == prefabPath)
        {
            var selectedObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Debug.Log($"Prefab '{selectedObject.name}' is used as instance in GameObject: {obj.name}");
            return true;
        }

        if (IsPrefabReferencedInScripts(obj, prefabPath))
        {
            var selectedObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Debug.Log($"Prefab '{selectedObject.name}' is referenced in scripts of GameObject: {obj.name}");
            return true;
        }

        return obj.transform.Cast<Transform>().Any(child => CheckPrefabInHierarchy(child.gameObject, prefabPath));
    }


    private static void UnloadSceneIfPossible(Scene scene)
    {
        Scene currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene != scene)
        {
            EditorSceneManager.UnloadSceneAsync(scene);
        }
    }
}
