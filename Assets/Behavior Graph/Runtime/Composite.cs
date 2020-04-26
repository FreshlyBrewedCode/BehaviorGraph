using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorGraph
{
    public class Composite<T> : Behavior<T> where T : BehaviorContext, new()
    {
        [SerializeField]
        public List<Behavior> children;

        public override int ChildCount => children.Count;

        public Composite()
        {
            children = new List<Behavior>();
        }

        public override Behavior GetChild(int index = 0)
        {
            return children[index];
        }

        public override void SetChild(Behavior child, int index = 0)
        {
            if (index >= ChildCount) children.Add(child);
            else children[index] = child;
        }

        public override void RemoveChild(Behavior child)
        {
            if (children.Contains(child)) children.Remove(child);
        }

    }
}
