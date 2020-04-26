using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorAgent : MonoBehaviour
{
    private Dictionary<string, Context> contextMap;

    // Start is called before the first frame update
    void Start()
    {
        const int NUMBER = 1000000;
        using (new Timer("Empty"))
        {
            for (var ii = 0; ii < NUMBER; ii++)
            {
            }
        }

        using (new Timer("Get Component"))
        {
            for (var ii = 0; ii < NUMBER; ii++)
            {
                GetComponent<Camera>();
            }
        }

        ComponentContext c = new ComponentContext(this);
        using (new Timer("Get Component (Component Context)"))
        {
            for (var ii = 0; ii < NUMBER; ii++)
            {
                c.GetComponent<Camera>();
            }
        }

        contextMap = new Dictionary<string, Context>();
        TestContextBehavior testContextBehavior = new TestContextBehavior();
        testContextBehavior.Init(this);

        using (new Timer("Context Behavior (Get from Context)"))
        {
            for (var ii = 0; ii < NUMBER; ii++)
            {
                testContextBehavior.Tick(contextMap[testContextBehavior.GUID] as TestContextBehavior.TestContextBehaviorContext, this);
            }
        }

        TestBehavior behavior = new TestBehavior();
        behavior.Init(this);

        using (new Timer("Behavior (Get Directly)"))
        {
            for (var ii = 0; ii < NUMBER; ii++)
            {
                behavior.Tick(null, this);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RegisterContext<T>(ContextBehavior<T> behavior, Context context) where T : Context, new()
    {
        contextMap.Add(behavior.GUID, context);
    }

    class Timer : System.IDisposable
    {
        private readonly string m_Text;
        private System.Diagnostics.Stopwatch m_Stopwatch;

        public Timer(string text)
        {
            m_Text = text;
            m_Stopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void Dispose()
        {
            m_Stopwatch.Stop();
            Debug.Log(string.Format("Profiled {0}: {1:0.00}ms", m_Text, m_Stopwatch.ElapsedMilliseconds));
        }
    }

    public class ComponentContext
    {
        private Dictionary<System.Type, Component> components;
        private BehaviorAgent agent;

        public ComponentContext(BehaviorAgent agent)
        {
            this.agent = agent;
            components = new Dictionary<System.Type, Component>();
        }

        public T GetComponent<T>() where T : Component
        {
            // System.Type type = typeof(T);
            // T comp;
            // try
            // {
            //     comp = components[type] as T;
            // }
            // catch (System.Exception e)
            // {
            //     comp = agent.GetComponent(type) as T;
            //     components.Add(type, comp);
            //     return comp;
            // }

            // return comp;
            return agent.GetComponent<T>();
        }
    }

    public class Context
    {
        public Context()
        {
        }
    }

    public class ContextBehavior<T> where T : Context, new()
    {
        protected T context;
        private string guid;
        public string GUID => guid;

        public ContextBehavior()
        {
            guid = System.Guid.NewGuid().ToString();
        }

        public virtual void Init(BehaviorAgent agent)
        {
            agent.RegisterContext<T>(this, new T());
        }

        public virtual void Tick(T context, BehaviorAgent agent)
        {

        }
    }

    public class TestContextBehavior : ContextBehavior<TestContextBehavior.TestContextBehaviorContext>
    {
        public class TestContextBehaviorContext : Context
        {
            public Camera camera;
        }

        public override void Init(BehaviorAgent agent)
        {
            var c = new TestContextBehaviorContext();
            c.camera = agent.GetComponent<Camera>();

            agent.RegisterContext<TestContextBehaviorContext>(this, c);
        }

        public override void Tick(TestContextBehaviorContext context, BehaviorAgent agent)
        {
            context.camera.ToString();
        }
    }

    public class TestBehavior : ContextBehavior<TestContextBehavior.TestContextBehaviorContext>
    {
        public override void Init(BehaviorAgent agent)
        {

        }

        public override void Tick(TestContextBehavior.TestContextBehaviorContext context, BehaviorAgent agent)
        {
            agent.GetComponent<Camera>().ToString();
        }
    }
}

