using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BM_AGR_PARAM.UI;

namespace BM_AGR_PARAM.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CheckCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            CheckWindow window = new CheckWindow(doc);
            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}
