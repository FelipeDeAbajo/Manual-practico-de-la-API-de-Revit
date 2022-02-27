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

namespace ExportDWG
{
    [Transaction(TransactionMode.Manual)]
    public class ExportDWG : IExternalCommand
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

            //Nobre de DWGExportOptions a buscar
            string setupName = "Revit API Manual";

            //Creamos DWGExportOptions
            DWGExportOptions dwgOptions = null;

            //Obtenemos lista nombres de DWGExportOptions
            IList<string> setupNames = BaseExportOptions.GetPredefinedSetupNames(doc);

            //Iteramos en la lista
            foreach (string name in setupNames)
            {
                //Si hay DWGExportOptions setupNameLínea 30. Creamos un nombre con el que buscaremos 
                if (name == setupName)
                {
                    //Recuperamos DWGExportOptions
                    dwgOptions = DWGExportOptions.GetPredefinedOptions(doc, name);
                }
            }
            //Incluimos la vista actual en una lista
            ICollection<ElementId> views = new List<ElementId>() { doc.ActiveView.Id };

            //Necisitanos path. Por ejemplo del rvt
            if (string.IsNullOrEmpty(doc.PathName))
            {
                message = "Proyecto no salvado";
                return Result.Cancelled;
            }

            //Exportamos en mismo path y mismo nombre
            bool exported = doc.Export(System.IO.Path.GetDirectoryName(doc.PathName),
                  System.IO.Path.GetFileNameWithoutExtension(doc.PathName), views, dwgOptions);

            return Result.Succeeded;
        }
    }
}
