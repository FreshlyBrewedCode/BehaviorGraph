using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BehaviorGraph;
using System.Linq;

namespace BehaviorGraphEditor
{
    /// Class that contains extension and utility methods to edit behavior trees in the editor
    public static class BehaviorTreeEditorExtension
    {
        /// Instantiate a Behavior of the given type and add it to the tree asset
        public static Behavior AddBehavior(this BehaviorTree bt, System.Type type)
        {
            var newBehavior = ScriptableObject.CreateInstance(type) as Behavior;
            // newBehavior.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(newBehavior, bt);
            // AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(bt);
            EditorUtility.SetDirty(newBehavior);

            return newBehavior;
        }

        // Generic overload
        public static Behavior AddBehavior<T>(this BehaviorTree bt) where T : Behavior
        {
            return AddBehavior(bt, typeof(T));
        }

        public static List<Behavior> GetBehaviors(this BehaviorTree bt)
        {
            var path = AssetDatabase.GetAssetPath(bt);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            List<Behavior> behaviors = new List<Behavior>();
            foreach (var a in assets)
            {
                if (a is Behavior) behaviors.Add(a as Behavior);
            }

            return behaviors;
        }
    }
}
