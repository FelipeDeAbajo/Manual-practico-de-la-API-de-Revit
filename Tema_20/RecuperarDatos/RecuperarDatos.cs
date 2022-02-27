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

namespace RecuperarDatos
{
    [Transaction(TransactionMode.Manual)]
    public class RecuperarDatos : IExternalCommand
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

            string txtSalida = "Datos recuperados: ";
            #region Conociendo Guid
            //GUID almacenado Debemos conocer todos los datos y estructura
            Guid guidSchema = new Guid("4d8a80d3-e1c3-4b83-ada1-ce975e420529");
            //Obtenemos el Schema
            Schema schema = Schema.Lookup(guidSchema);
            //Schema es nulo?
            if (schema != null)
            {
                //Obtenemos Entity
                Entity entity = wall.GetEntity(schema);
                //Recuperamos el valor. V2022
                //V2021 alternar 3º parametro
                if(entity==null || entity.Schema == null || !entity.IsValidObject)
                {
                    message = "El muro no tiene asignado el Entity";
                    return Result.Failed;
                }
                double espesorRecuperado = entity.Get<double>("CampoEspesorMuro", UnitTypeId.Meters /*DisplayUnitType.DUT_METERS*/);
                txtSalida = txtSalida + "\n\tCampoEspesorMuro: " + espesorRecuperado.ToString("N3")+ " metros";
            }
            #endregion
            
            txtSalida = txtSalida + "\n\nDatos recursivos:";

            #region Recursivo
            IList<Guid> guids = wall.GetEntitySchemaGuids();

            foreach (Guid guid in guids)
            {
                //Obtenemos el Schema
                schema = Schema.Lookup(guid);
                //Schema es nulo?
                if (schema != null)
                {
                    txtSalida = txtSalida + "\nSchema nombre: " + schema.SchemaName;
                    //Obtenemos Entity
                    Entity entity = wall.GetEntity(schema);
                    //Obtenemos lista de Fields en Schema
                    IList<Field> fields = schema.ListFields();
                    //Iteramos en cada Field del Schema
                    foreach (Field field in fields)
                    {
                        //Nombre del Field
                        string nameField = field.FieldName;
                        //Obtenemos tipo de dato almacenado
                        Type type = field.ValueType;

                        //Obtenemos unidades
                        ForgeTypeId forgeTypeId = field.GetSpecTypeId();
                        ForgeTypeId unit = UnitUtils.IsMeasurableSpec(forgeTypeId) ? UnitUtils.GetValidUnits(field.GetSpecTypeId()).First() : UnitTypeId.Custom;

                        //Nombre unidades metros, centimetros
                        string txtUnits = unit.TypeId.Split(':')[1].Split('-')[0];

                        //Es string=
                        if (type == typeof(string))
                        {
                            string valueString = entity.Get<string>(nameField, unit);
                            txtSalida = txtSalida + "\n      "+ nameField + ": " + valueString;

                        }
                        //Es double y Simple?
                        else if (type == typeof(double) && field.ContainerType == ContainerType.Simple)
                        {
                            double valueDouble = entity.Get<double>(nameField, unit);
                            txtSalida = txtSalida + "\n      " + nameField + ": " + valueDouble.ToString("N3")+ " "+ txtUnits;

                        }
                        //Es double y Array?
                        else if (type == typeof(double) && field.ContainerType == ContainerType.Array)
                        {
                            IList<double> valueList = entity.Get<IList<double>>(nameField, unit);
                            txtSalida = txtSalida + "\n      " + $"{nameField} Espesor: {valueList.ElementAt(0) + " " + txtUnits}, Longitud: {valueList.ElementAt(1) + " " + txtUnits}";

                        }
                        //Es XYZ
                        else if (type == typeof(XYZ) && field.ContainerType == ContainerType.Map)
                        {
                            IDictionary<int, XYZ> valueDic = entity.Get<IDictionary<int, XYZ>>(nameField, unit);
                            txtSalida = txtSalida + "\n      " + $"{nameField} InicioX: {valueDic[0].X} {txtUnits} FinX: {valueDic[1].X} {txtUnits}";

                        }
                        //Es ElemenId?
                        else if (type == typeof(ElementId) )
                        {
                            ElementId mId = entity.Get<ElementId>(nameField);
                            txtSalida = txtSalida + "\n      " + $"{nameField} Ejemplar: {mId.IntegerValue}.";

                        }
                    }
                }
            }
            #endregion
            TaskDialog.Show("Revit API Manual", txtSalida);

            return Result.Succeeded;

           
        }
    }
}
