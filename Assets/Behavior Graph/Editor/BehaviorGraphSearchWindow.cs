using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using BehaviorGraph;

namespace BehaviorGraphEditor
{
    public class BehaviorGraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow window;
        private BehaviorGraphView graphView;
        private List<SearchTreeEntry> searchTree;

        public void Initialize(EditorWindow window, BehaviorGraphView view)
        {
            this.window = window;
            this.graphView = view;
            GenerateBehaviorSearchTree();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            graphView.AddBehavior(SearchTreeEntry.userData as System.Type, context.screenMousePosition);
            return true;
        }

        private void GenerateBehaviorSearchTree()
        {
            searchTree = new List<SearchTreeEntry>();
            var categories = BehaviorGraphUtility.GetBehaviorCategoryTree();

            // Set the name of the root element (= search window title)
            categories.name = "Add Behavior";

            // Recursivly add all entries
            AddTreeEntry(categories, 0);
        }

        private void AddTreeEntry(BehaviorSearchCategory cat, int level)
        {
            // Add Behavior element
            if (cat.IsBehavior)
            {
                System.Type behaviorType = BehaviorGraphUtility.GetBehaviorType(cat.descriptor.name);
                if (behaviorType == null)
                {
                    Debug.LogWarning("Could not find Behavior for descriptor '" + cat.descriptor.name + "'");
                    return;
                }

                var entry = new SearchTreeEntry(new GUIContent(cat.name, cat.descriptor.icon))
                {
                    level = level,
                    userData = behaviorType
                };
                searchTree.Add(entry);
            }
            // Add Group
            else
            {
                var group = new SearchTreeGroupEntry(new GUIContent(cat.name), level);
                searchTree.Add(group);

                // Add sub categories
                foreach (var subCat in cat.subCategories)
                {
                    AddTreeEntry(subCat, level + 1);
                }
            }
        }
    }
}
