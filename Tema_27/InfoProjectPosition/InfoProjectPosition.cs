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

namespace InfoProjectPosition
{
    [Transaction(TransactionMode.Manual)]
    public class InfoProjectPosition : IExternalCommand
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

            //Obtenemos el ProjectLocation
            ProjectLocation projectLocation = doc.ActiveProjectLocation;

            //Obtenemos ProjectPosition desde ProjectLocation 
            XYZ origin = new XYZ(0, 0, 0);
            Autodesk.Revit.DB.ProjectPosition position = projectLocation.GetProjectPosition(origin);
                  
            // Obtenemos datos.
            string prompt = "ProjectPosition actual:\n";
            prompt += "\n\t" + "Punto base:";
            prompt += "\n\t\t" + "Angulo: " + position.Angle;
            prompt += "\n\t\t" + "Este Oeste desfase: " + position.EastWest;
            prompt += "\n\t\t" + "Elevación: " + position.Elevation;
            prompt += "\n\t\t" + "Norte Sur desfase: " + position.NorthSouth;
          
            TaskDialog.Show("Revit API Manual", prompt);
           
            return Result.Succeeded;
        }
    }
}
