#if GODOT
namespace UniDi
{
    [NoReflectionBaking]
    public class NameNodeScopeConcreteIdArgConditionCopyNonLazyBinder : NodeScopeConcreteIdArgConditionCopyNonLazyBinder
    {
        public NameNodeScopeConcreteIdArgConditionCopyNonLazyBinder(
            BindInfo bindInfo,
            NodeCreationParameters gameObjectInfo)
            : base(bindInfo, gameObjectInfo)
        {
        }

        public NodeScopeConcreteIdArgConditionCopyNonLazyBinder WithNodeName(string nodeName)
        {
            NodeInfo.Name = nodeName;
            return this;
        }
    }
}
#endif
