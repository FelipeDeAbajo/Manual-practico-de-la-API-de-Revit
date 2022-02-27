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

namespace ExtensibleStorage
{
    [Transaction(TransactionMode.Manual)]
    public class ExtensibleStorage : IExternalCommand
    {
        //Creamos un GUID
        internal static Guid guidSchema = new Guid("4d8a80d3-e1c3-4b83-ffff-ce975e420529");

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // Seleccionamos algunos objetos y creamos en ellos un Schema
            IList<Element> elementSelect = null;
            try
            {
                //Hacemos una selección rectangular
                elementSelect = uidoc.Selection.PickElementsByRectangle("Selecciona objetos por rectángulo");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            //Creamos el Schema en cada objeto seleccionado y modificamos el doc; debemos utilizar una Transacion
            //using (Transaction transaction = new Transaction(doc,"Crear Schema"))
            //{
            //    transaction.Start();
            //    elementSelect.ToList().ForEach(x => CrearSchema(x));
            //    transaction.Commit();
            //}
            //// Hasta aqui es una fase previa para crear las condiciones en el modelo
            //return Result.Succeeded;
            //Creamos el filtro con el GUID 
            ExtensibleStorageFilter filterExtensibleStorage = new ExtensibleStorageFilter(guidSchema);

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Aplicamos el filtro a los elementos del documento activo

            collector.WherePasses(filterExtensibleStorage).ToElements();

            IList<Element> elementsSet = collector.ToElements();

            List<string> names = elementsSet.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI tienen el Schema");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }

        internal static void CrearSchema(Element element)
        {
           
            Schema schema = Schema.Lookup(guidSchema);
            if (schema == null)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(guidSchema);
                schemaBuilder.SetVendorId("FDeA");
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);//Establecemos acceso
                schemaBuilder.SetSchemaName("TestFilterExtensibleStorage");
                schemaBuilder.SetDocumentation("Empleado para test del filtro ExtensibleStorage");
                //Creanmos el campo para almacenar string
                FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("TestFilterValue1", typeof(string));
                schema = schemaBuilder.Finish();
            }
            Entity entity = new Entity(schema);
            entity.Set<string>("TestFilterValue1", "Poner un valor");
            if (element != null) element.SetEntity(entity);
        }
    }
}
