﻿using System.Collections.Generic;
using Godot;
using UniDi.Internal;

#if GODOT
namespace UniDi;

public partial class AutoloadContext : NodeContextBase
{
    static AutoloadContext _instance;

    public static bool HasInstance
    {
        get { return _instance != null; }
    }

    public static AutoloadContext Instance
    {
        get
        {
            if (_instance == null)
            {
                Assert.IsNotNull(_instance);
            }

            return _instance;
        }
    }

    public override void _EnterTree()
    {
        GD.Print("AutoloadContext > _EnterTree");
        SetSingletonAndInitialize();
    }

    public override void _Ready()
    {
        GD.Print("AutoloadContext > _Ready");
    }

    private void SetSingletonAndInitialize()
    {
        Assert.IsNull(_instance, "Project cannot have more then one AutoloadContext node");
        _instance = this;
        RunInternal();
    }

    protected override IEnumerable<DiContainer> GetParentContainers()
    {
        return new[] { StaticContext.Container };
    }

    protected override void InstallInternal()
    {
        // TODO: this is done by project context normally (we need some toplevel context)
        // InitializableManager, TickableManager, DisposableManager
        GD.Print($"Container={Container}");
        UniDiManagersInstaller.Install(Container);
    }

    protected override void GetInjectableDescendants(List<Node> nodes)
    {
        // ProjectContext doesn't inject any descendants
    }

    /*
    static AutoloadContext _instance;

    DiContainer _container;

    public DiContainer Container
    {
        get { return _container; }
    }

    public static bool HasInstance
    {
        get { return _instance != null; }
    }

    public static AutoloadContext Instance
    {
        get
        {
            if (_instance == null)
            {
                Assert.IsNotNull(_instance);
            }

            return _instance;
        }
    }

    public static void EnsureIsInitialized(Node trigger)
    {
        GD.Print("AutoloadContext > EnsureIsInitialized");
        InstantiateAndInitialize(trigger.GetTree());
    }

    static void InstantiateAndInitialize(SceneTree sceneTree)
    {
        GD.Print("AutoloadContext > InstantiateAndInitialize = _sceneTree=" + sceneTree);

        _instance = new AutoloadContext();
        _instance.Name = _instance.GetType().Name;
        sceneTree.Root.AddChild(_instance);
        _instance.Owner = sceneTree.Root;



        // Note: We use Initialize instead of awake here in case someone calls
        // ProjectContext.Instance while ProjectContext is initializing
        _instance.Initialize();

    }

    void Initialize()
    {

    }
    */

}
#endif