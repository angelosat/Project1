using System;

namespace Project1.Core.Events;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false)]
internal sealed class NetworkReplicateAttribute : Attribute;
