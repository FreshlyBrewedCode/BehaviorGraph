using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorGraph
{
    /// Base class for all decorator behaviors. A decorator has only one direct child
    public class Decorator<T> : Behavior<T> where T : BehaviorContext, new()
    {
        protected Behavior child;
        public override int ChildCount => child == null ? 0 : 1;

        public override Behavior GetChild(int index = 0)
        {
            return child;
        }

        public override void RemoveChild(Behavior child)
        {
            if (this.child = child)
                this.child = null;
        }

        public override void SetChild(Behavior child, int index = 0)
        {
            this.child = child;
        }
    }
}
