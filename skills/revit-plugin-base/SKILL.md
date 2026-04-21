---
name: revit-plugin-base
description: Standard structure for Revit 2023 plugins following the BM standard. Includes folder organization, IExternalApplication setup, and 'BM Plugins' ribbon tab/panel creation. Use when starting a new Revit plugin project or refactoring an existing one to meet BM standards.
---

# Revit Plugin Base (BM Standard)

This skill guides the creation of a standardized Revit 2023 plugin project using C# and .NET 6 (or 4.8).

## 1. Project Structure

Organize your project files into the following folders for clarity and consistency:

```text
MyPlugin/
├── Commands/      - Classes implementing IExternalCommand
├── Handlers/      - ExternalEvent handlers for Revit API interactions from WPF
├── Models/        - Data structures and business entities
├── Services/      - Business logic and API utility services
├── UI/            - XAML files and code-behind for the interface
├── Resources/     - Icons, images, and static assets
├── App.cs         - Main entry point (IExternalApplication)
├── version.txt    - Plain text version (e.g., 1.0.0)
└── MyPlugin.addin - Revit manifest file
```

## 2. Ribbon Creation ("BM Plugins")

All BM plugins must live under the **"BM Plugins"** tab. Use the following pattern in your `App.cs` to ensure the tab is created only once and the panel is shared.

### App.cs Implementation

```csharp
public class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        CreateRibbon(application);
        return Result.Succeeded;
    }

    private void CreateRibbon(UIControlledApplication app)
    {
        const string tabName   = "BM Plugins";
        const string panelName = "BM Plugins";

        // Create Tab (or catch if exists)
        try { app.CreateRibbonTab(tabName); } catch { }

        // Find or Create Panel
        RibbonPanel panel = null;
        foreach (RibbonPanel p in app.GetRibbonPanels(tabName))
        {
            if (p.Name == panelName) { panel = p; break; }
        }
        if (panel == null)
            panel = app.CreateRibbonPanel(tabName, panelName);

        string dll = typeof(App).Assembly.Location;

        // Add Button
        PushButtonData btnData = new PushButtonData(
            "MyPlugin_MainBtn",
            "Plugin\nName",
            dll,
            "MyPlugin.Commands.MainCommand")
        {
            ToolTip = "Description of the plugin",
            LargeImage = LoadResourceImage("icon.png")
        };

        panel.AddItem(btnData);
    }

    private BitmapSource LoadResourceImage(string name)
    {
        // Use pack URI to load embedded resource from 'BM_RevitPlugin_SharedIcons' or similar
        string path = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/Resources/{name}";
        BitmapImage image = new BitmapImage(new Uri(path));
        return image;
    }
}
```

## 3. Revit Add-in Manifest (.addin)

Ensure the `.addin` file uses the full path to the DLL (usually handled by the installer).

```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>My Plugin Name</Name>
    <Assembly>C:\Path\To\MyPlugin.dll</Assembly>
    <AddInId>Generate-A-New-GUID-Here</AddInId>
    <FullClassName>MyNamespace.App</FullClassName>
    <VendorId>BM</VendorId>
    <VendorDescription>BuroMoscow</VendorDescription>
  </AddIn>
</RevitAddIns>
```

## 4. Key Priorities
1. **Consistency**: Always use the "BM Plugins" tab.
2. **Icons**: Use high-quality 32x32 PNG icons for buttons.
3. **Threading**: Use `ExternalEvent` when calling Revit API from WPF windows.
