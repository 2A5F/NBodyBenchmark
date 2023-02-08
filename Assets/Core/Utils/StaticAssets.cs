using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Utils
{

    [CreateAssetMenu(fileName = "Static Assets", menuName = "Assets/Static Assets", order = 0)]
    public class StaticAssets : ScriptableObject
    {
        public StaticAssetItem[] assets;

        public static Dictionary<string, Object> Assets { get; private set; }

        private void OnEnable()
        {
            Init(this);
        }

        private static void Init(StaticAssets self)
        {
            if (self == null) return;
            Assets = self.assets.ToDictionary(a => a.name, a => a.asset);
        }

        public static T Get<T>(string name) where T : Object
        {
            if (Assets == null) Init(Resources.Load<StaticAssets>("Static Assets"));
            if (Assets == null) return null;
            if (Assets.TryGetValue(name, out var val)) return val as T;
            else return null;
        }
    }

    [Serializable]
    public class StaticAssetItem
    {
        public string name;
        public Object asset;
    }

}
