#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace ConstruirFamiliaPilar
{
    [Transaction(TransactionMode.Manual)]
    public class ConstruirFamiliaPilar : IExternalCommand
    {
        List<Level> levels = null;
        Dictionary<EnumeracionPlanosRef, ReferencePlane> dicPlanosRef = null;
        List<View> views = null;
        List<Dimension> dimensions = null;


        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Paso 1. Creación desde plantilla. Obtenemos UIDocument.Abrimos la Familia
            UIDocument familyUIDoc = CrearFamilia(uiapp, out string errorCreacion);        
            if (familyUIDoc == null)
            {
                message = errorCreacion;
                return Result.Failed;
            }
            //Obtenemos Document de Familia
            Document familyDoc = familyUIDoc.Document;

            //Paso 2. El documento es correcto?
            //if (!IsDocumentoCorrecto(familyDoc, out string error))
            //{
            //    message = error;
            //    return Result.Failed;
            //}

            //Paso 3. Obtenemos los planos de referencia
            //dicPlanosRef = GetDicPlanosRef(familyDoc);
            //if (dicPlanosRef == null)
            //{
            //    message = "ErrorPlanos de referencia modificados";
            //    return Result.Failed;
            //}

            //Paso 4. Obtenemos niveles
           // levels = GetListaNiveles(familyDoc);

            //Paso 5. Obtenemos lista View
          //  views = GetListaVistas(familyDoc);

            //Paso 6. Obtenemos restricciones
          //  dimensions = GetListaCotas(familyDoc);

            //Declaramos la Transaction
            using (Transaction tx = new Transaction(familyDoc))
            {
                //Iniciamos la Transaction
                tx.Start("Transaction Name");

                //Paso 7 Creamos extrusión
              //  Extrusion extrusion = CrearExtrusion(familyDoc);

                //Paso 8 Creamos restricciones
             //  CrearRestricciones(familyDoc, extrusion);

                //Paso  9 Añadir tipos
                //AddType(familyDoc, "500x500", 500, 500);
                //AddType(familyDoc, "300x600", 300, 600);
                //AddType(familyDoc, "400x800", 400, 800);
                //AddType(familyDoc, "500x500", 500, 500);

                //Paso 10 Asignar material 
                Element newMaterial = AddMaterial(familyDoc, "Revit API Manual");
             //   AsignarMaterial(familyDoc, extrusion, newMaterial.Id);

                //Paso 11 Añadir formulas
              //  AñadirFormula(familyDoc);

                //Paso 12 Establecer visibilidad
              //  SetVisibilidad(familyDoc, extrusion);

                //Confirmamos Transaction
                tx.Commit();
            }

            //Guardamos cambios
            familyDoc.Save();

            return Result.Succeeded;
        }
        //Paso 1
        private UIDocument CrearFamilia(UIApplication uiapp, out string error)
        {
            error = string.Empty;

            //Creamos un nombre para la plantilla
            string nombreFichero = string.Empty;

            //Creamos un formulario de lectura de ficheros
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            //Obtenemos la carpeta de plantillas
            //Lo pasamos como carpeta inicial al formulario
            openFileDialog.InitialDirectory = uiapp.Application.FamilyTemplatePath;

            //Ponemos título
            openFileDialog.Title = "Abrir plantilla de Familia";

            //Filtramos tipo de archivos para leer
            openFileDialog.Filter = "Plantillas de Familias de Revit (*.rft)|*.rft";

            //Mostramos el formulario
            System.Windows.Forms.DialogResult dialogResult = openFileDialog.ShowDialog();
            //Si cancelamos o cerramos sin seleccionar archivo válido
            if (dialogResult != System.Windows.Forms.DialogResult.OK)
            {
                error = "Seleccion cancelada";
                return null;

            }
            //asignamos el nombre del fichero
            nombreFichero = openFileDialog.FileName;
            if (!System.IO.File.Exists(nombreFichero))
            {
                error = "El archivo seleccionado no existe";
                return null;
            }
            //Creamos una nueva Familia
            Document familiDoc = uiapp.Application.NewFamilyDocument(nombreFichero);

            //Creamos un formulario de lectura de ficheros
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();

            //Filtramos tipo de archivos para leer
            saveFileDialog.Filter = "Familias de Revit (*.rfa)|*.rfa";

            //Ponemos título
            saveFileDialog.Title = "Salvado de archivo de Familia";

            //Mostramos el formulario
            dialogResult = saveFileDialog.ShowDialog();

            //Asignamos el nombre del fichero
            string nombreFamilia = saveFileDialog.FileName;

            if (!System.IO.File.Exists(nombreFichero))
            {
                error = "El archivo seleccionado no existe";
                return null;
            }

            //Opciones de salvado. Sobreescribir
            SaveAsOptions saveAsOptions = new SaveAsOptions();
            saveAsOptions.OverwriteExistingFile = true;

            //Salvamos como
            familiDoc.SaveAs(nombreFamilia, saveAsOptions);
            //Cerramos no salvamos cambios
            familiDoc.Close(false);
          
            //Abrimos y activamos
            UIDocument uIDocument = uiapp.OpenAndActivateDocument(nombreFamilia);

            return uIDocument;
        }

        //Paso 2
        private bool IsDocumentoCorrecto(Document docFamily, out string error)
        {
            bool isCorrecto = false;
            error = string.Empty;

            // Si es FamilyDocument
            if (!docFamily.IsFamilyDocument)
            {
                error = "No es documento FamilyDocumen";
                return isCorrecto;
            }

            //Si tenemos acceso al OwnerFamily
            if (docFamily.OwnerFamily == null)
            {
                error = "No es existe OwnerFamily";
                return isCorrecto;
            }

            //Si es de categoría Pilar
            if (docFamily.OwnerFamily.FamilyCategory.Id.IntegerValue != (int)BuiltInCategory.OST_Columns)
            {
                error = "No es Categoría Pilar";
                return isCorrecto;
            }

            isCorrecto = true;
            return isCorrecto;
        }

        //Paso 3 Obtener Planos de Referencia
        private Dictionary<EnumeracionPlanosRef, ReferencePlane> GetDicPlanosRef(Document docFamily)
        {
            //Creamos dictionary, desde la enumeración EnumeracionPlanosRef
            Dictionary<EnumeracionPlanosRef, ReferencePlane> keyValuePairs = new Dictionary<EnumeracionPlanosRef, ReferencePlane>();

            //Creamos colector. Aplicamos filtro de Clase ReferencePlane
            FilteredElementCollector col = new FilteredElementCollector(docFamily);
            List<ReferencePlane> referencePlanes = col.OfClass(typeof(ReferencePlane)).Select(x => x as ReferencePlane).ToList();

            //Si no son 7, es que se ha modificado la plantilla
            if (referencePlanes.Count() != 7)
            {
                return null;
            }

            //Ordenamos la lista y dividimos en planos paralelos a X y a Y
            List<ReferencePlane> nListPlaRefHor = referencePlanes.Where(p => Math.Abs(p.Normal.Y) == 1).OrderBy(p => p.BubbleEnd.Y).ToList();
            List<ReferencePlane> nListPlaRefVer = referencePlanes.Where(p => Math.Abs(p.Normal.X) == 1).OrderBy(p => p.BubbleEnd.X).ToList();

            //Incluimos en el Dictionary, ordenamos con la EnumeracionPlanosRef
            //Paralelos a X
            keyValuePairs.Add(EnumeracionPlanosRef.HorizontalMenorY, nListPlaRefHor.ElementAt(0));
            keyValuePairs.Add(EnumeracionPlanosRef.HorizontalCentral, nListPlaRefHor.ElementAt(1));
            keyValuePairs.Add(EnumeracionPlanosRef.HorizontalMayorY, nListPlaRefHor.ElementAt(2));

            //Incluimos en el Dictionary, ordenamos con la EnumeracionPlanosRef
            //Paralelos a Y
            keyValuePairs.Add(EnumeracionPlanosRef.VerticalMenorX, nListPlaRefVer.ElementAt(0));
            keyValuePairs.Add(EnumeracionPlanosRef.VerticalCentral, nListPlaRefVer.ElementAt(1));
            keyValuePairs.Add(EnumeracionPlanosRef.VerticalMayorX, nListPlaRefVer.ElementAt(2));

            //Incluimos en el Dictionary, ordenamos con la EnumeracionPlanosRef
            //Paralelos a Z
            keyValuePairs.Add(EnumeracionPlanosRef.Base, referencePlanes.Where(p => Math.Abs(p.Normal.Z) == 1).First());

            return keyValuePairs;
        }

        //Paso 4 Obtenemos niveles
        private List<Level> GetListaNiveles(Document docFamily)
        {
            //Creamos un colector
            FilteredElementCollector col = new FilteredElementCollector(docFamily);
            //Aplicamos filtro de clase Level. Ordenamos la lista por su elevación
            List<Level> levels = col.OfClass(typeof(Level)).Select(x => x as Level).OrderBy(x => x.Elevation).ToList();
            return levels;
        }

        //Paso 5 Obtenemos vistas
        private List<View> GetListaVistas(Document docFamily)
        {
            //Creamos un colector
            FilteredElementCollector col = new FilteredElementCollector(docFamily);
            //Aplicamos filtro de clase View.
            List<View> views = col.OfClass(typeof(View)).Select(x => x as View).ToList();
            return views;
        }

        //Paso 6 Obtenemos restricciones
        private List<Dimension> GetListaCotas(Document docFamily)
        {
            //Creamos un colector
            FilteredElementCollector col = new FilteredElementCollector(docFamily);
            //Aplicamos filtro de clase Dimension y de categoría.
            List<Dimension> cotas = col.OfClass(typeof(Dimension)).OfCategory(BuiltInCategory.OST_Dimensions).Select(x => x as Dimension).
            Where(x => x.FamilyLabel!=null).ToList();
            return cotas;
        }

        //Paso 7 Creamos extrusión
        private Extrusion CrearExtrusion(Document docFamily)
        {
            // (1) definir un perfil rectangular
            //
            //  3     2
            //   +---+
            //   |   | d    h = altura
            //   +---+
            //  0     1
            //  4  w

            #region CurveArray
            //Obtenemos los 4 puntos. Coordenadas de Planos de Referencia
            XYZ xYZ0 = new XYZ(dicPlanosRef[EnumeracionPlanosRef.VerticalMenorX].BubbleEnd.X, dicPlanosRef[EnumeracionPlanosRef.HorizontalMenorY].BubbleEnd.Y, 0);
            XYZ xYZ1 = new XYZ(dicPlanosRef[EnumeracionPlanosRef.VerticalMayorX].BubbleEnd.X, dicPlanosRef[EnumeracionPlanosRef.HorizontalMenorY].BubbleEnd.Y, 0);
            XYZ xYZ2 = new XYZ(dicPlanosRef[EnumeracionPlanosRef.VerticalMayorX].BubbleEnd.X, dicPlanosRef[EnumeracionPlanosRef.HorizontalMayorY].BubbleEnd.Y, 0);
            XYZ xYZ3 = new XYZ(dicPlanosRef[EnumeracionPlanosRef.VerticalMenorX].BubbleEnd.X, dicPlanosRef[EnumeracionPlanosRef.HorizontalMayorY].BubbleEnd.Y, 0);

            //Construimos las Line
            Line line0 = Line.CreateBound(xYZ0, xYZ1);
            Line line1 = Line.CreateBound(xYZ1, xYZ2);
            Line line2 = Line.CreateBound(xYZ2, xYZ3);
            Line line3 = Line.CreateBound(xYZ3, xYZ0);

            //Construimos CurveArray y agregamos las Line
            CurveArray curveArray = docFamily.Application.Create.NewCurveArray();
            curveArray.Append(line0);
            curveArray.Append(line1);
            curveArray.Append(line2);
            curveArray.Append(line3);

            //Construimos CurveArrArray y agregamos el CurveArray
            CurveArrArray curveArrayArray = docFamily.Application.Create.NewCurveArrArray();
            curveArrayArray.Append(curveArray);
            #endregion

            #region SketchPlane
            //Colectamos SketchPlane horizontal. con Z=0
            FilteredElementCollector col = new FilteredElementCollector(docFamily);
            SketchPlane sketchPlane = col.OfClass(typeof(SketchPlane)).Select(x => x as SketchPlane).
                  Where(x => x.GetPlane().Normal.IsAlmostEqualTo(XYZ.BasisZ) && x.GetPlane().Origin.Z == 0).First();
           
            #endregion


            return docFamily.FamilyCreate.NewExtrusion(true, curveArrayArray, sketchPlane, levels.Last().Elevation);
        }

        //Paso 8 Creamos restricciones
        private void CrearRestricciones(Document docFamily, Extrusion extrusion)
        {
            #region En planta
            //Obtenemos la FloorPlan con menor Z
            View viewPlan = views.Where(x => x.ViewType == ViewType.FloorPlan).OrderBy(x => x.GenLevel.Elevation).First();

            //1ª restricción. 
            ReferencePlane referencePlane = dicPlanosRef[EnumeracionPlanosRef.VerticalMenorX];
            PlanarFace planarFace = BuscarCara(extrusion, -XYZ.BasisX);
            docFamily.FamilyCreate.NewAlignment(viewPlan, referencePlane.GetReference(), planarFace.Reference);

            //2ª restricción. 
            referencePlane = dicPlanosRef[EnumeracionPlanosRef.VerticalMayorX];
            planarFace = BuscarCara(extrusion, XYZ.BasisX);
            docFamily.FamilyCreate.NewAlignment(viewPlan, referencePlane.GetReference(), planarFace.Reference);

            //3ª restricción. 
            referencePlane = dicPlanosRef[EnumeracionPlanosRef.HorizontalMenorY];
            planarFace = BuscarCara(extrusion, -XYZ.BasisY);
            docFamily.FamilyCreate.NewAlignment(viewPlan, referencePlane.GetReference(), planarFace.Reference);

            //4ª restricción. 
            referencePlane = dicPlanosRef[EnumeracionPlanosRef.HorizontalMayorY];
            planarFace = BuscarCara(extrusion, XYZ.BasisY);
            docFamily.FamilyCreate.NewAlignment(viewPlan, referencePlane.GetReference(), planarFace.Reference);
            #endregion

            #region En alzado
            //Obtenemos un alzado. Cualquiera.
            View vertical = views.Where(x => x.ViewType == ViewType.Elevation).First();

            //5ª restricción. 
            referencePlane = dicPlanosRef[EnumeracionPlanosRef.Base];
            planarFace = BuscarCara(extrusion, -XYZ.BasisZ);
            docFamily.FamilyCreate.NewAlignment(vertical, referencePlane.GetReference(), planarFace.Reference);

            //Obtenemos el nivel superior
            Level level = levels.Last();

            //6ª restricción. 
            planarFace = BuscarCara(extrusion, XYZ.BasisZ);
            docFamily.FamilyCreate.NewAlignment(vertical, level.GetPlaneReference(), planarFace.Reference);
            #endregion
        }

        //Paso 9 Añadir tipos
        private void AddType(Document docFamily, string nombre, double w, double d)
        {
            //Obtenemos FamilyManager
            FamilyManager familyManager = docFamily.FamilyManager;

            //Obtenemos los FamilyTypes existentes
            FamilyTypeSet familyTypeSet = familyManager.Types;

            //Si ya existe un FamilyType con ese nombre
            if (familyTypeSet.Cast<FamilyType>().Where(x => x.Name == nombre).Count() > 0)
            {
                TaskDialog.Show("API Revit Manual", "El FamilyType " + nombre + " ya existe");
                return;
            }

            //Creamos el FamilyType
            FamilyType familyType = familyManager.NewType(nombre);

            //1º parámetro
            FamilyParameter familyParameter = dimensions.Where(x => (x.Curve as Line).Direction.IsAlmostEqualTo(XYZ.BasisX)).First().FamilyLabel;
            w = UnitUtils.ConvertToInternalUnits(w, UnitTypeId.Millimeters /*DisplayUnitType.DUT_MILLIMETERS*/);//Comentado para versiones anteriores a 2022
            familyManager.Set(familyParameter, w);

            //2º parámetro
            familyParameter = dimensions.Where(x => (x.Curve as Line).Direction.IsAlmostEqualTo(XYZ.BasisY)).First().FamilyLabel;
            d = UnitUtils.ConvertToInternalUnits(d, UnitTypeId.Millimeters /*DisplayUnitType.DUT_MILLIMETERS*/);//Comentado para versiones anteriores a 2022
            familyManager.Set(familyParameter, d);

        }

        //Paso 10 Asignar material 
        private void AsignarMaterial(Document docFamily, Extrusion extrusion, ElementId elementId)
        {
            const string nombreParaMate = "Mi parametro material";

            //Obtenemos FamilyManager
            FamilyManager familyManager = docFamily.FamilyManager;

            //Obtenemos FamilyParameterSet => colección de FamilyParameter
            FamilyParameterSet familyParameterSet = familyManager.Parameters;

            //Convertimos a FamilyParameter y buscamos nombre
            FamilyParameter familyParameter = familyParameterSet.Cast<FamilyParameter>().Where(x => x.Definition.Name == nombreParaMate).FirstOrDefault();

            //Si no existe lo creamos el FamilyParameter
            if (familyParameter == null)
            {
                //comentado version anterior a 2022
                familyParameter = familyManager.AddParameter(nombreParaMate, GroupTypeId.Materials, /*BuiltInParameterGroup.PG_MATERIALS*/SpecTypeId.Reference.Material, /*ParameterType.Material*/ true);
            }

            //Obtenemos Parameter Material de extrusión
            Parameter parameter = extrusion.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);

            //Asociamos Parameter y FamilyParameter
            familyManager.AssociateElementParameterToFamilyParameter(parameter, familyParameter);

            //Asignamos valor
            familyManager.Set(familyParameter, elementId);
        }

        //Paso 11 Añadir formulas
        private void AñadirFormula(Document docFamily)
        {
            //Buscamos los dos parametros por direción
            FamilyParameter familyParameter_w = dimensions.Where(x => (x.Curve as Line).Direction.IsAlmostEqualTo(XYZ.BasisX)).First().FamilyLabel;
            FamilyParameter familyParameter_d = dimensions.Where(x => (x.Curve as Line).Direction.IsAlmostEqualTo(XYZ.BasisY)).First().FamilyLabel;

            //Obtenemos FamilyManager
            FamilyManager familyManager = docFamily.FamilyManager;

            //Aplicamos formula a FamilyParameter
            familyManager.SetFormula(familyParameter_d, familyParameter_w.Definition.Name + "*3");
        }

        //Paso 12 Establecer visibilidad
        private void SetVisibilidad(Document docFamily, Extrusion extrusion)
        {
            //Creamos 4 XYZ en los verices
            XYZ xYZ0 = new XYZ(dicPlanosRef[EnumeracionPlanosRef.VerticalMenorX].BubbleEnd.X, dicPlanosRef[EnumeracionPlanosRef.HorizontalMenorY].BubbleEnd.Y, 0);
            XYZ xYZ1 = new XYZ(dicPlanosRef[EnumeracionPlanosRef.VerticalMayorX].BubbleEnd.X, dicPlanosRef[EnumeracionPlanosRef.HorizontalMenorY].BubbleEnd.Y, 0);
            XYZ xYZ2 = new XYZ(dicPlanosRef[EnumeracionPlanosRef.VerticalMayorX].BubbleEnd.X, dicPlanosRef[EnumeracionPlanosRef.HorizontalMayorY].BubbleEnd.Y, 0);
            XYZ xYZ3 = new XYZ(dicPlanosRef[EnumeracionPlanosRef.VerticalMenorX].BubbleEnd.X, dicPlanosRef[EnumeracionPlanosRef.HorizontalMayorY].BubbleEnd.Y, 0);

            #region SketchPlane
            //Buscamos SketchPlane, Z=0
            FilteredElementCollector col = new FilteredElementCollector(docFamily);
            SketchPlane sketchPlane = col.OfClass(typeof(SketchPlane)).Select(x => x as SketchPlane).
                  Where(x => x.GetPlane().Normal.IsAlmostEqualTo(XYZ.BasisZ) && x.GetPlane().Origin.Z == 0).First();
            #endregion

            //Creamos dos Line. Diagonales
            Line lCruz1 = Line.CreateBound(xYZ0, xYZ2);
            Line lCruz2 = Line.CreateBound(xYZ1, xYZ3);

            //Creamos dos SymbolicCurve con las Line
            SymbolicCurve cruz1 = docFamily.FamilyCreate.NewSymbolicCurve(lCruz1, sketchPlane);
            SymbolicCurve cruz2 = docFamily.FamilyCreate.NewSymbolicCurve(lCruz2, sketchPlane);

            //FamilyElementVisibility para objetos dependientes de vista
            FamilyElementVisibility familyElementVisibilityVista = new FamilyElementVisibility(FamilyElementVisibilityType.ViewSpecific);

            //Establecemos visibilidad
            familyElementVisibilityVista.IsShownInFine = false;
            familyElementVisibilityVista.IsShownInMedium = false;

            //Asignamos a SymbolicCurve
            cruz1.SetVisibility(familyElementVisibilityVista);
            cruz2.SetVisibility(familyElementVisibilityVista);

            //FamilyElementVisibility de modelo
            FamilyElementVisibility familyElementVisibilityModel = new FamilyElementVisibility(FamilyElementVisibilityType.Model);

            //Establecemos visibilidad
            familyElementVisibilityModel.IsShownInFine = true;
            familyElementVisibilityModel.IsShownInCoarse = false;
            familyElementVisibilityModel.IsShownInMedium = false;

            //Asignamos a la extrusión
            extrusion.SetVisibility(familyElementVisibilityModel);
        }

        //Método auxiliar busqueda de PlanarFace por normal
        private PlanarFace BuscarCara(Extrusion extrusion, XYZ normal)
        {

            Options options = new Options();
            options.ComputeReferences = true;
            GeometryElement geometryElement = extrusion.get_Geometry(options);
            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is Solid solid)
                {
                    FaceArray faceArray = solid.Faces;
                    foreach (Face face in faceArray)
                    {
                        PlanarFace planarFace = face as PlanarFace;
                        if ((planarFace != null) && planarFace.FaceNormal.IsAlmostEqualTo(normal))
                        {
                            return planarFace;
                        }

                    }
                }
            }
            return null;
        }

        //Método auxiliar Creación de material
        private Element AddMaterial(Document docFamily, string nombre)
        {
            //Nuevs propiedades
            const string mNameStruc = "Mis propiedades estructurales";
            const string mNameTerm = "Mis propiedades térmicas";

            //Filtramos pot clase Material
            FilteredElementCollector col = new FilteredElementCollector(docFamily);
            col.OfClass(typeof(Material));
            Element element = col.Where(x => x.Name == nombre).FirstOrDefault();
            //Si existe Material, lo tomamos. Si no lo creamos
            ElementId elementId = (element != null) ? element.Id : Material.Create(docFamily, nombre);

            //obtenemos el material desde ElementId
            Material material = docFamily.GetElement(elementId) as Material;

           //Propiedades estructurales
            StructuralAsset structuralAsset = new StructuralAsset(mNameStruc, StructuralAssetClass.Concrete);
            structuralAsset.Density = 450;
            structuralAsset.Behavior = StructuralBehavior.Isotropic;

            //Propiedades térmicas
            ThermalAsset thermalAsset = new ThermalAsset(mNameTerm, ThermalMaterialType.Solid);
            thermalAsset.ThermalConductivity = 0.27;

            //Filtramos por PropertySetElement
            PropertySetElement propertySetElement = null;
            col = new FilteredElementCollector(docFamily);
            propertySetElement = col.OfClass(typeof(PropertySetElement)).Where(x => x.Name == mNameStruc).FirstOrDefault() as PropertySetElement;

            //Si existe Propiedades estructurales, lo tomamos. Si no lo creamos
            if (propertySetElement == null)
            {
                propertySetElement = PropertySetElement.Create(docFamily, structuralAsset);
            }

            //Asignamos Propiedades estructurales a material
            material.SetMaterialAspectByPropertySet(MaterialAspect.Structural, propertySetElement.Id);

            //Filtramos por PropertySetElement
            col = new FilteredElementCollector(docFamily);
            propertySetElement = col.OfClass(typeof(PropertySetElement)).Where(x => x.Name == mNameTerm).FirstOrDefault() as PropertySetElement;

            //Si existe Propiedades térmicas, lo tomamos. Si no lo creamos
            if (propertySetElement == null)
            {
                propertySetElement = PropertySetElement.Create(docFamily, thermalAsset);
            }

            //Asignamos Propiedades térmicas a material
            material.SetMaterialAspectByPropertySet(MaterialAspect.Thermal, propertySetElement.Id);

            return material;
        }

        //Enumeración para los planos de referencia
        enum EnumeracionPlanosRef
        {
            Base,
            VerticalCentral,
            VerticalMayorX,
            VerticalMenorX,
            HorizontalCentral,
            HorizontalMenorY,
            HorizontalMayorY,
        }
    }
}
