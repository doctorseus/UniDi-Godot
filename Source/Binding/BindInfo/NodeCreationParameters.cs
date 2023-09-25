#if GODOT
using System;
using Godot;

namespace UniDi
{
    [NoReflectionBaking]
    public class NodeCreationParameters
    {
        public string Name
        {
            get;
            set;
        }

        public string GroupName
        {
            get;
            set;
        }

        public Node ParentNode
        {
            get;
            set;
        }

        public Func<InjectContext, Node> ParentNodeGetter
        {
            get;
            set;
        }

        public Vector3? Position
        {
            get;
            set;
        }

        public Quaternion? Rotation
        {
            get;
            set;
        }

        public static readonly NodeCreationParameters Default = new NodeCreationParameters();

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 29 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 29 + (GroupName == null ? 0 : GroupName.GetHashCode());
                hash = hash * 29 + (ParentNode == null ? 0 : ParentNode.GetHashCode());
                hash = hash * 29 + (ParentNodeGetter == null ? 0 : ParentNodeGetter.GetHashCode());
                hash = hash * 29 + (!Position.HasValue ? 0 : Position.Value.GetHashCode());
                hash = hash * 29 + (!Rotation.HasValue ? 0 : Rotation.Value.GetHashCode());
                return hash;
            }
        }

        public override bool Equals(object other)
        {
            if (other is NodeCreationParameters)
            {
                NodeCreationParameters otherId = (NodeCreationParameters)other;
                return otherId == this;
            }

            return false;
        }

        public bool Equals(NodeCreationParameters that)
        {
            return this == that;
        }

        public static bool operator ==(NodeCreationParameters left, NodeCreationParameters right)
        {
            return Equals(left.Name, right.Name)
                && Equals(left.GroupName, right.GroupName);
        }

        public static bool operator !=(NodeCreationParameters left, NodeCreationParameters right)
        {
            return !left.Equals(right);
        }
    }
}
#endif
