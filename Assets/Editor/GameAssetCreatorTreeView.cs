using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Slafurry.Utils.Attributes;

namespace Slafurry.Editor.GameAssetCreator
{
    /// <summary>
    /// TreeView listing every ScriptableObject marked with [GameAssetCreator].
    /// Category strings support "/" to build nested folders - e.g.
    /// Category = "Audio/Music" produces:
    ///   Audio
    ///     Music
    ///       Music Library
    ///
    /// Ordering is fully explicit via the attribute's Order value.
    /// Search is handled by TreeView's built-in searchString filtering
    /// (matched against each item's displayName).
    /// Double-click a leaf item to create an instance of that type.
    /// </summary>
    public class GameAssetCreatorTreeView : TreeView
    {
        private class Entry
        {
            public Type Type;
            public string Category;
            public string DisplayName;
            public int Order;
        }

        private class TypeTreeViewItem : TreeViewItem
        {
            public readonly Type Type;

            public TypeTreeViewItem(int id, int depth, string displayName, Type type)
                : base(id, depth, displayName)
            {
                Type = type;
            }
        }

        private readonly List<Entry> _entries;

        public GameAssetCreatorTreeView(TreeViewState state) : base(state)
        {
            _entries = ScanEntries();
            showBorder = true;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var folderCache = new Dictionary<string, TreeViewItem> { [""] = root };
            int nextId = 1;

            // sort by category path first, then by Order within that same
            // category, so siblings render in exactly the order specified
            var sorted = _entries.OrderBy(e => e.Category).ThenBy(e => e.Order).ToList();

            foreach (var entry in sorted)
            {
                var parent = GetOrCreateFolder(root, folderCache, entry.Category, ref nextId);
                var item = new TypeTreeViewItem(nextId++, parent.depth + 1, entry.DisplayName, entry.Type)
                {
                    icon = GetTypeIcon(entry.Type)
                };
                parent.AddChild(item);
            }

            if (!root.hasChildren)
                root.AddChild(new TreeViewItem(nextId++, 0, "No [GameAssetCreator] types found"));

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        private TreeViewItem GetOrCreateFolder(TreeViewItem root, Dictionary<string, TreeViewItem> cache, string categoryPath, ref int nextId)
        {
            if (string.IsNullOrEmpty(categoryPath)) return root;
            if (cache.TryGetValue(categoryPath, out var existing)) return existing;

            var parts = categoryPath.Split('/');
            TreeViewItem current = root;
            string builtPath = "";

            foreach (var part in parts)
            {
                builtPath = string.IsNullOrEmpty(builtPath) ? part : $"{builtPath}/{part}";

                if (!cache.TryGetValue(builtPath, out var folder))
                {
                    folder = new TreeViewItem(nextId++, current.depth + 1, part)
                    {
                        icon = FolderIcon
                    };
                    current.AddChild(folder);
                    cache[builtPath] = folder;
                }

                current = folder;
            }

            return current;
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            if (item is TypeTreeViewItem typeItem)
                CreateAsset(typeItem.Type, typeItem.displayName);
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        private static Texture2D FolderIcon =>
            (Texture2D)EditorGUIUtility.IconContent("Folder Icon").image;

        private static Texture2D GetTypeIcon(Type type) =>
            (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image;

        private static List<Entry> ScanEntries()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => new { Type = t, Attr = t.GetCustomAttribute<GameAssetCreatorAttribute>() })
                .Where(x => x.Attr != null)
                .Select(x => new Entry
                {
                    Type = x.Type,
                    Category = x.Attr.Category,
                    DisplayName = x.Attr.DisplayName,
                    Order = x.Attr.Order
                })
                .ToList();
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch { return Array.Empty<Type>(); }
        }

        private static void CreateAsset(Type type, string displayName)
        {
            string folder = GetSelectedFolderOrDefault();
            string fileName = $"New {displayName}.asset";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{fileName}");

            var instance = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = instance;
        }

        private static string GetSelectedFolderOrDefault()
        {
            foreach (var obj in Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(path)) return path;
                return Path.GetDirectoryName(path);
            }
            return "Assets";
        }
    }
}