﻿using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    public class EnumFlagsAttribute : PropertyAttribute
    {
        public string enumName;

        public EnumFlagsAttribute() { }

        public EnumFlagsAttribute(string name)
        {
            enumName = name;
        }
    }
}