using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Runtime.Loader;

public class CustomAssemblyLoadContext : AssemblyLoadContext
{
    public IntPtr LoadUnmanagedLibrary(string absolutePath)
    {
        return LoadUnmanagedDll(absolutePath);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        return LoadLibrary(unmanagedDllName);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        return null;
    }

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("libdl", SetLastError = true)]
    private static extern IntPtr dlopen(string fileName, int flags);
}