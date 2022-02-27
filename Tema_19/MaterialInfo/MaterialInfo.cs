#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace MaterialInfo
{
    [Transaction(TransactionMode.Manual)]
    public class MaterialInfo : IExternalCommand
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


            //Filtramos por la clase Material
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Material));
            //Seleccionamos el material Arena
            Material material = col.ToElements().Where(x => x.Name == "Arena").FirstOrDefault() as Material;

            //Si no existe salimos
            if (material == null)
            {
                message = "Material no encontrado";
                return Result.Failed;
            }

            //Preparamos salida
            string datos = "Caracteristicas del material: " + material.Name;

            //Obtenemos los tres (de los cinco) Asset accesibles
            ElementId strucAssetId = material.StructuralAssetId;
            ElementId apperanceAssetId = material.AppearanceAssetId;
            ElementId thermalAssetId = material.ThermalAssetId;

            //Obtenemos el color,
            int colorInt = material.get_Parameter(BuiltInParameter.MATERIAL_PARAM_COLOR).AsInteger();
            datos = datos + "\nColor int: " + colorInt;

            //Analizamos ThermalAsset
            if (thermalAssetId != ElementId.InvalidElementId)
            {
                //Obtenemos PropertySetElement
                PropertySetElement pse = doc.GetElement(thermalAssetId) as PropertySetElement;
                if (pse != null)
                {
                    //Obtenemos el ThermalAsset
                    ThermalAsset asset = pse.GetThermalAsset();

                    // Verificamos el material. Solo avanzamos si es sólido
                    if (asset.ThermalMaterialType == ThermalMaterialType.Solid)
                    {
                        //Obtenemos las propiedades que se admiten en tipo sólido 
                        bool isTransmitsLight = asset.TransmitsLight;
                        double permeability = asset.Permeability;
                        double porosity = asset.Porosity;
                        double reflectivity = asset.Reflectivity;
                        double resistivity = asset.ElectricalResistivity;
                        StructuralBehavior behavior = asset.Behavior;

                        // Obtenemos otras propiedades.
                        double heatOfVaporization = asset.SpecificHeatOfVaporization;
                        double emissivity = asset.Emissivity;
                        double conductivity = asset.ThermalConductivity;
                        double density = asset.Density;

                        //Mostramos p.e. la Conductividad
                        datos = datos + "\nConductividad: " + conductivity.ToString("N3");
                    }

                }
            }

            //Analizamos StructuralAsset
            if (strucAssetId != ElementId.InvalidElementId)
            {
                //Obtenemos PropertySetElement
                PropertySetElement pse = doc.GetElement(strucAssetId) as PropertySetElement;
                if (pse != null)
                {
                    //Obtenemos StructuralAsset
                    StructuralAsset asset = pse.GetStructuralAsset();

                    // Verificamos el material. Solo avanzamos si es Isotropic
                    if (asset.Behavior == StructuralBehavior.Isotropic)
                    {
                        // Obtenemos la clase de material
                        StructuralAssetClass assetClass = asset.StructuralAssetClass;
                        datos = datos + "\nClase de material estructural: " + assetClass;

                        // Obtenemos otras propiedades.
                        double poisson = asset.PoissonRatio.X;
                        double youngMod = asset.YoungModulus.X;
                        double thermCoeff = asset.ThermalExpansionCoefficient.X;
                        double unitweight = asset.Density;
                        double shearMod = asset.ShearModulus.X;

                        if (assetClass == StructuralAssetClass.Metal)
                        {
                            //Propiedades especificas de metal
                            double dMinStress = asset.MinimumYieldStress;
                        }
                        else if (assetClass == StructuralAssetClass.Concrete)
                        {
                            //Propiedades especificas de hormigón
                            double dConcComp = asset.ConcreteCompression;
                        }

                        //Mostramos p.e. la Densidad
                        datos = datos + "\nDensidad: " + unitweight.ToString("N3");
                    }
                }
            }


            // Definimos nueva Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Color");
                //Obtenemos AppearanceAssetElement
                AppearanceAssetElement assetElem = doc.GetElement(apperanceAssetId) as AppearanceAssetElement;
                //Definimos AppearanceAssetEditScope- Similar a Transaction
                using (AppearanceAssetEditScope editScope = new AppearanceAssetEditScope(doc))
                {
                    //Iniciamos AppearanceAssetEditScope
                    Asset editableAsset = editScope.Start(assetElem.Id);

                    //Buscamos los datos de "generic_diffuse"
                    AssetPropertyDoubleArray4d genericDiffuseProperty = editableAsset.FindByName("generic_diffuse") as AssetPropertyDoubleArray4d;
                    //Obtenemos el color en RGB

                    Color color = genericDiffuseProperty.GetValueAsColor();
                    datos = datos + "\nColor RGB actual: " + color.Red + " | " + color.Green + " | " + color.Blue;
                    //Definimos nuevo color
                    Color newColor = new Color(255, 0, 0);
                    genericDiffuseProperty.SetValueAsColor(newColor);
                    // Confirmamos AppearanceAssetEditScope
                    editScope.Commit(true);
                }
                //Confirmamos Transaction
                tx.Commit();
            }

            TaskDialog.Show("Revit API Manual", datos);

            return Result.Succeeded;
        }
    }
}
