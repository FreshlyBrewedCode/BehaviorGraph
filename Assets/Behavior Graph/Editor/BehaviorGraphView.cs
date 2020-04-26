using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using BehaviorGraph;
using System.Linq;

namespace BehaviorGraphEditor
{
    public class BehaviorGraphView : GraphView
    {
        public StyleSheet graphStyle;
        public BehaviorGraphWindow graphWindow;

        private BehaviorTree behaviorTree;
        private BehaviorGraphSearchWindow searchWindow;

        private Dictionary<Behavior, BehaviorGraphNode> nodeLookUp;
        private List<Behavior> behaviorsToDelete;

        public BehaviorGraphView(BehaviorGraphWindow window, BehaviorTree bt)
        {
            this.graphWindow = window;
            this.behaviorTree = bt;

            // Make sure we can continue without problems
            Debug.Assert(this.graphWindow != null, "BehaviorGraphView needs a parent BehaviorGraphWindow");
            Debug.Assert(this.behaviorTree != null, "BehaviorGraphView needs a BehaviorTree");

            // Init 
            nodeLookUp = new Dictionary<Behavior, BehaviorGraphNode>();
            behaviorsToDelete = new List<Behavior>();

            // Setup graph view features
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Setup grid
            var grid = new GridBackground();
            grid.AddToClassList("grid");
            Insert(0, grid);
            grid.StretchToParentSize();

            // Load the current behavior tree
            AddBehaviorNodes();
            AddBehaviorConnections();

            // Add windows
            AddSearchWindow();

            this.graphViewChanged = OnGraphViewChanged;
        }

        public void AddBehavior(System.Type behaviorType, Vector2 position)
        {
            var b = behaviorTree.AddBehavior(behaviorType);
            var node = CreateNode(b);

            var mousePosition = graphWindow.rootVisualElement.ChangeCoordinatesTo(graphWindow.rootVisualElement.parent,
                position - graphWindow.position.position);
            var graphMousePosition = this.contentViewContainer.WorldToLocal(mousePosition);

            node.SetPosition(new Rect(graphMousePosition, BehaviorGraphNode.Size));
            this.AddElement(node);

            node.MarkDirty();
            EditorUtility.SetDirty(behaviorTree);
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

        // So because there seems to be no callback or virtual method for deleting a node inside the graph the only way I
        // found to detect if we should delete the Behavior of a deleted node is to listen to the OnGraphViewChanged callback
        // and get the affected Behaviors from the elementsToRemove list. However, because the disconnecting of Behaviors is 
        // currently handled inside the Disconnect method of the BehaviorNodePort we need to make sure the actual deletion of
        // the Behaviors happens AFTER the edges where properly disconnected. By looking at the source I found that the DeleteSelection
        // method will call OnGraphViewChanged (where we put all affected Behaviors in behaviorsToDelete) and after that disconnect
        // the edges. So it should be safe to remove the Behaviors after base.DeleteSelection has been called.
        // TODO: handle disconnecting and deletion of the actual Behavior Objects centrally in OnGraphViewChanged
        public override EventPropagation DeleteSelection()
        {
            var result = base.DeleteSelection();

            // Remove the behaviors
            if (behaviorsToDelete.Count > 0)
            {
                foreach (var behavior in behaviorsToDelete)
                {
                    Behavior.DestroyImmediate(behavior, true);
                }
                behaviorsToDelete.Clear();
            }

            return result;
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

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                var extraElementsToRemove = new List<GraphElement>();

                // Make sure we remove all connections from the nodes we want to delete
                foreach (var node in change.elementsToRemove.OfType<BehaviorGraphNode>())
                {
                    // Remove all parent connections
                    foreach (var edge in node.InPort.connections)
                    {
                        if (!change.elementsToRemove.Contains(edge)) extraElementsToRemove.Add(edge);
                    }

                    // Remove all child connections
                    foreach (var edge in node.OutPort.connections)
                    {
                        if (!change.elementsToRemove.Contains(edge)) extraElementsToRemove.Add(edge);
                    }

                    // Mark the corresponding behavior to be deleted
                    behaviorsToDelete.Add(node.Behavior);
                }
                change.elementsToRemove.AddRange(extraElementsToRemove);
            }

            return change;
        }
    }
}
