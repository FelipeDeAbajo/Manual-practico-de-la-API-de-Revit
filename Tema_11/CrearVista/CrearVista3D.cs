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

namespace CrearVista
{
    [Transaction(TransactionMode.Manual)]
    public class CrearVista3D : IExternalCommand
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

            // Access current selection
            //Filtro de clase ViewFamilyType

            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType));
            //Refinamos busqueda a ViewFamily.ThreeDimensional
            ViewFamilyType viewFamilyType = col.Cast<ViewFamilyType>().Where(x => x.ViewFamily == ViewFamily.ThreeDimensional).FirstOrDefault();

            // Modify document within a transaction
            // Creamos la transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos la Transaction
                tx.Start("Creación de vista");

                View3D mView3D = View3D.CreateIsometric(doc, viewFamilyType.Id);
                //No asignamos ninguna plantilla
                mView3D.ViewTemplateId = ElementId.InvalidElementId;
                //Establecemos nivel de detalle
                mView3D.DetailLevel = ViewDetailLevel.Fine;
                // Damos nombre a la vista
                mView3D.Name = "NuevaVista3D";
                TaskDialog.Show("API Revit Manual", "Nueva vista 3D creada." );
              
                tx.Commit();
            }

            return Result.Succeeded;
        }
       
    }
}
