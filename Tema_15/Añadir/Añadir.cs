#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace Añadir
{
    [Transaction(TransactionMode.Manual)]
    public class Añadir : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            View view = uidoc.ActiveView;

            ColorFillScheme scheme = doc.GetElement(view.GetColorFillSchemeId(new ElementId(BuiltInCategory.OST_Rooms))) as ColorFillScheme;

            IList<ColorFillSchemeEntry> entries = scheme.GetEntries();
            foreach (ColorFillSchemeEntry entry in entries)
            {
                entry.FillPatternId = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .First(a => a.Name == "Acero")
                .Id;
            }
            using (Transaction tx = new Transaction(doc ))
            {
                tx.Start("Transaction Modificar patrón");
                scheme.SetEntries(entries);
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
