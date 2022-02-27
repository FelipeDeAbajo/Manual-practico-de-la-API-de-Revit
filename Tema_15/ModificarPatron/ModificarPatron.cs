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

namespace ModificarPatron
{
    [Transaction(TransactionMode.Manual)]
    public class ModificarPatron : IExternalCommand
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

            //Obtenemos la View actual
            View view = uidoc.ActiveView;

            //Obtenemos el ColorFillScheme, de la View actual asociada a Habitaciones
            ColorFillScheme scheme = doc.GetElement(view.GetColorFillSchemeId(new ElementId(BuiltInCategory.OST_Rooms))) as ColorFillScheme;
            //Obtenemos todas las entradas del Esquema
            IList<ColorFillSchemeEntry> entries = scheme.GetEntries();

            foreach (ColorFillSchemeEntry entry in entries)
            {
                //Si es la nueva entrada le cambio el patron
                if (entry.GetStringValue()!="Nuevo acabado") continue;
                entry.FillPatternId = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .ElementAt(1).Id;//Tomo el de indice 1
            }

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Modificar patrón");

                //Asignamos las entradas modificada
                scheme.SetEntries(entries);

                //Confirmamos Transaction
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
