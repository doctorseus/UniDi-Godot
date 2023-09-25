namespace UniDi
{
    public interface IDecoratableNodeKernel
    {
        bool ShouldInitializeOnStart();
        void Initialize();
        void Update();
        void FixedUpdate();
        void LateUpdate();
        void Dispose();
        void LateDispose();
    }

    public class DecoratableNodeKernel : IDecoratableNodeKernel
    {
        [InjectLocal] 
        public TickableManager TickableManager { get; protected set; } = null;

        [InjectLocal]
        public InitializableManager InitializableManager { get; protected set; } = null;

        [InjectLocal]
        public DisposableManager DisposablesManager { get; protected set; } = null;
        
        
        public virtual bool ShouldInitializeOnStart() => true;
        
        public virtual void Initialize()
        {
            InitializableManager.Initialize();
        }

        public void Update()
        {
            TickableManager.Update();
        }

        public void FixedUpdate()
        {
            TickableManager.FixedUpdate();
        }

        public void LateUpdate()
        {
            TickableManager.LateUpdate();
        }

        public void Dispose()
        {
            DisposablesManager.Dispose();
        }

        public void LateDispose()
        {
            DisposablesManager.LateDispose();
        }
    }

    public abstract class BaseNodeKernelDecorator : IDecoratableNodeKernel
    {
        [Inject] 
        protected IDecoratableNodeKernel DecoratedNodeKernel;

        public virtual bool ShouldInitializeOnStart() => DecoratedNodeKernel.ShouldInitializeOnStart();
        public virtual void Initialize() => DecoratedNodeKernel.Initialize();
        public virtual void Update() => DecoratedNodeKernel.Update();
        public virtual void FixedUpdate() => DecoratedNodeKernel.FixedUpdate();
        public virtual void LateUpdate() => DecoratedNodeKernel.LateUpdate();
        public virtual void Dispose() => DecoratedNodeKernel.Dispose();
        public virtual void LateDispose() => DecoratedNodeKernel.LateDispose();
    }

}