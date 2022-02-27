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

namespace CambiarWorkset
{
    [Transaction(TransactionMode.Manual)]
    public class CambiarWorkset : IExternalCommand
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

            //Nombre del Workset al que cambiar
            string targetWorksetName = "Nuevo Workset";

            //Buscamos workset destino
            FilteredWorksetCollector worksetCollector = new FilteredWorksetCollector(doc);
            worksetCollector.OfKind(WorksetKind.UserWorkset);
            Workset workset = worksetCollector.FirstOrDefault<Workset>(ws => ws.Name == targetWorksetName);

            //Si no existe cancelamos
            if (workset == null)
            {
                message = string.Format("No se ha encontrado el workset, {0} en el proyecto.", targetWorksetName);
                return Result.Failed;
            }

            //Creamos nivel
            Level level = null;
            //Obtenemos nivel
            if (doc.ActiveView is ViewPlan view)
            {
                level = view.GenLevel;
            }
            else
            {
                message = "Solo en vistas en planta";
                return Result.Failed;
            }

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction CambiarWorkset");

                //Creamos un muro
                Wall wall = Wall.Create(doc, Line.CreateBound(XYZ.Zero, new XYZ(10, 0, 0)), level.Id, false);
                
                //Obtenemos parámetro del Workset
                Parameter parameter = wall.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);

                //Asignamos valor. Debe ser int
                parameter.Set(workset.Id.IntegerValue);

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
