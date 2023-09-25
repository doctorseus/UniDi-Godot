#if GODOT

using System;
using System.Collections.Generic;
using Godot;
using UniDi.Internal;

namespace UniDi
{
    public partial class NodeContext : NodeContextBase
    {

        public override void _EnterTree()
        {
            GD.Print("NodeContext > _EnterTree");

            RunInternal();
        }

        public override void _Ready()
        {
            GD.Print("NodeContext > _Ready");
        }

        protected override IEnumerable<DiContainer> GetParentContainers()
        {
            if (this._parentContainer == null)
            {
                // Check if we have an AutoloadContext instance available
                // otherwise we use the StaticContext as parent (if no parent is given).

                if (AutoloadContext.Instance != null)
                {
                    return new[] { AutoloadContext.Instance.Container };
                }
                else
                {
                    return new[] { StaticContext.Container };
                }
            }
            else
            {
                return new[] { _parentContainer };
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
