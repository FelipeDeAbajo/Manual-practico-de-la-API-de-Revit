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

namespace CrearSchemaEntity
{
    [Transaction(TransactionMode.Manual)]
    public class CrearSchemaEntity : IExternalCommand
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

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Schema");

                Guid guid = Guid.Empty;
                #region Simple_double
                guid = new Guid("4d8a80d3-e1c3-4b83-ada1-ce975e420529");
                //Recuperamos Schema
                Schema schema = Schema.Lookup(guid);
                //Si es null, le creamos. Si no es null genera error
                if (schema == null)
                {
                    //Construimos Schema
                    SchemaBuilder schemaBuilder = new SchemaBuilder(guid);
                    schemaBuilder.SetVendorId("FdAA");
                    schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                    schemaBuilder.SetSchemaName("EspesorMuro");
                    schemaBuilder.SetDocumentation("Schema un Field. Simple <double> = Espesor del muro");

                    //Construimos Field
                    FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("CampoEspesorMuro", typeof(double));
                    fieldBuilder.SetSpec(SpecTypeId.Length); //V2022
                    // fieldBuilder.SetUnitType(UnitType.UT_Length);/V2021

                    //Finalizamos constructor Field
                    schema = schemaBuilder.Finish();
                }
                double espesor = wall.Document.GetElement(wall.GetTypeId()).get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble();

                //Construimos Entity.
                Entity entity = new Entity(schema);
                //Asignamos datos V2022 UnitTypeId.Feet | V2021 DisplayUnitType.DUT_DECIMAL_FEET
                entity.Set<double>("CampoEspesorMuro", espesor, UnitTypeId.Feet /*DisplayUnitType.DUT_DECIMAL_FEET*/);
                //Asociamos a wall
                if (entity.IsValid()) wall.SetEntity(entity);
                #endregion

                #region Array_double
                guid = new Guid("5d8a80d3-e1c3-4b83-ada1-ce975e420529");
                //Recuperamos Schema
                schema = Schema.Lookup(guid);
                //Si es null, le creamos. Si no es null genera error
                if (schema == null)
                {
                    //Construimos Schema
                    SchemaBuilder schemaBuilder = new SchemaBuilder(guid);
                    schemaBuilder.SetVendorId("FdAA");
                    schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                    schemaBuilder.SetSchemaName("EspesorMuroLongitud");
                    schemaBuilder.SetDocumentation("Schema un Field. Array<double> = Espesor y longitud del muro");

                    //Construimos Field
                    FieldBuilder fieldBuilder = schemaBuilder.AddArrayField("CampoEspesorLongitudMuro", typeof(double));
                    fieldBuilder.SetSpec(SpecTypeId.Length);//V2022
                    //fieldBuilder.SetUnitType(UnitType.UT_Length);//V2021

                    //Finalizamos constructor Field
                    schema = schemaBuilder.Finish();
                }
                double longitud = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                IList<double> vs = new List<double>() { espesor, longitud };

                //Construimos Entity. 
                entity = new Entity(schema);
                //Asignamos datos V2022 UnitTypeId.Feet | V2021 DisplayUnitType.DUT_DECIMAL_FEET
                entity.Set<IList<double>>("CampoEspesorLongitudMuro", vs, UnitTypeId.Feet /*DisplayUnitType.DUT_DECIMAL_FEET*/);
                //Asociamos a wall
                if (entity.IsValid()) wall.SetEntity(entity);
                #endregion

                #region Simple_string+Array_double
                guid = new Guid("6d8a80d3-e1c3-4b83-ada1-ce975e420529");
                //Recuperamos Schema
                schema = Schema.Lookup(guid);
                //Si es null, le creamos. Si no es null genera error
                if (schema == null)
                {
                    //Construimos Schema
                    SchemaBuilder schemaBuilder = new SchemaBuilder(guid);
                    schemaBuilder.SetVendorId("FdAA");
                    schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                    schemaBuilder.SetSchemaName("EspesorMuroLongitudyNombre");
                    schemaBuilder.SetDocumentation("Schema dos Field. Simple <string> = nombre. Array<double> = Espesor y longitud del muro");

                    //Construimos Fields
                    FieldBuilder fieldBuilder = schemaBuilder.AddArrayField("CampoEspesorLongitud", typeof(double));
                    fieldBuilder.SetSpec(SpecTypeId.Length);//.SetUnitType(UnitType.UT_Length);
                    //Segundo Field
                    schemaBuilder.AddSimpleField("CampoNombreMuro", typeof(string));

                    //Finalizamos constructor Fields
                    schema = schemaBuilder.Finish();
                }
                longitud = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                vs = new List<double>() { espesor, longitud };

                //Construimos Entity. 
                entity = new Entity(schema);
                //Asignamos datos V2022 UnitTypeId.Feet | V2021 DisplayUnitType.DUT_DECIMAL_FEET
                entity.Set<IList<double>>("CampoEspesorLongitud", vs, UnitTypeId.Feet /*DisplayUnitType.DUT_DECIMAL_FEET*/);
                entity.Set<string>("CampoNombreMuro", wall.Name);
                //Asociamos a wall
                if (entity.IsValid()) wall.SetEntity(entity);
                #endregion

                #region  Simple_ElementIdMapField_XYZ
                guid = new Guid("7d8a80d3-e1c3-4b83-ada1-ce975e420529");
                //Recuperamos Schema
                schema = Schema.Lookup(guid);
                //Si es null, le creamos. Si no es null genera error
                if (schema == null)
                {
                    //Construimos Schema
                    SchemaBuilder schemaBuilder = new SchemaBuilder(guid);
                    schemaBuilder.SetVendorId("FdAA");
                    schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                    schemaBuilder.SetSchemaName("DiccionarioXYZ");
                    schemaBuilder.SetDocumentation("Schema dos Field. Simple <ElementId> = Id.  IDictionary<int, XYZ> = Indide y XYZ");

                    //Construimos Fields
                    FieldBuilder fieldBuilder = schemaBuilder.AddMapField("CampoDiccionario", typeof(int), typeof(XYZ));
                    fieldBuilder.SetSpec(SpecTypeId.Length);//V2022
                                                            //.SetUnitType(UnitType.UT_Length);//V2021
                    //Segundo Field
                    schemaBuilder.AddSimpleField("CampoID", typeof(ElementId));

                    //Finalizamos constructor Fields
                    schema = schemaBuilder.Finish();
                }

                IDictionary<int, XYZ> keyValuePairs = new Dictionary<int, XYZ>();
                LocationCurve locationCurve = wall.Location as LocationCurve;
                Curve line = locationCurve.Curve;
                keyValuePairs.Add(0, line.GetEndPoint(0));
                keyValuePairs.Add(1, line.GetEndPoint(1));

                //Construimos Entity.
                entity = new Entity(schema);
                //Asignamos datos V2022 UnitTypeId.Feet | V2021 DisplayUnitType.DUT_DECIMAL_FEET
                entity.Set<IDictionary<int, XYZ>>("CampoDiccionario", keyValuePairs, UnitTypeId.Feet /*DisplayUnitType.DUT_DECIMAL_FEET*/);
                entity.Set<ElementId>("CampoID", wall.Id);
                //Asociamos a wall
                if (entity.IsValid()) wall.SetEntity(entity);
                #endregion

                //Confirmamos Transaction
                tx.Commit();
                TaskDialog.Show("Revit API Manual", "Schemas creados");

            }

            return Result.Succeeded;
        }
    }
}
