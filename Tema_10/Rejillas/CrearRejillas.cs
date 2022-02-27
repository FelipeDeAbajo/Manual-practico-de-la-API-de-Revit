#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace Rejillas
{
    [Transaction(TransactionMode.Manual)]
    public class CrearRejillas : IExternalCommand
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

            //Creamos una lista de curves
            List<Curve> curves = new List<Curve>();

            //Creamos 4 Line y las añadimos a la lista
            curves.Add(Line.CreateBound(new XYZ(-18, -20, 0), new XYZ(-18, 20, 0)));
            curves.Add(Line.CreateBound(new XYZ(18, -20, 0), new XYZ(18, 20, 0)));
            curves.Add(Line.CreateBound(new XYZ(20, -18, 0), new XYZ(-20, -18, 0)));
            curves.Add(Line.CreateBound(new XYZ(20, 18, 0), new XYZ(-20, 18, 0)));
          
            using (Transaction tx = new Transaction(doc, "Crear Grids"))
            {
                tx.Start();
                //Para cada Line creamos una Grid
                foreach (Line line in curves)
                    Grid.Create(doc, line);
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
