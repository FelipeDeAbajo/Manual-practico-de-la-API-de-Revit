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

namespace CrearVistaPLano
{
    [Transaction(TransactionMode.Manual)]
    public class CrearVistaPLano : IExternalCommand
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

            //Filtro de clase ViewFamilyType
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(View));
            //Refinamos busqueda a ViewType.CeilingPlan
            ElementId viewFamilyTypeId = col.Cast<View>().Where(x => x.ViewType == ViewType.CeilingPlan).FirstOrDefault().GetTypeId();

            //Filtro de clase Level
             col = new FilteredElementCollector(doc).OfClass(typeof(Level));
            
            Level level = col.FirstOrDefault() as Level;


            // Modify document within a transaction
            // Creamos la transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos la Transaction
                tx.Start("Creación de vista");

                ViewPlan mViewPlant = ViewPlan.Create(doc, viewFamilyTypeId, level.Id);
                //No asignamos ninguna plantilla
                mViewPlant.ViewTemplateId = ElementId.InvalidElementId;
                //Establecemos nivel de detalle
                mViewPlant.DetailLevel = ViewDetailLevel.Fine;
                // Damos nombre a la vista
                mViewPlant.Name = "NuevaVistaTecho";
                TaskDialog.Show("API Revit Manual", "Nueva vista de planta creada.");

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
