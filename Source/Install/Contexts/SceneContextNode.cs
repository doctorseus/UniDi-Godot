#if GODOT

using System.Collections.Generic;
using Godot;
using UniDi.Internal;

namespace UniDi
{
    public partial class SceneContextNode : ContextNode
    {

        public override void _EnterTree()
        {
            GD.Print($"NodeContext > _EnterTree ({Name})");

            RunInternal();
        }

        public override void _Ready()
        {
            GD.Print("NodeContext > _Ready");
            base._Ready();
        }

        protected override DiContainer GetParentContainer()
        {
            if (_parentContainer != null)
                return _parentContainer;
            // Check if we have an ProjectContextNode instance available
            // otherwise we use the StaticContext as parent (if no parent is already given).

            if (ProjectContextNode.Instance != null)
            {
                GD.Print($"{Name} > GetParentContainer = ProjectContextNode.Instance.Container = {ProjectContextNode.Instance.Container}");
                return ProjectContextNode.Instance.Container;
            }
            else
            {
                GD.Print($"{Name} > GetParentContainer = StaticContext.Container = {StaticContext.Container}");
                return StaticContext.Container;
            }

        }

        protected override void InstallInternal()
        { }

        protected override void GetInjectableDescendants(List<Node> nodes)
        {
            foreach (var child in GetChildren())
            {
                GetInjectableDescendantNodes(child, nodes);
            }
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
            GD.Print($"##> Add for injection {node.Name}[{node.GetType()}]");

            if (node.GetType().DerivesFromOrEqual<SceneContextNode>())
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
