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

namespace Localizacion
{
    [Transaction(TransactionMode.Manual)]
    public class Localizacion : IExternalCommand
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

            // Get the project location handle 
            ProjectLocation projectLocati = doc.ActiveProjectLocation;

            // Show the information of current project location
            XYZ origin = new XYZ(0, 0, 0);
            ProjectPosition position = projectLocati.GetProjectPosition(origin);
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

            prompt += "\n\t";
            prompt += "\n\t";
            prompt += "SiteLocation del proyecto actual después:";
            prompt += "\n\t" + "Latitud: " + site.Latitude / angleRatio + " grados";
            prompt += "\n\t" + "Longitud: " + site.Longitude / angleRatio + " grados";
            prompt += "\n\t" + "Nombre: " + site.PlaceName;
            prompt += "\n\t" + "Estación metereológica: " + site.WeatherStationName;

            TaskDialog.Show("Revit API Manual", prompt);
            CitySet cities = doc.Application.Cities;


            foreach (City city in cities)
            {
                    if (city.Name.Contains("Barcelona") )
                    {
                        Transaction transaction = new Transaction(doc, "Transaction Name City");
                        transaction.Start();

                        ProjectLocation projectLocation = doc.ActiveProjectLocation;

                         site = projectLocation.GetSiteLocation(); 

                        // site.PlaceName = city.Name;
                        site.Latitude = city.Latitude; // latitude information
                        site.Longitude = city.Longitude; // longitude information
                        site.TimeZone = city.TimeZone; // TimeZone information
                        transaction.Commit();
                        break;
                    }
                }
           
            return Result.Succeeded;
        }
    }
}
