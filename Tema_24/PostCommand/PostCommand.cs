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

namespace PostCommand
{
    [Transaction(TransactionMode.Manual)]
    public class PostCommand : IExternalCommand
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

            //Creamos un colector. Filtro WallType
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(WallType));

            //Obtenemos el primer WallType
            WallType wallType = col.ToElements().FirstOrDefault() as WallType;

            //Obtenemos el tipo por defecto. No tiene por que coincidir
            ElementId elementIdIni = doc.GetDefaultElementTypeId(ElementTypeGroup.WallType);
            if (wallType.Id != elementIdIni)
            {
                //Definimos Transaction
                using (Transaction tx = new Transaction(doc))
                {
                    //Iniciamos Transaction
                    tx.Start("Transaction Name");

                    //Cambiamos tipo por defecto
                    doc.SetDefaultElementTypeId(ElementTypeGroup.WallType, wallType.Id);

                    //Confirmamos Transaction
                    tx.Commit();
                }
            }

            ////Seleccionamos el RevitCommandId correspondiente a creación de muro
            //RevitCommandId revitCommandId = RevitCommandId.LookupPostableCommandId(PostableCommand.ArchitecturalWall);

            ////Iniciamos la construcción de muros desde UIApplication
            //uiapp.PostCommand(revitCommandId);

            // Seleccionamos el RevitCommandId correspondiente a creación de Región de máscara
            RevitCommandId revitCommandId = RevitCommandId.LookupPostableCommandId(PostableCommand.MaskingRegion);

           // Iniciamos la creación de Región de máscara desde UIApplication
            uiapp.PostCommand(revitCommandId);
            return Result.Succeeded;
        }
    }
}
