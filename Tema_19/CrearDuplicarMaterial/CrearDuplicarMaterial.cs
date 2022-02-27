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

namespace CrearDuplicarMaterial
{
    [Transaction(TransactionMode.Manual)]
    public class CrearDuplicarMaterial : IExternalCommand
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

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");

                //Creamos nuevo material
                ElementId materialId = Material.Create(doc, "Nuevo Material");
                Material material = doc.GetElement(materialId) as Material;

                //Creamos un nuevo conjunto de propiedades. 
                StructuralAsset strucAsset = new StructuralAsset("Nuevo Property Set", StructuralAssetClass.Concrete);
                //Propiedades minimas
                strucAsset.Behavior = StructuralBehavior.Isotropic;
                strucAsset.Density = 232.0;

                //Asignamos el conjunto de propiedades al material. Estructural
                PropertySetElement pse = PropertySetElement.Create(doc, strucAsset);
                material.SetMaterialAspectByPropertySet(MaterialAspect.Structural, pse.Id);

                //Nuevo nombre
                string newName = material.Name + "_Duplicado";

                //Filtramos materiales. Buscamos anteriores copias
                FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Material));
                List<Material> materials = col.ToElements().Cast<Material>().ToList();

                int nRepetidos = materials.Where(x => x.Name.StartsWith(newName)).Count();
                if (nRepetidos > 0)
                {
                    //Incrementamos con contador si existe el duplicado
                    newName = "New" + material.Name + "(" + nRepetidos + ")";
                }

                //Duplicamos material y asignamos nuevo nombre
                Material myMaterial = material.Duplicate(newName);

                if (null == myMaterial) TaskDialog.Show("Revit API Manual", "No se ha podido duplicar");
                else TaskDialog.Show("Revit API Manual", "Material duplicado");

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
