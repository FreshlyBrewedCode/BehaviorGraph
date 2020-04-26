using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using BehaviorGraph;

namespace BehaviorGraphEditor
{
    public class BehaviorGraphView : GraphView
    {
        public StyleSheet graphStyle;
        public BehaviorGraphWindow graphWindow;

        private BehaviorTree behaviorTree;
        private BehaviorGraphSearchWindow searchWindow;

        private Dictionary<Behavior, BehaviorGraphNode> nodeLookUp;

        public BehaviorGraphView(BehaviorGraphWindow window, BehaviorTree bt)
        {
            this.graphWindow = window;
            this.behaviorTree = bt;

            Debug.Assert(this.graphWindow != null, "BehaviorGraphView needs a parent BehaviorGraphWindow");
            Debug.Assert(this.behaviorTree != null, "BehaviorGraphView needs a BehaviorTree");

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            grid.AddToClassList("grid");
            Insert(0, grid);
            grid.StretchToParentSize();

            nodeLookUp = new Dictionary<Behavior, BehaviorGraphNode>();

            AddBehaviorNodes();
            AddBehaviorConnections();
            AddSearchWindow();
        }

        private BehaviorGraphNode CreateNode(Behavior behavior)
        {
            var node = new BehaviorGraphNode(behavior);
            return node;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                if (port != startPort && port.node != startPort.node && port.direction != startPort.direction)
                {
                    // if the start port is an input we cannot connect it to outputs that are children
                    if (startPort.direction == Direction.Input)
                    {
                        if ((startPort.node as BehaviorGraphNode).HasChild(port.node as BehaviorGraphNode)) return;
                    }
                    else
                    {
                        if ((startPort.node as BehaviorGraphNode).HasParent(port.node as BehaviorGraphNode)) return;
                    }

                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        public void AddBehaviorNodes()
        {
            foreach (var b in behaviorTree.GetBehaviors())
            {
                var node = CreateNode(b);
                nodeLookUp.Add(b, node);
                AddElement(node);
            }
        }

        private void AddBehaviorConnections()
        {
            nodes.ForEach((node) =>
            {
                var behaviorNode = node as BehaviorGraphNode;
                if (behaviorNode.Behavior.ChildCount <= 0) return;

                behaviorNode.shouldManageBehavior = false;

                for (int i = 0; i < behaviorNode.Behavior.ChildCount; i++)
                {
                    var childBehavior = behaviorNode.Behavior.GetChild(i);
                    var childNode = nodeLookUp[childBehavior];
                    childNode.shouldManageBehavior = false;

                    ConnectBehaviors(behaviorNode, childNode);
                    childNode.shouldManageBehavior = true;
                }

                behaviorNode.shouldManageBehavior = true;
            });
        }

        private void AddSearchWindow()
        {
            searchWindow = ScriptableObject.CreateInstance<BehaviorGraphSearchWindow>();
            searchWindow.Initialize(graphWindow, this);

            nodeCreationRequest = (context) =>
            {
                var searchWindowContext = new SearchWindowContext(context.screenMousePosition);
                SearchWindow.Open<BehaviorGraphSearchWindow>(searchWindowContext, searchWindow);
            };
        }

        private void ConnectBehaviors(BehaviorGraphNode parent, BehaviorGraphNode child)
        {
            var edge = new Edge()
            {
                output = parent.OutPort,
                input = child.InPort
            };

            parent.OutPort.Connect(edge);
            child.InPort.Connect(edge);

            AddElement(edge);
        }
    }
}
