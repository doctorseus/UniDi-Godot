#if GODOT
using System;
using Godot;

namespace UniDi
{
    [NoReflectionBaking]
    public class NodeScopeConcreteIdArgConditionCopyNonLazyBinder : ScopeConcreteIdArgConditionCopyNonLazyBinder
    {
        public NodeScopeConcreteIdArgConditionCopyNonLazyBinder(
            BindInfo bindInfo,
            NodeCreationParameters nodeInfo)
            : base(bindInfo)
        {
            NodeInfo = nodeInfo;
        }

        protected NodeCreationParameters NodeInfo
        {
            get;
            private set;
        }

        public ScopeConcreteIdArgConditionCopyNonLazyBinder UnderTransform(Node parent)
        {
            NodeInfo.ParentNode = parent;
            return this;
        }

        public ScopeConcreteIdArgConditionCopyNonLazyBinder UnderTransform(Func<InjectContext, Node> parentGetter)
        {
            NodeInfo.ParentNodeGetter = parentGetter;
            return this;
        }

        public ScopeConcreteIdArgConditionCopyNonLazyBinder UnderTransformGroup(string transformGroupname)
        {
            NodeInfo.GroupName = transformGroupname;
            return this;
        }
    }
}
#endif
