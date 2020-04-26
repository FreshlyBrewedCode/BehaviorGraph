using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorGraph
{

    public class Behavior<T> : Behavior where T : BehaviorContext, new()
    {
        // Called when the running status is entered
        protected virtual void OnStart(T context) { }
        protected override void OnStart(BehaviorContext context)
        {
            this.OnStart(context as T);
        }

        // Called during the running status
        protected virtual BehaviorStatus OnUpdate(T context) { return BehaviorStatus.Success; }
        protected override BehaviorStatus OnUpdate(BehaviorContext context)
        {
            return this.OnUpdate(context as T);
        }


        // Called once the behavior finished with success or failure
        protected virtual void OnTerminate(T context, BehaviorStatus status) { }
        protected override void OnTerminate(BehaviorContext context, BehaviorStatus status)
        {
            this.OnTerminate(context as T, status);
        }

        protected override BehaviorContext CreateContext()
        {
            return new T();
        }

        protected override void DestoryContext(BehaviorContext context)
        {
            this.DestoryContext(context as T);
        }

        protected virtual void DestoryContext(T context)
        {
            context.status = BehaviorStatus.Invalid;
        }

        public Behavior()
        {

        }

        // Tick function used to make sure we always invoke onStart, onUpdate and onTerminate according to state 
        public override BehaviorStatus Tick(BehaviorContext context)
        {
            if (context.ChildContext == null || context.ChildContext.status == BehaviorStatus.Invalid)
            {
                context.ChildContext = CreateContext();
                OnStart(context.ChildContext as T);
            }

            var status = OnUpdate(context.ChildContext as T);
            context.ChildContext.status = status;

            if (status != BehaviorStatus.Running)
            {
                OnTerminate(context.ChildContext as T, status);
                DestoryContext(context.ChildContext as T);
            }

            return status;
        }

        [SerializeField]
        private BehaviorDescriptor descriptor;
    }

    public class BehaviorContext
    {
        public BehaviorStatus status;
        public virtual BehaviorContext ChildContext { get; set; }

        public BehaviorContext()
        {
            status = BehaviorStatus.Invalid;
        }
    }
}
