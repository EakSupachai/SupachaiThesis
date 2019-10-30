using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bci2000Api
{
    public static class Interop
    {
        public const string api_dll = "Bci2000Api";

        [DllImport(api_dll, EntryPoint = "Create")]
        public static extern IntPtr Create();

        [DllImport(api_dll, EntryPoint = "UseSetState")]
        public static extern void UseSetState(IntPtr api, int value);
    }
}