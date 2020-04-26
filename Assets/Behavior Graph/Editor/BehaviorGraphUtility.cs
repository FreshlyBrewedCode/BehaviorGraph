using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BehaviorGraph;

namespace BehaviorGraphEditor
{

    public class BehaviorGraphUtility
    {
        private static Dictionary<System.Type, BehaviorDescriptor> descriptorLookup;
        private static Dictionary<System.Type, BehaviorDescriptor> DescriptorLookup
        {
            get
            {
                if (descriptorLookup == null)
                {
                    descriptorLookup = new Dictionary<System.Type, BehaviorDescriptor>();
                    foreach (BehaviorDescriptor d in FindAllBehaviorDescriptors())
                    {
                        System.Type t = GetBehaviorType(d.name);
                        if (t != null) descriptorLookup.Add(t, d);
                    }
                }
                return descriptorLookup;
            }
        }

        public static BehaviorTree ActiveBehaviorTree
        {
            get
            {
                if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(BehaviorTree))
                    return Selection.activeObject as BehaviorTree;
                return null;
            }
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        public static List<BehaviorDescriptor> FindAllBehaviorDescriptors()
        {
            return FindAssetsByType<BehaviorDescriptor>();
        }

        public static System.Type GetBehaviorType(string name)
        {
            string typeName = name.Replace("-", ".");

            System.Type t = typeof(Behavior).Assembly.GetType(typeName);

            if (t == null)
            {
                foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = a.GetType(typeName);
                    if (t != null) break;
                }
            }

            if (t == null)
            {
                Debug.Log("Unable to find type '" + name + "'");
                return null;
            }

            if (!t.IsSubclassOf(typeof(BehaviorGraph.Behavior)))
            {
                Debug.Log("Type '" + name + "' is not a Behavior");
                return null;
            }

            return t;
        }

        public static BehaviorDescriptor GetDescriptor(Behavior behavior)
        {
            var type = behavior.GetType();
            if (DescriptorLookup.ContainsKey(type)) return DescriptorLookup[type];
            return null;
        }

    }
}