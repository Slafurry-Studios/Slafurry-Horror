using Slafurry.Core.Interface;
using Slafurry.Utils.Attributes;
using UnityEngine;

namespace Slafurry.System.Audio
{


    [GameAssetCreator("Audio/SFX", "SFX Data", order: 1)]
    [CreateAssetMenu(fileName = "New SFX Data", menuName = "Audio/SFX Data", order = 1)]
    public class SFXData : ScriptableObject
    {
        public SFXCategory[] categories;

        public SFXCategory GetCategory(string categoryName)
        {
            foreach (var cat in categories)
            {
                if (cat.categoryName == categoryName)
                    return cat;
            }
            return null;
        }

        public string GetDropdownLabel() => $"{name}";


        public SFXEffect GetSFXEffect(string categoryName, string effectName)
        {
            var category = GetCategory(categoryName);
            if (category?.effects == null) return null;

            foreach (var effect in category.effects)
            {
                if (effect.groupID == effectName)
                    return effect;
            }
            return null;
        }
    }
}