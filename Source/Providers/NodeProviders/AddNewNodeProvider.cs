#if GODOT
using System;
using System.Collections.Generic;
using Godot;

namespace UniDi
{
    [NoReflectionBaking]
    public class AddNewNodeProvider  : AddNewNodeProviderBase
    {
        readonly NodeCreationParameters _nodeBindInfo;

        public AddNewNodeProvider(
            DiContainer container, Type componentType,
            IEnumerable<TypeValuePair> extraArguments, NodeCreationParameters nodeBindInfo,
            object concreteIdentifier,
            Action<InjectContext, object> instantiateCallback)
            : base(container, componentType, extraArguments, concreteIdentifier, instantiateCallback)
        {
            _nodeBindInfo = nodeBindInfo;
        }

        protected override bool ShouldToggleActive
        {
            get { return true; }
        }

        protected override Node GetNode(Type nodeType, InjectContext context)
        {
            if (_nodeBindInfo.Name == null)
            {
                _nodeBindInfo.Name = NodeType.Name;
            }

            return Container.CreateNodeOfType(_nodeBindInfo, nodeType, context);
        }
    }
}
#endif
