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

namespace CrearVariasFamilias
{
    [Transaction(TransactionMode.Manual)]
    public class CrearVariasFamilias : IExternalCommand
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
                return Result.Cancelled;
            }
            //Obtenemos el Level asociado a la la vista
            Level level = doc.ActiveView.GenLevel;

            FamilySymbol familySymbol = null;
            //Filtramos el Document para obtener el primer FamilySymbol de Pilar estructural
            FilteredElementCollector col = new FilteredElementCollector(doc);
            IList<Element> familySymbols = col.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();
            familySymbol = familySymbols.FirstOrDefault() as FamilySymbol;

            if (familySymbol == null)
            {
                message = "No hay pilares estructurales";
                return Result.Failed;
            }

            //Creamos una Lista de FamilyInstanceCreationData. Luego creamos todos las <FamilyInstances simultaneamente
            List<Autodesk.Revit.Creation.FamilyInstanceCreationData> familyInstanceCreationDatas = new List<Autodesk.Revit.Creation.FamilyInstanceCreationData>();

            //Bucle de 10 pilares
            for (int n = 0; n < 10; n++)
            {
                //Obtenemos el XYZ de inserción
                XYZ xYZ = new XYZ(n * 4, 10, level.Elevation);
                //Creamos FamilyInstanceCreationData y añadimos a lista
                Autodesk.Revit.Creation.FamilyInstanceCreationData familyInstanceCreationData =
                    new Autodesk.Revit.Creation.FamilyInstanceCreationData(xYZ, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                familyInstanceCreationDatas.Add(familyInstanceCreationData);
            }

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Inserción 10 pilares");
                //Creamos las FamilyInstances
                ICollection<ElementId> ids = doc.Create.NewFamilyInstances2(familyInstanceCreationDatas);
                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
