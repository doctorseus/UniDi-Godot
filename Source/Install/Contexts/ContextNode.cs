#if GODOT

using System;
using System.Collections.Generic;
using System.Linq;
using DevLabs.Collections;
using Godot;
using UniDi.Internal;

namespace UniDi
{
    public abstract partial class ContextNode : Node
    {
        [Export] public InstallerNode Installer;

        KernelNode _kernel;

        DiContainer _container;

        public event Action PreInstall;
        public event Action PostInstall;
        public event Action PreResolve;
        public event Action PostResolve;

        [Inject(Optional = true)]
        protected internal DiContainer _parentContainer;

        bool _hasInstalled;

        public DiContainer Container
        {
            get { return _container; }
        }

        public override void _EnterTree()
        {
            GD.Print("SceneContext > _EnterTree");



            RunInternal();
        }

        private class InitKernelCallback : IInitializable
        {
            private readonly KernelNode _kernel;

            public InitKernelCallback(KernelNode kernel)
            {
                this._kernel = kernel;
            }

            public void Initialize()
            {
                this._kernel.RunInitialize();
            }
        }

        public override void _Ready()
        {
            GD.Print($"SceneContext({Name}) > _Ready");
            var nodeKernel = _container.Resolve<KernelNode>();
            GD.Print(" nodeKernel = " + nodeKernel);

            // Only if we have a parent we could get an InitializableManager.
            // We don't really have to care about a ProjectContextNode, its _ready() will always be fired before
            // the main scene _ready().
            if (_parentContainer != null)
            {
                var parentManager = _parentContainer.Resolve<InitializableManager>();

                // If we have a parent InitializableManager we check if it already fired
                // - If not, we register ourselves as the very last to initialize. This will ensure that upper level
                //   scenes will come first in the initialization order.
                // - If yes, we might be a scene which was loaded at a later point so we immediately run.
                if (parentManager is { HasInitialized: false })
                {
                    parentManager.Add(new InitKernelCallback(nodeKernel), Int32.MaxValue);
                }
                else
                {
                    nodeKernel.RunInitialize();
                }
            }
            else
            {
                nodeKernel.RunInitialize();
            }

        }

        protected void RunInternal()
        {
            GD.Print($"> RunInternal ParentInject={_parentContainer}");

            var parentContainer = GetParentContainer();
            Install(parentContainer);

            InstallInternal();

            ResolveAndStart();
        }

        protected abstract DiContainer GetParentContainer();

        protected abstract void InstallInternal();

        public void Install(DiContainer parentContainer)
        {
            GD.Print("> Install");
            Assert.That(_parentContainer == null || _parentContainer == parentContainer);

            // We allow calling this explicitly instead of relying on the [Inject] event above
            // so that we can follow the two-pass construction-injection pattern in the providers
            if (_hasInstalled)
            {
                return;
            }

            _hasInstalled = true;

            Assert.IsNull(_container);
            GD.Print($"_container = parentContainer.CreateSubContainer(); {Name}");
            _container = parentContainer.CreateSubContainer();
            GD.Print($"-_container = {_container}");
            GD.Print($"-Container = {Container}");

            // Do this after creating DiContainer in case it's needed by the pre install logic
            if (PreInstall != null)
            {
                PreInstall();
            }

            var injectableNodes = new List<Node>();

            GetInjectableDescendants(injectableNodes);

            foreach (var instance in injectableNodes)
            {
                // if (instance is MonoKernel)
                // {
                //     Assert.That(ReferenceEquals(instance, _kernel),
                //         "Found MonoKernel derived class that is not hooked up to GameObjectContext.  If you use MonoKernel, you must indicate this to GameObjectContext by dragging and dropping it to the Kernel field in the inspector");
                // }

                _container.QueueForInject(instance);
            }

            _container.IsInstalling = true;
            GD.Print("> Install 2");
            try
            {            GD.Print("> Install 3");
                InstallBindings(injectableNodes);            GD.Print("> Install 4");
            }
            finally
            {
                _container.IsInstalling = false;
            }
            GD.Print("> Install 5");
            if (PostInstall != null)
            {
                PostInstall();
            }            GD.Print("> Install 6");
        }

        void ResolveAndStart()
        {
            GD.Print("> ResolveAndStart");
            if (PreResolve != null)
            {
                PreResolve();
            }

            _container.ResolveRoots();

            if (PostResolve != null)
            {
                PostResolve();
            }

            // Normally, the IInitializable.Initialize method would be called during MonoKernel.Start
            // However, this behaviour is undesirable for dynamically created objects, since Unity
            // has the strange behaviour of waiting until the end of the frame to call Start() on
            // dynamically created objects, which means that any GameObjectContext that is created
            // dynamically via a factory cannot be used immediately after calling Create(), since
            // it will not have been initialized
            // So we have chosen to diverge from Unity behaviour here and trigger IInitializable.Initialize
            // immediately - but only when the GameObjectContext is created dynamically.  For any
            // GameObjectContext's that are placed in the scene, we still want to execute
            // IInitializable.Initialize during Start()
            // if (/*gameObject.scene.isLoaded &&*/ !_container.IsValidating)
            // {
            //     _kernel = _container.Resolve<NodeKernel>();
            //     _kernel.Initialize();
            //     GD.Print("> NodeKernel>Initialize()");
            // }
        }

        protected abstract void GetInjectableDescendants(List<Node> nodes);

        void InstallBindings(List<Node> injectableNodes)
        {
            Container.Bind<UniDiSceneLoader>().AsSingle();

            GD.Print("> InstallBindings 1");
            // _container.DefaultParent = transform;
            // _container.Bind<Context>().FromInstance(this);
            _container.Bind<ContextNode>().FromInstance(this);
            GD.Print("> InstallBindings 2");
            if (_kernel == null)
            {            GD.Print("> InstallBindings 31");
                _container.Bind<KernelNode>()
                    .To<DefaultKernelNode>().OnNewNode().AsSingle().OnInstantiated((context, o) =>
                    {
                        _kernel = (KernelNode) o;
                    }).NonLazy();
                GD.Print("> InstallBindings 32");
            }
            else
            {
                GD.Print("> InstallBindings 41");
                _container.Bind<KernelNode>().FromInstance(_kernel).AsSingle().NonLazy();
                GD.Print("> InstallBindings 42");
            }
            GD.Print("> InstallBindings 5");
            // InstallSceneBindings(injectableMonoBehaviours);
            InstallInstallers();
            GD.Print("> InstallBindings 6");
        }

        void InstallInstallers()
        {
            if (Installer == null) return;

            Assert.DerivesFrom<InstallerNodeBase>(Installer.GetType());
            _container.Inject(Installer);
            Installer.InstallBindings();
        }

        public override void _ExitTree()
        {
            GD.Print("GameController> Dispose(_ExitTree)");
        }

        protected override void Dispose(bool disposing)
        {
            GD.Print("GameController> Dispose(" + disposing + ")");
        }

    }
}
#endif
