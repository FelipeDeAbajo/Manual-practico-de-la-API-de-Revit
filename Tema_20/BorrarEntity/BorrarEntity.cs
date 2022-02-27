#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace BorrarEntity
{
    [Transaction(TransactionMode.Manual)]
    public class BorrarEntity : IExternalCommand
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

            //Iniciamos con un Wall seleccionado
            Selection sel = uidoc.Selection;
            ElementId id = sel.GetElementIds().FirstOrDefault();
            if (id is null)
            {
                message = "Se debe iniciar con un muro seleccionado";
                return Result.Cancelled;
            }

            //Obtenemos el muro
            Wall wall = doc.GetElement(id) as Wall;
            if (wall is null)
            {
                message = "Se debe iniciar con un muro seleccionado";
                return Result.Cancelled;
            }
            //Obtenemos los Schemas en memoria. 
            IList<Schema> schemasPre = Schema.ListSchemas();

            //Obtenenos los GUID de Schemas en muro
            IList<Guid> guids = wall.GetEntitySchemaGuids();
            Schema schema = null;

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamoa Transaction
                tx.Start("Transaction Borrar Schema y Entity");
                //Primer GUID
                Guid guid = guids.FirstOrDefault();
                //Obtenemos primer Schema en muro
                schema = Schema.Lookup(guid);
                //Es nulo?
                if (schema != null)
                {
                    //Borramos Entity en solo este muro
                    wall.DeleteEntity(schema);

                }

                //Ultimo GUID
                guid = guids.LastOrDefault();
                //Obtenemos ultimo Schema en muro
                schema = Schema.Lookup(guid);
                //Es nulo?
                if (schema != null)
                {
                    //Borramos Schema de todo el document
                    doc.EraseSchemaAndAllEntities(Schema.Lookup(guid));
                }
                //Confirmamos Transaction
                tx.Commit();
            }

            //Obtenemos los Schemas en memoria. 
            IList<Schema> schemasPost = Schema.ListSchemas();

            TaskDialog.Show("Revit API Manual", "Schemas iniciales: " + schemasPre.Count+ "\n"+String.Join("\n", schemasPre.Select(x => x.SchemaName).ToList()) +
                 "\n\nSchemas finales: " + schemasPost.Count + "\n" + String.Join("\n", schemasPost.Select(x => x.SchemaName).ToList()));
            return Result.Succeeded;
        }
    }
}
