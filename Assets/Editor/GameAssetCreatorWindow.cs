using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Slafurry.Editor.GameAssetCreator
{
    /// <summary>
    /// Window hosting a searchable TreeView (GameAssetCreatorTreeView) that
    /// supports nested categories (folder within folder). Double-click a
    /// leaf item to create that asset type.
    /// </summary>
    public class GameAssetCreatorWindow : EditorWindow
    {
        [SerializeField] private TreeViewState _treeViewState;

        private GameAssetCreatorTreeView _treeView;
        private SearchField _searchField;

        [MenuItem("Slafurry/Game Data")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameAssetCreatorWindow>();
            window.titleContent = new GUIContent("Game Data");
            window.minSize = new Vector2(320, 400);
        }

        private void OnEnable()
        {
            _treeViewState ??= new TreeViewState();
            _treeView = new GameAssetCreatorTreeView(_treeViewState);
            _searchField = new SearchField();
            _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(4);

            _treeView.searchString = _searchField.OnGUI(_treeView.searchString);

            EditorGUILayout.Space(4);

            Rect treeRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            _treeView.OnGUI(treeRect);

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Double-click an asset type to create it inside whichever folder is currently selected in the Project window.",
                MessageType.Info);
        }
    }
}