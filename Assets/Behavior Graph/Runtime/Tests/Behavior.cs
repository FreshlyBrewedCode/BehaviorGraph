using System.Collections;
using System.Collections.Generic;
using BehaviorGraph;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class Behavior
    {
        // A Test behaves as an ordinary method
        [Test]
        public void BehaviorStart()
        {
            MockBehavior behavior = MockBehavior.CreateInstance<MockBehavior>();
            Assert.AreEqual(0, behavior.startCalled);

            behavior.Tick(new BehaviorContext());

            Assert.AreEqual(1, behavior.startCalled);
        }

        [Test]
        public void BehaviorUpdate()
        {
            MockBehavior behavior = MockBehavior.CreateInstance<MockBehavior>();
            Assert.AreEqual(0, behavior.updateCalled);

            behavior.Tick(new BehaviorContext());

            Assert.AreEqual(1, behavior.updateCalled);
        }

        [Test]
        public void BehaviorTerminate()
        {
            MockBehavior behavior = MockBehavior.CreateInstance<MockBehavior>();
            behavior.returnStatus = BehaviorStatus.Failed;
            Assert.AreEqual(0, behavior.terminateCalled);

            behavior.Tick(new BehaviorContext());

            Assert.AreEqual(1, behavior.terminateCalled);
            Assert.AreEqual(behavior.returnStatus, behavior.terminateStatus);
        }

        [Test]
        public void SequenceFailureOnSecondChild()
        {
            MockSequence sequence = MockSequence.CreateInstance<MockSequence>();
            MockBehavior behavior1 = MockBehavior.CreateInstance<MockBehavior>();
            MockBehavior behavior2 = MockBehavior.CreateInstance<MockBehavior>();

            behavior1.returnStatus = BehaviorStatus.Success;
            behavior2.returnStatus = BehaviorStatus.Failed;

            sequence.AddChild(behavior1);
            sequence.AddChild(behavior2);

            Assert.AreEqual(0, sequence.startCalled);
            Assert.AreEqual(0, sequence.updateCalled);
            Assert.AreEqual(0, sequence.terminateCalled);

            var sequenceStatus = sequence.Tick(new BehaviorContext());

            Assert.AreEqual(1, sequence.startCalled);
            Assert.AreEqual(1, sequence.updateCalled);
            Assert.AreEqual(1, sequence.terminateCalled);

            Assert.AreEqual(1, behavior1.startCalled);
            Assert.AreEqual(1, behavior1.updateCalled);
            Assert.AreEqual(1, behavior1.terminateCalled);

            Assert.AreEqual(1, behavior2.startCalled);
            Assert.AreEqual(1, behavior2.updateCalled);
            Assert.AreEqual(1, behavior2.terminateCalled);

            Assert.AreEqual(behavior1.terminateStatus, BehaviorStatus.Success);
            Assert.AreEqual(behavior2.terminateStatus, BehaviorStatus.Failed);

            Assert.AreEqual(sequenceStatus, BehaviorStatus.Failed);
            Assert.AreEqual(sequence.terminateStatus, sequenceStatus);
        }

        [Test]
        public void SequenceFailureOnFirstChild()
        {
            MockSequence sequence = MockSequence.CreateInstance<MockSequence>();
            MockBehavior behavior1 = MockBehavior.CreateInstance<MockBehavior>();
            MockBehavior behavior2 = MockBehavior.CreateInstance<MockBehavior>();

            behavior1.returnStatus = BehaviorStatus.Failed;
            behavior2.returnStatus = BehaviorStatus.Success;

            sequence.AddChild(behavior1);
            sequence.AddChild(behavior2);

            Assert.AreEqual(0, sequence.startCalled);
            Assert.AreEqual(0, sequence.updateCalled);
            Assert.AreEqual(0, sequence.terminateCalled);

            var sequenceStatus = sequence.Tick(new BehaviorContext());

            Assert.AreEqual(1, sequence.startCalled);
            Assert.AreEqual(1, sequence.updateCalled);
            Assert.AreEqual(1, sequence.terminateCalled);

            Assert.AreEqual(1, behavior1.startCalled);
            Assert.AreEqual(1, behavior1.updateCalled);
            Assert.AreEqual(1, behavior1.terminateCalled);

            Assert.AreEqual(0, behavior2.startCalled);
            Assert.AreEqual(0, behavior2.updateCalled);
            Assert.AreEqual(0, behavior2.terminateCalled);

            Assert.AreEqual(behavior1.terminateStatus, BehaviorStatus.Failed);

            Assert.AreEqual(sequenceStatus, BehaviorStatus.Failed);
            Assert.AreEqual(sequence.terminateStatus, sequenceStatus);
        }

        [Test]
        public void SequenceSuccess()
        {
            MockSequence sequence = MockSequence.CreateInstance<MockSequence>();
            MockBehavior behavior1 = MockBehavior.CreateInstance<MockBehavior>();
            MockBehavior behavior2 = MockBehavior.CreateInstance<MockBehavior>();

            behavior1.returnStatus = BehaviorStatus.Success;
            behavior2.returnStatus = BehaviorStatus.Success;

            sequence.AddChild(behavior1);
            sequence.AddChild(behavior2);

            Assert.AreEqual(0, sequence.startCalled);
            Assert.AreEqual(0, sequence.updateCalled);
            Assert.AreEqual(0, sequence.terminateCalled);

            var sequenceStatus = sequence.Tick(new BehaviorContext());

            Assert.AreEqual(1, sequence.startCalled);
            Assert.AreEqual(1, sequence.updateCalled);
            Assert.AreEqual(1, sequence.terminateCalled);

            Assert.AreEqual(1, behavior1.startCalled);
            Assert.AreEqual(1, behavior1.updateCalled);
            Assert.AreEqual(1, behavior1.terminateCalled);

            Assert.AreEqual(1, behavior2.startCalled);
            Assert.AreEqual(1, behavior2.updateCalled);
            Assert.AreEqual(1, behavior2.terminateCalled);

            Assert.AreEqual(behavior1.terminateStatus, BehaviorStatus.Success);
            Assert.AreEqual(behavior2.terminateStatus, BehaviorStatus.Success);

            Assert.AreEqual(sequenceStatus, BehaviorStatus.Success);
            Assert.AreEqual(sequence.terminateStatus, sequenceStatus);
        }

    }

    public class MockBehavior : BehaviorGraph.Behavior
    {
        public int startCalled, terminateCalled, updateCalled;
        public BehaviorGraph.BehaviorStatus returnStatus, terminateStatus;

        public MockBehavior() : base() { }

        protected override void OnStart(BehaviorContext context)
        {
            startCalled++;
        }

        protected override void OnTerminate(BehaviorContext context, BehaviorStatus status)
        {
            terminateCalled++;
            terminateStatus = status;
        }

        protected override BehaviorStatus OnUpdate(BehaviorContext context)
        {
            updateCalled++;
            return returnStatus;
        }
    }

    public class MockSequence : BehaviorGraph.Sequence
    {
        public int startCalled, updateCalled, terminateCalled, createContextCalled, destoryContextCalled;
        public BehaviorStatus terminateStatus;

        public void AddChild(BehaviorGraph.Behavior child)
        {
            children.Add(child);
        }

        public override BehaviorStatus Tick(BehaviorContext context)
        {
            return base.Tick(context);
        }

        protected override BehaviorContext CreateContext()
        {
            createContextCalled++;
            return base.CreateContext();
        }

        protected override void DestoryContext(Context context)
        {
            destoryContextCalled++;
            base.DestoryContext(context);
        }

        protected override void OnStart(Context context)
        {
            startCalled++;
            base.OnStart(context);
        }

        protected override void OnTerminate(Context context, BehaviorStatus status)
        {
            terminateCalled++;
            terminateStatus = status;
            base.OnTerminate(context, status);
        }

        protected override BehaviorStatus OnUpdate(Context context)
        {
            updateCalled++;
            return base.OnUpdate(context);
        }
    }
}
