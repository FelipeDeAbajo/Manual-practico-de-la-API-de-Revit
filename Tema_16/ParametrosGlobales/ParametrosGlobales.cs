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

namespace ParametrosGlobales
{
    [Transaction(TransactionMode.Manual)]
    public class ParametrosGlobales : IExternalCommand
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


            Selection sel = uidoc.Selection;
            //Definiciones de nombres
            string name = "ParametroGlobalSinInforme";
            string nameInforme = "ParametroGlobalConInforme";

            //El Document permite parámetros globales?
            if (!GlobalParametersManager.AreGlobalParametersAllowed(doc))
            {
                message = "En este document no es posible crear Parametros globales";
                return Result.Failed;
            }
            //Existen parámetros con esos nombres?
            if (!GlobalParametersManager.IsUniqueName(doc, name) || !GlobalParametersManager.IsUniqueName(doc, nameInforme))
            {
                message = "Ya existe un Parámetro global con ese nombre, en este documento";
                return Result.Failed;
            }

            //Obtenemos todos los parámetros globales y los mostramos en un TaskDialog
            ISet<ElementId> elementIds = GlobalParametersManager.GetAllGlobalParameters(doc);
            List<GlobalParameter> globalParameters = elementIds.Select(x => doc.GetElement(x)).Cast<GlobalParameter>().ToList();
            TaskDialog.Show("Revit API Manual", "Parámetros globales existentes:\n" + String.Join("\n", globalParameters.Select(x => x.Name).ToList()));

            //Buscamos al menos dos cotas en el proyecto. Luego asignaremos los parametros a estas cotas
            List<Dimension> dimensions = sel.GetElementIds().Select(x => doc.GetElement(x)).Select(x => x as Dimension).Where(x => x != null).ToList();
            if (dimensions.Count < 2)
            {
                message = "Necesarias dos cotas";
                return Result.Cancelled;
            }

            //Creamos una Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Parámetros globales");
           
                //Creamos un GP. de Longitud sin Informe
                GlobalParameter gp = GlobalParameter.Create(doc, name, SpecTypeId.Length);
                if (gp != null)
                {
                    // Si la creación ha sido posible, le asignamos un valor de tipo double
                    gp.SetValue(new DoubleParameterValue(12)); //En unidades internas

                    // Obtenemos la primera cota
                    ElementId elemid = dimensions[0].Id;

                    // Se puede asignar este GP a esta cota?
                    if (gp.CanLabelDimension(elemid))
                    {
                        gp.LabelDimension(elemid);
                    }

                }
                //Creamos un GP. de Longitud con Informe
                gp = GlobalParameter.Create(doc, nameInforme, SpecTypeId.Length);
                if (gp != null)
                {
                    // Si la creación ha sido posible, le asignamos un valor de tipo double
                    gp.SetValue(new DoubleParameterValue(6)); //En unidades internas

                    // Obtenemos la segunda cota
                    ElementId elemid = dimensions[1].Id;

                    // Se puede asignar este GP a esta cota?
                    if (gp.CanLabelDimension(elemid))
                    {
                        gp.SetDrivingDimension(elemid);
                    }

                }

                if (gp != null)
                {
                    // Si la creación ha sido posible, le asignamos un valor de tipo double

                    //Filtramos para el primer familySymbol de Mesa con Sillas Reuniones
                    IList<FamilySymbol> familySymbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList();
                    FamilySymbol familySymbol = familySymbols.Where(x => x.FamilyName == "Mesa con Sillas Reuniones").FirstOrDefault();

                    //Seleccionamos el primer parametro con nombre "Anchura". 
                    Parameter parameter = familySymbol.GetParameters("Anchura").FirstOrDefault();
                    //Asociamos el valor al parámetro.
                    parameter.AssociateWithGlobalParameter(gp.Id);
                }
                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
