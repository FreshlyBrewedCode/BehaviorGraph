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

        public void Initialize(EditorWindow window, BehaviorGraphView view)
        {
            this.window = window;
            this.graphView = view;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Add Behavior"), 0)
            };
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            return true;
        }
    }
}
