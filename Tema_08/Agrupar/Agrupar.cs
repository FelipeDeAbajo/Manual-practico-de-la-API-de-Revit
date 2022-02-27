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

namespace Agrupar
{
    [Transaction(TransactionMode.Manual)]
    public class Agrupar : IExternalCommand
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

            // Accedemos a la selección actual
            Selection sel = uidoc.Selection;

            // Chequeamos que solo tenemos un objeto seleccionado
            if (sel.GetElementIds().Count == 0)
            {
                message = "Se debe seleccionar al menos un elemento";
                return Result.Failed;
            }

            // Creamos transaction
            using (Transaction tx = new Transaction(doc))
            {
                // Iniciamos transaction

                tx.Start("Transaction grupos");
                //Creamos el grupo con los objetos seleccionadoos
                Group group = doc.Create.NewGroup(sel.GetElementIds());
                //Cambiamos el nombre al GroupType
                group.GroupType.Name = "GrupoAPI";

                //Creamos una nueva instancia del grupo.
                Group newGroup = doc.Create.PlaceGroup(XYZ.Zero, group.GroupType);

                //Obtenemos los ElementId de los objetos
                IList<ElementId> elementIds = group.GetMemberIds();
                IList<ElementId> elementIdsNew = newGroup.GetMemberIds();

                //Desagrupamos el grupo inicial
                ICollection<ElementId> elementIdsOriginales = group.UngroupMembers();
                //Confirmamos transaction
                tx.Commit();
            }

            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Grupos creados");
            return Result.Succeeded;
        }
    }
}