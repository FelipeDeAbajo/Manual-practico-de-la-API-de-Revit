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

namespace DuplicarProjectLocation
{
    [Transaction(TransactionMode.Manual)]
    public class DuplicarProjectLocation : IExternalCommand
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


            string newName = "Mi ProjectLocation";

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name DuplicarProjectLocation");

                //Obtenemos ProjectLocation
                ProjectLocation currentLocation = doc.ActiveProjectLocation;

                //Obtenemos set de ProjectLocation
                ProjectLocationSet locations = doc.ProjectLocations;

                TaskDialog.Show("Revit API Manual", "Número de ProjectLocation antes: " + locations.Size);

                //Iteramos para buscar coincidencias
                foreach (ProjectLocation projectLocation in locations)
                {
                    if (projectLocation.Name == newName)
                    {
                        message = "El nombre ya está en uso";
                        return Result.Cancelled;
                    }
                }

                //Duplicamos ProjectLocation con nuevo nombre
                ProjectLocation project = currentLocation.Duplicate(newName);

                //Obtenemos set de ProjectLocation
                locations = doc.ProjectLocations;

                TaskDialog.Show("Revit API Manual", "Número de ProjectLocation después: "+ locations.Size);

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
