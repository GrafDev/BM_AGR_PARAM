using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace BM_AGR_Loader
{
    [Transaction(TransactionMode.Manual)]
    public class LoaderApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            const string tabName   = "BM Plugins";
            const string panelName = "BM Plugins";

            try { application.CreateRibbonTab(tabName); } catch { }

            RibbonPanel panel = null;
            foreach (RibbonPanel p in application.GetRibbonPanels(tabName))
                if (p.Name == panelName) { panel = p; break; }

            if (panel == null)
                panel = application.CreateRibbonPanel(tabName, panelName);

            string loaderDll = Assembly.GetExecutingAssembly().Location;

            // Кнопка теперь ВСЕГДА ссылается на LoaderCommand (в этой же DLL)
            PushButtonData btnData = new PushButtonData(
                "BM_AGR_PARAM_Check",
                "TRUE HOT\nReload",
                loaderDll,
                "BM_AGR_Loader.LoaderCommand")
            {
                ToolTip = "Настоящая горячая замена (Loader v1.1.0)"
            };

            panel.AddItem(btnData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
