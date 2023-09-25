#if GODOT
using System;
using System.Diagnostics;
using Godot;

namespace UniDi
{
    // We'd prefer to make this abstract but Unity 5.3.5 has a bug where references
    // can get lost during compile errors for classes that are abstract
    [DebuggerStepThrough]
    public partial class NodeInstallerBase : Node, IInstaller
    {
        [Inject]
        protected DiContainer Container
        {
            get; set;
        }

        public virtual bool IsEnabled
        {
            get { return true; }
        }

        public virtual void Start()
        {
            // Define this method so we expose the enabled check box
        }

        public virtual void InstallBindings()
        {
            throw new NotImplementedException();
        }
    }
}
#endif
