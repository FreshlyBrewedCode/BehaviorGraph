using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorGraph
{
    public class Parallel : Composite<Parallel.Context>
    {
        public class Context : BehaviorContext
        {
            public BehaviorContext[] childContexts;
            public int currentChild;
            public int numberOfChildrenSucceeded;

            public override BehaviorContext ChildContext
            {
                get { return childContexts[currentChild]; }
                set { childContexts[currentChild] = value; }
            }
        }

        public Parallel() : base()
        {

        }

        protected override BehaviorContext CreateContext()
        {
            Context context = new Context();
            context.childContexts = new BehaviorContext[children.Count];

            return context;
        }

        protected override void OnStart(Context context)
        {
            context.currentChild = 0;
            context.numberOfChildrenSucceeded = 0;
        }

        protected override BehaviorStatus OnUpdate(Context context)
        {
            // Keep going until all children are proccessed
            while (context.numberOfChildrenSucceeded < children.Count)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    // Set the current child for the context
                    context.currentChild = i;

                    // Tick the current child
                    var status = children[i].Tick(context);

                    // Only return if the child failed
                    if (status == BehaviorStatus.Failed)
                    {
                        Debug.Log("Terminate children here");
                        return status;
                    }
                    else if (status == BehaviorStatus.Success)
                    {
                        context.numberOfChildrenSucceeded++;
                    }
                }

                // Check if we are still running or we are done
                if (context.numberOfChildrenSucceeded < children.Count)
                {
                    return BehaviorStatus.Running;
                }
                else
                {
                    return BehaviorStatus.Success;
                }
            }

            // If we exit the loop it means all children have succeeded
            return BehaviorStatus.Success;
        }
    }
}
