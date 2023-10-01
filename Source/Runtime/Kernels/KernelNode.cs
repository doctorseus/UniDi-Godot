#if GODOT

using Godot;
using UniDi.Internal;

namespace UniDi
{
    public partial class KernelNode : Node
    {
        [InjectLocal]
        TickableManager _tickableManager = null;

        [InjectLocal]
        InitializableManager _initializableManager = null;

        [InjectLocal]
        DisposableManager _disposablesManager = null;

        [InjectOptional] 
        private IDecoratableNodeKernel decoratableNodeKernel;

        bool _hasInitialized;
        bool _isDestroyed;

        protected bool IsDestroyed
        {
            get { return _isDestroyed; }
        }

        public override void _Ready()
        { } // we fire initialize from root node otherwise no nodes can be added to it (as it is data blocked)

        internal void RunInitialize()
        {
            if (decoratableNodeKernel?.ShouldInitializeOnStart()??true)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            GD.Print(">>>> Initialize " + GetParent().Name);
            // We don't put this in start in case Start is overridden
            if (!_hasInitialized)
            {
                _hasInitialized = true;

                if (decoratableNodeKernel != null)
                {
                    decoratableNodeKernel.Initialize();
                }
                else
                {
                    GD.Print(">>>> _initializableManager.Initialize()");
                    _initializableManager.Initialize();
                }
            }
        }

        public override void _Process(double delta)
        {
            Update();
        }

        public virtual void Update()
        {
            // Don't spam the log every frame if initialization fails and leaves it as null
            if (_tickableManager != null)
            {
                if (decoratableNodeKernel != null)
                {
                    decoratableNodeKernel.Update();
                }
                else
                {
                    _tickableManager.Update();
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            FixedUpdate();
        }

        public virtual void FixedUpdate()
        {
            // Don't spam the log every frame if initialization fails and leaves it as null
            if (_tickableManager != null)
            {
                if (decoratableNodeKernel != null)
                {
                    decoratableNodeKernel.FixedUpdate();
                }
                else
                {
                    _tickableManager.FixedUpdate();
                }
            }
        }

        public virtual void LateUpdate()
        {
            // Don't spam the log every frame if initialization fails and leaves it as null
            if (_tickableManager != null)
            {
                if (decoratableNodeKernel != null)
                {
                    decoratableNodeKernel.LateUpdate();
                }
                else
                {
                    _tickableManager.LateUpdate();
                }
            }
        }

        public override void _ExitTree()
        {
            OnDestroy();
        }

        public virtual void OnDestroy()
        {
            // _disposablesManager can be null if we get destroyed before the Start event
            if (_disposablesManager != null)
            {
                Assert.That(!_isDestroyed);
                _isDestroyed = true;

                if (decoratableNodeKernel != null)
                {
                    decoratableNodeKernel.Dispose();
                    decoratableNodeKernel.LateDispose();
                }
                else
                {
                    _disposablesManager.Dispose();
                    _disposablesManager.LateDispose();
                }
            }
        }
    }
}

#endif
