using System;
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
    public class BehaviorGraphNode : Node
    {
        public string GUID;
        public StyleSheet nodeStyle;

        public bool shouldManageBehavior = true;

        private Behavior behavior;
        public Behavior Behavior => behavior;

        private BehaviorDescriptor descriptor;
        public BehaviorDescriptor Descriptor => descriptor;

        private BehaviorNodePort inPort;
        public BehaviorNodePort InPort => inPort;

        private BehaviorNodePort outPort;
        public BehaviorNodePort OutPort => outPort;

        public BehaviorGraphNode ParentNode
        {
            get
            {
                if (InPort.connected) return InPort.connections.FirstOrDefault().output.node as BehaviorGraphNode;
                return null;
            }
        }
        public List<BehaviorGraphNode> ChildNodes => OutPort.connections.Select(edge => edge.input.node as BehaviorGraphNode).ToList();

        public static Vector2 Size = new Vector2(100, 100);

        public BehaviorGraphNode(Behavior behavior)
        {
            this.behavior = behavior;
            this.descriptor = BehaviorGraphUtility.GetDescriptor(behavior);

            Debug.Assert(descriptor != null && behavior != null, "The behavior node needs a valid Behavior and a valid BehaviorDescriptor");

            this.AddToClassList("behavior-node");
            this.title = descriptor.title;
            this.GUID = Guid.NewGuid().ToString();
            this.SetPosition(new Rect(behavior.metaData.editorPosition, Size));


            ConfigurePorts();
            ConfigureIcon();

            this.RefreshExpandedState();
        }

        private void ConfigurePorts()
        {
            var inPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(float));
            this.Add(inPort);
            this.inPort = inPort as BehaviorNodePort;

            var outPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(float));
            this.Add(outPort);
            this.outPort = outPort as BehaviorNodePort;

            this.RefreshPorts();
        }

        private void ConfigureIcon()
        {
            var img = new Image();
            img.image = descriptor.icon;
            img.name = "icon";
            this.titleContainer.Add(img);
        }

        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return BehaviorNodePort.CreateBehaviorNodePort<Edge>(orientation, direction, capacity, type);
        }

        public override void SetPosition(Rect newPos)
        {
            newPos.position = Snapping.Snap(newPos.position, new Vector2(20, 20));
            behavior.metaData.editorPosition = newPos.position;
            base.SetPosition(newPos);
            MarkDirty();
        }

        public void ConnectChildBehavior(BehaviorGraphNode child)
        {
            if (!shouldManageBehavior) return;

            Behavior.SetChild(child.Behavior, Behavior.ChildCount);
            MarkDirty();
        }

        public void DisconnectChildBehavior(BehaviorGraphNode child)
        {
            Debug.Log("Disconnect");

            if (!shouldManageBehavior) return;
            Behavior.RemoveChild(child.Behavior);
            MarkDirty();
        }

        public bool HasDirectChild(BehaviorGraphNode child)
        {
            foreach (var edge in OutPort.connections)
            {
                if (edge.input.node == child) return true;
            }
            return false;
        }

        public bool HasChild(BehaviorGraphNode child)
        {
            if (HasDirectChild(child)) return true;

            foreach (var edge in OutPort.connections)
            {
                if ((edge.input.node as BehaviorGraphNode).HasChild(child)) return true;
            }

            return false;
        }

        public bool HasDirectParent(BehaviorGraphNode parent)
        {
            foreach (var edge in InPort.connections)
            {
                if (edge.output.node == parent) return true;
            }
            return false;
        }

        public bool HasParent(BehaviorGraphNode parent)
        {
            if (HasDirectParent(parent)) return true;

            foreach (var edge in InPort.connections)
            {
                if ((edge.output.node as BehaviorGraphNode).HasParent(parent)) return true;
            }

            return false;
        }

        public void MarkDirty()
        {
            EditorUtility.SetDirty(Behavior);
        }
    }

    public class BehaviorNodePort : Port
    {
        protected BehaviorNodePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type) { }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            if (edge.input != null && edge.output != null && this.direction == Direction.Output)
            {
                var parentNode = edge.output.node as BehaviorGraphNode;
                var childNode = edge.input.node as BehaviorGraphNode;
                // Debug.Log("Connecting: " + parentNode.Behavior + " to " + childNode.Behavior);
                parentNode.ConnectChildBehavior(childNode);
            }
        }

        public override void Disconnect(Edge edge)
        {
            if (edge.input != null && edge.output != null && this.direction == Direction.Output)
            {
                var parentNode = edge.output.node as BehaviorGraphNode;
                var childNode = edge.input.node as BehaviorGraphNode;
                // Debug.Log("Disconnecting: " + parentNode.Behavior + " from " + childNode.Behavior);
                parentNode.DisconnectChildBehavior(childNode);
            }

            base.Disconnect(edge);
        }

        public static Port CreateBehaviorNodePort<TEdge>(Orientation orientation, Direction direction, Port.Capacity capacity, Type type) where TEdge : Edge, new()
        {
            var connectorListener = new BehaviorNodeEdgeConnectorListener();
            var port = new BehaviorNodePort(orientation, direction, capacity, type)
            {
                m_EdgeConnector = new EdgeConnector<TEdge>(connectorListener),
            };
            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }

        private class BehaviorNodeEdgeConnectorListener : IEdgeConnectorListener
        {
            private GraphViewChange m_GraphViewChange;
            private List<Edge> m_EdgesToCreate;
            private List<GraphElement> m_EdgesToDelete;

            public BehaviorNodeEdgeConnectorListener()
            {
                m_EdgesToCreate = new List<Edge>();
                m_EdgesToDelete = new List<GraphElement>();

                m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position) { }
            public void OnDrop(GraphView graphView, Edge edge)
            {
                m_EdgesToCreate.Clear();
                m_EdgesToCreate.Add(edge);

                // We can't just add these edges to delete to the m_GraphViewChange
                // because we want the proper deletion code in GraphView to also
                // be called. Of course, that code (in DeleteElements) also
                // sends a GraphViewChange.
                m_EdgesToDelete.Clear();
                if (edge.input.capacity == Capacity.Single)
                    foreach (Edge edgeToDelete in edge.input.connections)
                        if (edgeToDelete != edge)
                            m_EdgesToDelete.Add(edgeToDelete);
                if (edge.output.capacity == Capacity.Single)
                    foreach (Edge edgeToDelete in edge.output.connections)
                        if (edgeToDelete != edge)
                            m_EdgesToDelete.Add(edgeToDelete);
                if (m_EdgesToDelete.Count > 0)
                    graphView.DeleteElements(m_EdgesToDelete);

                var edgesToCreate = m_EdgesToCreate;
                if (graphView.graphViewChanged != null)
                {
                    edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
                }

                foreach (Edge e in edgesToCreate)
                {
                    graphView.AddElement(e);
                    edge.input.Connect(e);
                    edge.output.Connect(e);
                }
            }
        }
    }
}