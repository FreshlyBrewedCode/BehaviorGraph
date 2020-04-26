using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviorGraph;

namespace BehaviorGraphEditor
{
    public class FindBehaviorsUtility : EditorWindow
    {
        public class BehaviorInfo
        {
            public BehaviorDescriptor descriptor;
            public bool foldout = false;
            public Editor editor;
            public System.Type type;
        }

        private BehaviorInfo[] behaviors;
        private Vector2 scrollPos;

        private GUIStyle foldoutStyle;

        [MenuItem("Tools/Behavior Graph/Find Behaviors Utility")]
        private static void ShowWindow()
        {
            var window = GetWindow<FindBehaviorsUtility>();
            window.titleContent = new GUIContent("Find Behaviors");
            window.Show();
        }

        private void OnEnable()
        {
            foldoutStyle = new GUIStyle("foldout");
            foldoutStyle.richText = true;

            Refresh();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh")) Refresh();

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            var bt = BehaviorGraphUtility.ActiveBehaviorTree;

            foreach (var info in behaviors)
            {
                string title = info.descriptor.name;
                if (info.type != null) title += " [" + info.type.FullName + "]";
                else title += " <i>undefined</i>";

                GUILayout.BeginHorizontal();
                info.foldout = EditorGUILayout.Foldout(info.foldout, title, foldoutStyle);

                if (bt != null)
                {
                    if (GUILayout.Button("+", "minibutton", GUILayout.ExpandWidth(false)))
                    {
                        bt.AddBehavior(info.type);
                    }
                }
                GUILayout.EndHorizontal();

                if (info.foldout)
                {
                    EditorGUI.indentLevel = 2;
                    if (info.editor == null)
                        info.editor = Editor.CreateEditor(info.descriptor);
                    info.editor.OnInspectorGUI();
                }
                EditorGUI.indentLevel = 0;
            }

            GUILayout.EndScrollView();
        }

        private void Refresh()
        {
            behaviors = GetBehaviorInfo();
            Repaint();
        }

        public static BehaviorInfo[] GetBehaviorInfo()
        {
            return BehaviorGraphUtility.FindAllBehaviorDescriptors().Select((desc) =>
            {
                return new BehaviorInfo()
                {
                    descriptor = desc,
                    foldout = false,
                    type = BehaviorGraphUtility.GetBehaviorType(desc.name)
                };
            }).ToArray();
        }

        [MenuItem("Tools/Behavior Graph/List BT Behaviors")]
        public static void ListBehaviorTreeBehaviors()
        {
            if (BehaviorGraphUtility.ActiveBehaviorTree == null) return;
            var behaviors = BehaviorGraphUtility.ActiveBehaviorTree.GetBehaviors();
            Debug.Log("Behavior Count: " + behaviors.Count);

            foreach (var b in behaviors)
            {
                Debug.Log(b.GetType());
            }
        }
    }
}