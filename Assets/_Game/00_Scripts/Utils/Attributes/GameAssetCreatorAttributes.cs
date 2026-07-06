using System;

namespace Slafurry.Utils.Attributes
{
    /// <summary>
    /// Marks a ScriptableObject class to appear in the custom Game Asset
    /// Creator window (Slafurry > Create Game Asset), instead of relying
    /// on [CreateAssetMenu]'s order parameter - which bubbles priority up
    /// to parent submenus in unpredictable ways.
    ///
    /// Category groups entries under a bold header in the window.
    /// Order sorts entries within that category - fully explicit, no
    /// interaction with Unity's native Create menu whatsoever.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class GameAssetCreatorAttribute : Attribute
    {
        public string Category { get; }
        public string DisplayName { get; }
        public int Order { get; }

        public GameAssetCreatorAttribute(string category, string displayName, int order = 0)
        {
            Category = category;
            DisplayName = displayName;
            Order = order;
        }
    }
}