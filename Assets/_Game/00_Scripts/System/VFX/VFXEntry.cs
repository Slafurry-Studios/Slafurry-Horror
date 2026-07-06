using Slafurry.Utils.VFX;

[System.Serializable]
    public class VFXEntry
    {
        public string key;
        public VFXCleaner prefab;
        public int defaultCapacity = 5;
        public int maxSize = 30;
    }