using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorGraphEditor
{
    public class BehaviorGraphWindow : EditorWindow
    {
        [SerializeField]
        public StyleSheet graphStyle;


        [SerializeField]
        public Texture2D flagIcon;

        private BehaviorGraphView graphView;

        [MenuItem("Tools/Test Graph")]
        private static void ShowWindow()
        {
            var window = GetWindow<BehaviorGraphWindow>();
            window.titleContent = new GUIContent("Behavior Graph");
            window.Show();
        }

        private void OnEnable()
        {
            graphView = new BehaviorGraphView(this, BehaviorGraphUtility.ActiveBehaviorTree)
            {
                name = "Behavior Graph"
            };
            graphView.styleSheets.Add(graphStyle);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }
    }
}