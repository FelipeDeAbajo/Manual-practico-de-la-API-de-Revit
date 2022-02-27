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

namespace VisibilidadVista
{
    [Transaction(TransactionMode.Manual)]
    public class VisibilidadVista : IExternalCommand
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

            //Obtenemos vista actual
            View view = uidoc.ActiveView;

            //Obtenemos subproyectos e información básica para cada uno
            FilteredWorksetCollector collector = new FilteredWorksetCollector(doc);

            //Buscamos todos los subproyectos de usuario
            collector.OfKind(WorksetKind.UserWorkset);
            ICollection<WorksetId> worksetsIds = collector.ToWorksetIds();

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");

                //Cambiamos para cada subproyecto, su visibilidad
                foreach (WorksetId worksetsId in worksetsIds)
                {
                    //Obtenemos la visibilidad actual. 
                    WorksetVisibility visibility = view.GetWorksetVisibility(worksetsId);

                    //Permutamos su visibilidad (Visible o UseGlobalSetting)
                    if (visibility != WorksetVisibility.Hidden)
                    {
                        view.SetWorksetVisibility(worksetsId, WorksetVisibility.Hidden);
                    }
                    else if(visibility == WorksetVisibility.Hidden)
                    {
                        view.SetWorksetVisibility(worksetsId, WorksetVisibility.Visible);
                    }

                    //Obtenemos la visibilidad para todas las vistas 
                    WorksetDefaultVisibilitySettings defaultVisibility = WorksetDefaultVisibilitySettings.GetWorksetDefaultVisibilitySettings(doc);

                    //Permutamos la visibilidad para todas las vistas 
                    if (true == defaultVisibility.IsWorksetVisible(worksetsId))
                    {
                        defaultVisibility.SetWorksetVisibility(worksetsId, false);
                    }
                    else
                    {
                        defaultVisibility.SetWorksetVisibility(worksetsId, true);
                    }
                }

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
