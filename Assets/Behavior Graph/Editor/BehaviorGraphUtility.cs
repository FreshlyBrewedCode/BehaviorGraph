using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static BehaviorSearchCategory GetBehaviorCategoryTree()
        {
            var rootCategory = new BehaviorSearchCategory("root");
            foreach (var descriptor in FindAllBehaviorDescriptors())
            {
                rootCategory.Append(descriptor);
            }

            return rootCategory;
        }

    }

    public class BehaviorSearchCategory
    {
        public string name;
        public List<BehaviorSearchCategory> subCategories;
        public BehaviorDescriptor descriptor;
        public bool IsBehavior => descriptor != null;

        public BehaviorSearchCategory(string name)
        {
            this.name = name;
            subCategories = new List<BehaviorSearchCategory>();
        }

        public BehaviorSearchCategory(string name, BehaviorDescriptor descriptor)
        {
            this.name = name;
            this.descriptor = descriptor;
        }

        public void Append(BehaviorDescriptor descriptor)
        {
            Append(descriptor, descriptor.editorPath);
        }

        public void Append(BehaviorDescriptor descriptor, string path)
        {
            // Trim any whitespaces or slashes to get rid of empty categories
            path = path.Trim(' ', '/');

            // Get the position of the first slash, signaling the next Category in the path
            int nextCat = path.IndexOf('/');

            // if there is no slash it means we have reached the end of the path so we just add the descriptor
            if (nextCat < 0)
            {
                subCategories.Add(new BehaviorSearchCategory(path, descriptor));
                return;
            }

            // Get the name of first category in the current path
            string catName = path.Substring(0, nextCat);

            // try to get the child category or create a new one with that name
            var cat = GetCategory(catName);
            if (cat == null)
            {
                cat = new BehaviorSearchCategory(catName);
                subCategories.Add(cat);
            }

            // Recursivly append the remaining categories
            cat.Append(descriptor, path.Substring(nextCat + 1));
        }

        public BehaviorSearchCategory GetCategory(string name)
        {
            return subCategories.FirstOrDefault(c => c.name == name);
        }
    }
}