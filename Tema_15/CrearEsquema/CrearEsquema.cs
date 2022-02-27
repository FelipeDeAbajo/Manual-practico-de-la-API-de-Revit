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

namespace CrearEsquema
{
    [Transaction(TransactionMode.Manual)]
    public class CrearEsquema : IExternalCommand
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

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Scheme");

                //Duplicamos el actual ColorFillScheme
                ElementId newSchemeId = scheme.Duplicate("Acabado suelo");
                ColorFillScheme newScheme = doc.GetElement(newSchemeId) as ColorFillScheme;
              
                //Aplicamos titulo
                newScheme.Title = "Acabado suelo";

                //Asociamos al parámetro Acabado de la base
                newScheme.ParameterDefinition = new ElementId(BuiltInParameter.ROOM_FINISH_BASE);

                //Asignamos a la vista, para las Habitaciones el ColorFillScheme, recien creado
                view.SetColorFillSchemeId(new ElementId(BuiltInCategory.OST_Rooms), newSchemeId);

                //Creamos una nueva entrada, Rojo y relleno sólido
                ColorFillSchemeEntry entry = new ColorFillSchemeEntry(StorageType.String)
                {
                    Color = new Color(250, 0, 0),
                    FillPatternId = new FilteredElementCollector(doc)
                               .OfClass(typeof(FillPatternElement))
                               .Cast<FillPatternElement>()
                               .First(a => a.GetFillPattern().IsSolidFill)
                               .Id
                };

                //asignamos valor a la entrada
                entry.SetStringValue("Nuevo acabado");
                //Añadimos al nuevo ColorFillScheme la entrada recien creada
                newScheme.AddEntry(entry);

                //Confirmamos Transaction
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
