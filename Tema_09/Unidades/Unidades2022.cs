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

namespace Unidades
{
    [Transaction(TransactionMode.Manual)]
    public class Unidades2022 : IExternalCommand
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

            // Access current selection

            Selection sel = uidoc.Selection;

            ICollection<ElementId> elementIdsList = sel.GetElementIds();
            if (elementIdsList.Count != 1)
            {
                message = "Debe selecionar solo un elemento Wall.";
                return Result.Failed;
            }
            else if (doc.GetElement(elementIdsList.FirstOrDefault()) is Wall wall)
            {
                #region Convertir desde unidades internas
                //Obtenemos altura inicial del muro
                double alturaInicialInterna =
                    wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                double alturaInicialMetros = UnitUtils.ConvertFromInternalUnits(alturaInicialInterna
                    , UnitTypeId.Meters /*DisplayUnitType.DUT_METERS*/);
                string msg = "El muro tiene una altura inicial de \"" + alturaInicialMetros + "\" metros, sin redondeos";
                TaskDialog.Show("Manual Revit API", msg);
                #endregion

                #region converir a unidades internas
                using (Transaction tx = new Transaction(doc))
                {
                    //Como modificamos el documento debemos abrir Transaction
                    tx.Start("Modificar altura muro");
                    //multiplicamos altura * 2.15 
                    double nuevaAlturaMetros = alturaInicialMetros * 2.15;
                    //Convertir a unidades internas. 
                    double nuevaAlturaInterna = UnitUtils.ConvertToInternalUnits(nuevaAlturaMetros
                        , UnitTypeId.Meters /*DisplayUnitType.DUT_METERS*/);
                    //Actualizamos el parámetro del muro con el nuevo valor
                    wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(nuevaAlturaInterna);
                    tx.Commit();
                    msg = "El muro tiene ahora una altura de \"" + nuevaAlturaInterna + "\"" +
                        ", unidades internas, sin redondeos.";
                    TaskDialog.Show("Manual Revit API", msg);
                }
                #endregion

                #region Convertir string a numero Revit 
                //2 convertir string a numero
                string alturaTxtMetros = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsValueString();
                //Obtenemos la configuración actual de unidades del Document
                Units units = doc.GetUnits();
                ValueParsingOptions valueParsingOptions = new ValueParsingOptions();

                bool parsed = UnitFormatUtils.TryParse(units, SpecTypeId.Length /*UnitType.UT_Length*/, alturaTxtMetros
                  /* "10 imposible"*/, valueParsingOptions, out double valorConvertidoDesdeString, out message);

                if (parsed == false)
                {
                    message = "Introducir texto correcto en metros";
                    return Result.Failed;
                }
                msg = string.Format("El string con formato: \"{0}\", se ha convertido al valor double: \"{1}\""
                    , alturaTxtMetros, valorConvertidoDesdeString);
                TaskDialog.Show("Manual Revit API", msg);
                #endregion

                #region Convertir un numero Revit a string
                //Convertir numero a string con el formato de unidades, tomado desde las Units del document
                //Debe tener en Revit "Simbolo de unidad" y "Usar agrupacion de cifras"

                //Creamos un valor inventado grande, para poder generar agrupación de cifras
                double valorInventado = 30000;
                //Con agrupación de mumeros (punto de millar)
                string stringConAgrupacion = UnitFormatUtils.Format(units,
                    SpecTypeId.Length /*UnitType.UT_Length*/, valorInventado, /*false,*/ false);

                //Sin agrupación de mumeros (punto de millar)
                string stringSinAgrupacion = UnitFormatUtils.Format(units,
                    SpecTypeId.Length /*UnitType.UT_Length*/, valorInventado, /*false,*/ true);

                msg = string.Format("Numero inventado convertido a unidades actuales.\n\nString con agrupación: \"{0}\",\nString sin agrupación: \"{1}\""
                    , stringConAgrupacion, stringSinAgrupacion);

                TaskDialog.Show("Manual Revit API", msg);
                #endregion

                #region Convertir un numero Revit a string b)
                FormatOptions formatoptions = new FormatOptions();
                formatoptions = units.GetFormatOptions(SpecTypeId.Length /*UnitType.UT_Length*/);
                //Asignamos unidades
                //formatoptions.DisplayUnits = DisplayUnitType.DUT_CENTIMETERS;
                formatoptions.SetUnitTypeId(UnitTypeId.Centimeters);
                //Asignamos simbolo
                //formatoptions.UnitSymbol = UnitSymbolType.UST_CM;
                formatoptions.SetSymbolTypeId(SymbolTypeId.Cm); 
                 FormatValueOptions formatValueOptions = new FormatValueOptions();

                formatValueOptions.SetFormatOptions(formatoptions);

                string stringConAgrupacionMod = UnitFormatUtils.Format(units, SpecTypeId.Length
                    , valorConvertidoDesdeString, false, formatValueOptions);

                //string stringConAgrupacionMod = UnitFormatUtils.Format(units, UnitType.UT_Length
                //   , valorConvertidoDesdeString, false, false, formatValueOptions);

                msg = string.Format("Valor double: \"{0}\" unidades internas \nModificadas a Centimetros: \"{1}\""
                    , valorConvertidoDesdeString, stringConAgrupacionMod);

                TaskDialog.Show("Manual Revit API", msg);
                #endregion

            }
            else
            {
                message = "Debe selecionar un ejemplar de Wall.";
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
