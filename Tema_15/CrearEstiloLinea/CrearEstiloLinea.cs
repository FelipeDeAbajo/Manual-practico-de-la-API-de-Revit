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

namespace CrearEstiloLinea
{
    [Transaction(TransactionMode.Manual)]
    public class CrearEstiloLinea : IExternalCommand
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

            //Creamos patrçon de linea
            LinePattern linePattern = null;
            //Nombre para Subcategotia y Patron de lineas
            const string nombre = "Revit API Manual";
            //Creamos subcategoria
            Category subCategoriaLine = null;

            //Colector de clase LinePatternElement por LinePattern no podemos
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(LinePatternElement));
            //Buscamos el Patron de linea  "Revit API Manual"
            LinePatternElement linePatternElement = col.Cast<LinePatternElement>().Where(x => x.GetLinePattern().Name == nombre).FirstOrDefault();

            //Si no existe.
            if (linePatternElement == null)
            {
                //Creamos lista de segmentos
                List<LinePatternSegment> lstSegments = new List<LinePatternSegment>();
                //Añadimos segmentos
                lstSegments.Add(new LinePatternSegment(LinePatternSegmentType.Dot, 0.0));
                lstSegments.Add(new LinePatternSegment(LinePatternSegmentType.Space, 0.02));
                lstSegments.Add(new LinePatternSegment(LinePatternSegmentType.Dash, 0.03));
                lstSegments.Add(new LinePatternSegment(LinePatternSegmentType.Space, 0.02));

                //Crear patron de linea. con  "Revit API Manual"
                linePattern = new LinePattern(nombre);
                //Aplicamos segmentos
                linePattern.SetSegments(lstSegments);
            }

            // El nuevo estilo de línea será una subcategoría de la categoría Líneas.
            // 
            //Obtenemos todas categorias
            Categories categories = doc.Settings.Categories;
            //Seleccionamos la categoría de Lines
            Category categoriaLine = categories.get_Item(BuiltInCategory.OST_Lines);
            //Buscamos si ya esta creada la subcategoria
            if (categoriaLine.SubCategories.Contains(nombre))
                subCategoriaLine = categoriaLine.SubCategories.get_Item(nombre);
            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Create LineStyle");
                //Creamos un LinePatternElement
                if (linePatternElement == null)
                    linePatternElement = LinePatternElement.Create(doc, linePattern);
                //Creamos suncategoria
                if (subCategoriaLine == null)
                    subCategoriaLine = categories.NewSubcategory(categoriaLine, "Revit API Manual");

                // doc.Regenerate();
                //Configuramos subcategoria
                //Espesor
                subCategoriaLine.SetLineWeight(8, GraphicsStyleType.Projection);
                //Color rojo
                subCategoriaLine.LineColor = new Color(255, 0, 0);
                //LinePatternElement
                subCategoriaLine.SetLinePatternId(linePatternElement.Id, GraphicsStyleType.Projection);

                //Conformamos Transaction
                tx.Commit();
            }
            TaskDialog.Show("Manual Revit API", "Estilo de linea creado o actualizado");

            return Result.Succeeded;
        }
    }
}
