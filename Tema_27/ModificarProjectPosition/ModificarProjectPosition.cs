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

namespace ModificarProjectPosition
{
    [Transaction(TransactionMode.Manual)]
    public class ModificarProjectPosition : IExternalCommand
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
           ProjectPosition position = projectLocation.GetProjectPosition(origin);

            // Obtenemos datos.
            string prompt = "ProjectPosition original:\n";
            prompt += "\n\t" + "Origin point position:";
            prompt += "\n\t\t" + "Angle: " + position.Angle;
            prompt += "\n\t\t" + "East to West offset: " + position.EastWest;
            prompt += "\n\t\t" + "Elevation: " + position.Elevation;
            prompt += "\n\t\t" + "North to South offset: " + position.NorthSouth;

            //Constante factor Radianes <=> grados
            const double angleRatio = Math.PI / 180;  

            //Nuevos valores
            double angle = 20.0 * angleRatio;   
            double eastWest = 30.0;     
            double northSouth = 40;   
            double elevation = 50;

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name ModificarProjectPosition");

                //Creamos ProjectPosition con nuevos datos
                ProjectPosition newPosition = doc.Application.Create.NewProjectPosition(eastWest, northSouth, elevation, angle);

                //Si no es null
                if (null != newPosition)
                {
                    //Asignamos a ProjectLocation la nueva ProjectPosition
                    projectLocation.SetProjectPosition(origin, newPosition);

                    // Obtenemos datos.
                    prompt += "\n\t" ;
                    prompt += "\n\t" ;
                    prompt += "ProjectPosition modificada:\n";
                    prompt += "\n\t" + "Punto base:";
                    prompt += "\n\t\t" + "Angle: " + newPosition.Angle;
                    prompt += "\n\t\t" + "East to West offset: " + newPosition.EastWest;
                    prompt += "\n\t\t" + "Elevation: " + newPosition.Elevation;
                    prompt += "\n\t\t" + "North to South offset: " + newPosition.NorthSouth;
                }

                //Confirmamos Transaction
                tx.Commit();
            }

            TaskDialog.Show("Revit API Manual", prompt);

            return Result.Succeeded;
        }
    }
}
