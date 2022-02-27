#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace Recorrido
{
    [Transaction(TransactionMode.Manual)]
    public class Recorrido : IExternalCommand
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

            //Seleccionamos vista actual
            ViewPlan viewPlan = doc.ActiveView as ViewPlan;

            //Si vista actual es null
            if(viewPlan ==null)
            {
                message = "Solo vistas en planta";
                return Result.Failed;
            }

            //Creamos listas de puntos inicio y destino
            IList<XYZ> sourcePoints = new List<XYZ>();
            IList<XYZ> endPoints = new List<XYZ>();

            try
            {
                //Seleccionamos hanitación. Considere incluir un ISelectionFilter
                Reference roomReference = uidoc.Selection.PickObject(ObjectType.Element, "Seleccionar una habitación.");
                Room room = doc.GetElement(roomReference) as Room;
                sourcePoints.Add((room.Location as LocationPoint).Point);

                //Seleccionamos puerta de salida. Considere incluir un ISelectionFilter
                Reference doorReference = uidoc.Selection.PickObject(ObjectType.Element, "Seleccionar la puerta se salida.");
                FamilyInstance doorElement = doc.GetElement(doorReference) as FamilyInstance;
                endPoints.Add((doorElement.Location as LocationPoint).Point);
            }
            catch(Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

           //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Recorrido");
                
                //Lista para almacenar los status
                IList<PathOfTravelCalculationStatus> statuses;             
                //Creamos los Análisis de ruta
                IList<PathOfTravel> pathsOfTravel = PathOfTravel.CreateMapped(viewPlan, sourcePoints, endPoints, out statuses);

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
