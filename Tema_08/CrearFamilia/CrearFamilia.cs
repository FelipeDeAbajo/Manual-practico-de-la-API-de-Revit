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

namespace CrearFamilia
{
    [Transaction(TransactionMode.Manual)]
    public class CrearFamilia : IExternalCommand
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
            //Seleccionamos vista actual
            View view = doc.ActiveView;
            //Solo vistas en planta
            if (view.ViewType != ViewType.FloorPlan)
            {
                message = "La vista no es de planta";
                return Result.Failed;
            }
            //Siempre que usemos Selection.PickPoint ponemos try{}
            try
            {
                //Seleccionamos dos puntos. Siempre entre try{} catch{}
                XYZ xYZ0 = uidoc.Selection.PickPoint(ObjectSnapTypes.None, "Punto inicial viga");
                XYZ xYZ1 = uidoc.Selection.PickPoint(ObjectSnapTypes.None, "Punto final viga");
                //Construimos una Curve desde Line
                Curve curve = Line.CreateBound(xYZ0, xYZ1);
                //Construimos filtro para buscar FamilySymbol de Armazón estructural
                FilteredElementCollector col = new FilteredElementCollector(doc);
                col.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming);
                if (col.Count() == 0)
                {
                    message = "No hay viga cargada";
                    return Result.Failed;
                }
                // Seleccionamos FamilySymbol
                FamilySymbol familySymbol = col.FirstElement() as FamilySymbol;

                //Obtenemos el Level asociado a la vista en planta
                Level level = doc.ActiveView.GenLevel;

                //Construimos la Transaction
                using (Transaction tx = new Transaction(doc))
                {
                    //Iniciamos la Transaction
                    tx.Start("Transaction Name");

                    //Antes de crear una FamilyInstance hay que activar el tipo
                    familySymbol.Activate();

                    //Creamos la FamilyInstance
                    FamilyInstance familyInstance = doc.Create.NewFamilyInstance(curve, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    //Confirmamos la FamilyInstance
                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}
