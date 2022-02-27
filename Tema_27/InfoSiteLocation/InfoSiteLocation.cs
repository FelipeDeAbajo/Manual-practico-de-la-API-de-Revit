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

namespace InfoSiteLocation
{
    [Transaction(TransactionMode.Manual)]
    public class InfoSiteLocation : IExternalCommand
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
           
            //Obtenemos la SiteLocation.
           SiteLocation site = doc.SiteLocation;

            //Constante para convertir radianes <=> grados
            const double angleRatio = Math.PI / 180; 

            //Información actual. 
            string prompt = "SiteLocation del proyecto actual antes:";
            prompt += "\n\t" + "Latitud: " + site.Latitude / angleRatio + " grados";
            prompt += "\n\t" + "Longitud: " + site.Longitude / angleRatio + " grados";
            prompt += "\n\t" + "Nombre: " + site.PlaceName ;
            prompt += "\n\t" + "Estación metereológica: " + site.WeatherStationName;

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name SiteLocation");

                //Cambiamos SiteLocation latitud, longitud y timeZone.
                site.Latitude = 0.5;   
                site.Longitude = 0.5;   
                site.TimeZone = -5;    
                //Confirmamos Transaction
                tx.Commit();
            }

            //Información actualizada. 
            prompt += "\n\t";
            prompt += "\n\t";
            prompt += "SiteLocation del proyecto actual después:";
            prompt += "\n\t" + "Latitud: " + site.Latitude / angleRatio + " grados";
            prompt += "\n\t" + "Longitud: " + site.Longitude / angleRatio + " grados";
            prompt += "\n\t" + "Nombre: " + site.PlaceName;
            prompt += "\n\t" + "Estación metereológica: " + site.WeatherStationName;

            TaskDialog.Show("Revit API Manual", prompt);
                     
            return Result.Succeeded;
        }
    }
}
