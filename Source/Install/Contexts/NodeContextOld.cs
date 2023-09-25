﻿#if GODOT

using System;
using System.Collections.Generic;
using Godot;
using UniDi.Internal;

namespace UniDi
{
    public partial class NodeContextOld : Node
    {
        [Export] public NodeInstaller Installer;

        NodeKernel _kernel;

        DiContainer _container;

        public event Action PreInstall;
        public event Action PostInstall;
        public event Action PreResolve;
        public event Action PostResolve;

        DiContainer _parentContainer;

        bool _hasInstalled;

        public DiContainer Container
        {
            get { return _container; }
        }

        public override void _EnterTree()
        {
            GD.Print("SceneContext > _EnterTree");

            // TODO: just a decoy parent container for now
            _parentContainer = new DiContainer();

            RunInternal();
        }

        public override void _Ready()
        {
            GD.Print("SceneContext > _Ready");
        }

        protected void RunInternal()
        {
            // We always want to initialize AutoloadContext as early as possible
            //AutoloadContext.SceneTree = GetTree();
            //AutoloadContext.EnsureIsInitialized(this);

            GD.Print("> RunInternal");
            Install(_parentContainer);

            // TODO: this is done by project context normally (we need some toplevel context)
            // InitializableManager, TickableManager, DisposableManager
            UniDiManagersInstaller.Install(_container);

            ResolveAndStart();
        }

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
            _container = parentContainer.CreateSubContainer();

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
            if (/*gameObject.scene.isLoaded &&*/ !_container.IsValidating)
            {
                _kernel = _container.Resolve<NodeKernel>();
                _kernel.Initialize();
                GD.Print("> NodeKernel>Initialize()");
            }
        }

        protected void GetInjectableDescendants(List<Node> nodes)
        {
            foreach (var child in GetChildren())
            {
                GetInjectableDescendantNodes(child, nodes);
            }
        }

        void InstallBindings(List<Node> injectableNodes)
        {
            GD.Print("> InstallBindings 1");
            // _container.DefaultParent = transform;
            // _container.Bind<Context>().FromInstance(this);
            _container.Bind<NodeContextOld>().FromInstance(this);
            GD.Print("> InstallBindings 2");
            if (_kernel == null)
            {            GD.Print("> InstallBindings 31");
                _container.Bind<NodeKernel>()
                    .To<DefaultNodeKernel>().OnNewNode().AsSingle().NonLazy();
                GD.Print("> InstallBindings 32");
            }
            else
            {
                GD.Print("> InstallBindings 41");
                _container.Bind<NodeKernel>().FromInstance(_kernel).AsSingle().NonLazy();
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

            Assert.DerivesFrom<NodeInstallerBase>(Installer.GetType());
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

        // -- UniDiUtilInternal.cs --

        // NOTE: This method will not return components that are within a GameObjectContext
        // It returns monobehaviours in a bottom-up order
        public static void GetInjectableDescendantNodes(
            Node node, List<Node> injectableNodes)
        {
#if UNIDI_INTERNAL_PROFILING
        using (ProfileTimers.CreateTimedBlock("Searching Hierarchy"))
#endif
            {
                GetInjectableDescendantNodesInternal(node, injectableNodes);
            }
        }

        static void GetInjectableDescendantNodesInternal(
            Node node, List<Node> injectableNodes)
        {
            if (node == null)
            {
                return;
            }

            if (node.GetType().DerivesFromOrEqual<NodeContext>())
            {
                // TODO: This is now the same but for NodeContext... fix this
                // Need to make sure we don't inject on any MonoBehaviour's that are below a GameObjectContext
                // Since that is the responsibility of the GameObjectContext
                // BUT we do want to inject on the GameObjectContext itself
                injectableNodes.Add(node);
                return;
            }
            else
            {
                // Recurse first so it adds components bottom up though it shouldn't really matter much
                // because it should always inject in the dependency order
                foreach (var child in node.GetChildren())
                {
                    GD.Print($"> child=({child})");
                    GetInjectableDescendantNodesInternal(child, injectableNodes);
                }

                foreach (var child in node.GetChildren())
                {
                    injectableNodes.Add(child);
                }
            }
        }
    }
}
#endif