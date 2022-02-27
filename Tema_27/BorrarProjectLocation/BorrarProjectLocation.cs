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

namespace BorrarProjectLocation
{
    [Transaction(TransactionMode.Manual)]
    public class BorrarProjectLocation : IExternalCommand
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

            //Obtenemos ProjectLocation
            ProjectLocation currentLocation = doc.ActiveProjectLocation;

            //Obtenemos set de ProjectLocation
            ProjectLocationSet locations = doc.ProjectLocations;

            if (1 == locations.Size)
            {
                message = "Solo tenemos un ProjectLocation";
                return Result.Cancelled;
            }

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name BorrarProjectLocation");

                //Mismo nombre que en ejemplo anteror
                string name = "Mi ProjectLocation";

                //No podemos borrar el actual
                if (name != currentLocation.Name)
                {
                    //Iteramos para cada ProjectLocation buscando el nombre
                    foreach (ProjectLocation projectLocation in locations)
                    {
                        if (projectLocation.Name == name)
                        {
                            //Borramos como un Element
                            ICollection<ElementId> elemSet = doc.Delete(projectLocation.Id);                          
                        }
                    }
                }

                TaskDialog.Show("Revit API Manual", "Mi ProjectLocation borrado");

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
