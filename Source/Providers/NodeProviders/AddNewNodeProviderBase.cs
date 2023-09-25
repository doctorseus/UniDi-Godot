#if GODOT
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using UniDi.Internal;

namespace UniDi
{
    [NoReflectionBaking]
    public abstract class AddNewNodeProviderBase  : IProvider
    {
        readonly Type _nodeType;
        readonly DiContainer _container;
        readonly List<TypeValuePair> _extraArguments;
        readonly object _concreteIdentifier;
        readonly Action<InjectContext, object> _instantiateCallback;

        public AddNewNodeProviderBase(
            DiContainer container, Type nodeType,
            IEnumerable<TypeValuePair> extraArguments,
            object concreteIdentifier,
            Action<InjectContext, object> instantiateCallback)
        {
            Assert.That(nodeType.DerivesFrom<Node>());

            _extraArguments = extraArguments.ToList();
            _nodeType = nodeType;
            _container = container;
            _concreteIdentifier = concreteIdentifier;
            _instantiateCallback = instantiateCallback;
        }

        public bool IsCached
        {
            get { return false; }
        }

        public bool TypeVariesBasedOnMemberType
        {
            get { return false; }
        }

        protected DiContainer Container
        {
            get { return _container; }
        }

        protected Type NodeType
        {
            get { return _nodeType; }
        }

        protected abstract bool ShouldToggleActive
        {
            get;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _nodeType;
        }

        public void GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction, List<object> buffer)
        {
            Assert.IsNotNull(context);

            object instance;

            if (!_container.IsValidating || TypeAnalyzer.ShouldAllowDuringValidation(_nodeType))
            {
                instance = GetNode(_nodeType, context);

                Assert.IsNotNull(instance);
            }
            else
            {
                instance = new ValidationMarker(_nodeType);
            }

            injectAction = () =>
            {
                var extraArgs = UniDiPools.SpawnList<TypeValuePair>();

                extraArgs.AllocFreeAddRange(_extraArguments);
                extraArgs.AllocFreeAddRange(args);

                _container.InjectExplicit(instance, _nodeType, extraArgs, context, _concreteIdentifier);

                Assert.That(extraArgs.Count == 0);

                UniDiPools.DespawnList(extraArgs);

                if (_instantiateCallback != null)
                {
                    _instantiateCallback(context, instance);
                }
            };

            buffer.Add(instance);
        }

        protected abstract Node GetNode(Type nodeType, InjectContext context);
    }
}
#endif
