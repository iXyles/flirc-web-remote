using System.Runtime.InteropServices;

namespace FlircWrapper;

public static class LibResolver
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("libSystem.dylib", EntryPoint = "dlopen")]
    private static extern IntPtr dlopen_mac(string fileName, int flags);

    [DllImport("libSystem.dylib", EntryPoint = "dlsym")]
    private static extern IntPtr dlsym_mac(IntPtr handle, string symbol);

    [DllImport("libSystem.dylib", EntryPoint = "dlclose")]
    private static extern int dlclose_mac(IntPtr handle);

    [DllImport("libdl.so.2", EntryPoint = "dlopen")]
    private static extern IntPtr dlopen_linux(string fileName, int flags);

    [DllImport("libdl.so.2", EntryPoint = "dlerror")]
    private static extern IntPtr dlerror_linux();

    [DllImport("libdl.so.2", EntryPoint = "dlsym")]
    private static extern IntPtr dlsym_linux(IntPtr handle, string symbol);

    [DllImport("libdl.so.2", EntryPoint = "dlclose")]
    private static extern int dlclose_linux(IntPtr handle);

    private const int RtldNow = 2;

    public static IntPtr LoadLib(string dllName)
    {
        IntPtr hModule;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            hModule = LoadLibrary(dllName);
            if (hModule == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Failed to load library: {dllName}, Error Code: {errorCode}");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            hModule = dlopen_linux(dllName, RtldNow);
            if (hModule == IntPtr.Zero)
            {
                throw new Exception($"Failed to load library: {dllName} - {Marshal.PtrToStringAnsi(dlerror_linux())}");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            hModule = dlopen_mac(dllName, RtldNow);
            if (hModule == IntPtr.Zero)
            {
                throw new Exception($"Failed to load library: {dllName}");
            }
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform");
        }

        return hModule;
    }

    public static void AssignFunctionPointer<T>(IntPtr hModule, string functionName, out T delegateField) where T : Delegate
    {
        IntPtr pFunc;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            pFunc = GetProcAddress(hModule, functionName);
            if (pFunc == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Failed to get function pointer for {functionName}, Error Code: {errorCode}");
            }
        }
        else
        {
            pFunc = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? dlsym_linux(hModule, functionName)
                : dlsym_mac(hModule, functionName);

            if (pFunc == IntPtr.Zero)
            {
                throw new Exception($"Failed to get function pointer for {functionName}");
            }
        }

        delegateField = (T)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(T));
    }
}
