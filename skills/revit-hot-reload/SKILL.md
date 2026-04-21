---
name: revit-hot-reload
description: Instruction and code for developing Revit plugins without restarting Revit (Hot Reload/Assembly Loader pattern). Use when developers need to iterate quickly on code and UI without closing Revit between compilations.
---

# Revit Hot Reload (Assembly Loader)

Revit locks DLL files when they are loaded via the `.addin` manifest or standard `Assembly.LoadFile`. To bypass this and allow recompilation while Revit is running, use the **Assembly Loader** pattern.

## 1. The Core Concept

Instead of Revit loading `Plugin.dll` directly, it loads a small, stable **Loader.dll** (which never changes). When you click a button, the **Loader** reads the bytes of the actual `Plugin.dll`, loads them into memory, and executes the command.

## 2. Implementation Template (The Loader)

Create a "Stub" command in your project that acts as the entry point during development:

```csharp
[Transaction(TransactionMode.Manual)]
public class HotReloadCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // 1. Path to your actual compiled DLL (ensure it's the Release/Debug path)
        string dllPath = @"C:\Users\g.yakovlev\Documents\PLAGINS\BM_AGR_PARAM\bin\Release\BM_AGR_PARAM.dll";
        
        try
        {
            // 2. Read bytes to avoid file locking
            byte[] assemblyBytes = File.ReadAllBytes(dllPath);
            
            // 3. Load into memory
            Assembly assembly = Assembly.Load(assemblyBytes);
            
            // 4. Find and execute the target command
            // Note: You must use the full name (Namespace.Class)
            var type = assembly.GetType("BM_AGR_PARAM.Commands.CheckCommand");
            var method = type.GetMethod("Execute");
            
            object instance = Activator.CreateInstance(type);
            object[] parameters = new object[] { commandData, message, elements };
            
            return (Result)method.Invoke(instance, parameters);
        }
        catch (Exception ex)
        {
            message = ex.ToString();
            return Result.Failed;
        }
    }
}
```

## 3. Workflow for Developers

1.  **Rebuild** your project in Visual Studio / VS Code.
2.  **Click the Button** in Revit.
3.  The Loader will pick up the latest `.dll` bytes and run the new code.
4.  **No Restart Needed**.

## 4. Key Priorities
- **Namespace Consistency**: The Loader must know exactly which class and method to invoke in the target DLL.
- **Dependencies**: Any dependent DLLs (like Newtonsoft.Json) must either be in the same folder or loaded manually into the AppDomain.
- **PDB Files**: For debugging to work, ensure `.pdb` files are generated and potentially loaded similarly (or Revit will look for them next to the DLL).
- **Cleanup**: In-memory loading can consume memory over time (AppDomains can't be easily unloaded). Use this Primarily for **Development**, not for production.

## 5. Alternative: Add-In Manager
For most developers, using the **Autodesk Add-In Manager** (part of Revit SDK) is the easiest way to achieve this without writing custom loader code.
