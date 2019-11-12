using System;
using System.Runtime.InteropServices;

namespace Bci2000Api
{
    public static class Interop
    {
        public const string api_dll = "Bci2000Api";

        [DllImport(api_dll, EntryPoint = "Create")]
        public static extern IntPtr Create();

        [DllImport(api_dll, EntryPoint = "UseAssignSendAddress")]
        public static extern void UseAssignSendAddress(IntPtr api, int ip1, int ip2, int ip3, int ip4, int socket);

        [DllImport(api_dll, EntryPoint = "UseOpenSendConnection")]
        public static extern void UseOpenSendConnection(IntPtr api);

        [DllImport(api_dll, EntryPoint = "UseCloseSendConnection")]
        public static extern void UseCloseSendConnection(IntPtr api);

        [DllImport(api_dll, EntryPoint = "UseSetRecordingState")]
        public static extern void UseSetRecordingState(IntPtr api, int value);

        [DllImport(api_dll, EntryPoint = "UseSetClassifyingState")]
        public static extern void UseSetClassifyingState(IntPtr api, int value);

        [DllImport(api_dll, EntryPoint = "UseIsSendConnectionOpen")]
        public static extern bool UseIsSendConnectionOpen(IntPtr api);
    }
}