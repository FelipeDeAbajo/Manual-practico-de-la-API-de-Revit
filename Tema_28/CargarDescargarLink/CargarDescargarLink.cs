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

namespace CargarDescargarLink
{
    [Transaction(TransactionMode.Manual)]
    public class CargarDescargarLink : IExternalCommand
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

            //Buscanos RevitLinkType coincidente
            FilteredElementCollector col = new FilteredElementCollector(doc);
            RevitLinkType revitLinkType = col.OfClass(typeof(RevitLinkType)).Cast<RevitLinkType>().Where(x => x.Name == "Link.rvt").FirstOrDefault();

            //Creamos ElementId para RevitLinkType
            ElementId revitLinkTypeId = ElementId.InvalidElementId;

            //Si no existe
            if (revitLinkType == null)
            {
                message = "No existe el RevitLinkType";
                return Result.Failed;
            }
            else
            {
                //Si está leido
                if (revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
                {
                    revitLinkType.Unload(null);
                }
                //Si no está leido
                else if (revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.Unloaded)
                {
                    revitLinkType.Load();
                }
            }

            return Result.Succeeded;
        }
    }
}
