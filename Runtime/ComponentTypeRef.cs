using System;
using UnityEngine;

namespace USDGameReady
{
    [Serializable]
    public class ComponentTypeRef
    {
        [SerializeField] public string typeName;

        public Type Resolve()
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            return Type.GetType(typeName);
        }
    }
}
