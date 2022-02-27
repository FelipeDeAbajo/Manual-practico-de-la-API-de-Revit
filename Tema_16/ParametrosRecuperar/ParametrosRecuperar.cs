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
using System.Text;

#endregion

namespace ParametrosRecuperar
{
    [Transaction(TransactionMode.Manual)]
    public class ParametrosRecuperar : IExternalCommand
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

           // Accedemos a selecion actual
           Selection sel = uidoc.Selection;
            if(sel.GetElementIds().Count!=1)
            {
                message = "Necesitamos un objeto seleccionado";
            }

            //Obtenemos el Element
           Element  element = doc.GetElement(sel.GetElementIds().FirstOrDefault());
            // Iniciamos texto para el resumen
            String prompt = "Parámetros en el elemento seleccionado: \n\r";

            //iniciamos StringBuilder para el resumen
            StringBuilder st = new StringBuilder();
           
            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction parámetro");

                //Iteramos para cada parámetro del Element
                foreach (Parameter para in element.Parameters)
                {

                    string defName = para.Definition.Name + "\t : ";
                    string defValue = string.Empty;
                    // Diferenciamos según el tipo de almacenamiento
                    switch (para.StorageType)
                    {
                        case StorageType.Double:
                            //Obtenemos valor con unidades 
                            defValue = para.AsValueString();
                            //si el parametro es de tipo Longitud
                            if (para.Definition.GetDataType() == SpecTypeId.Length)
                            {
                                //Si no es de solo lectura y no es tampoco parámetro compartido
                                if (!para.IsReadOnly && para.Definition is InternalDefinition internalDefinition)
                                {
                                    //Si no es BuiltInParameter
                                    if (internalDefinition.BuiltInParameter == BuiltInParameter.INVALID)
                                    {
                                        //Obtenemos del parametro interno Longitud. Su valor en double, según versión de Revit

#if V2022
                                        double longitud = element.GetParameter(ParameterTypeId.CurveElemLength).AsDouble();
#else
                                        double longitud = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
#endif
                                        //Asignamos al parametro actual un valor calculado
                                        para.Set(longitud / 2);
                                    }
                                }
                            }
                            break;
                        case StorageType.ElementId:
                            //Obtenemos el Element asociado a este ElementId
                            Autodesk.Revit.DB.ElementId id = para.AsElementId();
                            if (id.IntegerValue >= 0)
                            {
                                defValue = doc.GetElement(id).Name;
                            }
                            else
                            {
                                defValue = id.IntegerValue.ToString();
                            }
                            break;
                        case StorageType.Integer:
                            if (SpecTypeId.Boolean.YesNo == para.Definition.GetDataType())
                            {
                                //Si es bool
                                defValue = Convert.ToBoolean(para.AsInteger()).ToString();
                            }
                            else
                            {
                                defValue = para.AsInteger().ToString();
                            }
                            break;
                        case StorageType.String:
                            defValue = para.AsString();
                            break;
                        default:
                            defValue = "Parámetro sin definir StorageType.";
                            break;
                    }
                    //Añadimos linea al resumen
                    st.AppendLine(defName + defValue);
                }

                //Confirmamos Transaction
                tx.Commit();
            }
            // Mostramos el resumen
            TaskDialog.Show("Revit API Manual", prompt + st.ToString());

            return Result.Succeeded;
        }
    }
}
