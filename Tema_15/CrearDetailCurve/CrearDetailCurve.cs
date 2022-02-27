#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace CrearDetailCurve
{
    [Transaction(TransactionMode.Manual)]
    public class CrearDetailCurve : IExternalCommand
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

            //Nombre para Subcategotia y Patron de lineas
            const string nombre = "Revit API Manual";

            //Creamos GraphicsStyle
            GraphicsStyle graphicsStyle = null;

            //Creamos 4 XYZ
            XYZ xYZ0 = XYZ.Zero;
            XYZ xYZ1 = new XYZ(10,0,0);
            XYZ xYZ2 = new XYZ(10, 10, 0);
            XYZ xYZ3 = new XYZ(0, 10, 0);

            //Creamos lista con 4 Lines
            List<Line> lines = new List<Line>();
            lines.Add(Line.CreateBound(xYZ0, xYZ1));
            lines.Add(Line.CreateBound(xYZ1, xYZ2));
            lines.Add(Line.CreateBound(xYZ2, xYZ3));
            lines.Add(Line.CreateBound(xYZ3, xYZ0));

            #region buscar Estilo grafico
            Categories categories = doc.Settings.Categories;
            Category categoriaLines = categories.get_Item(BuiltInCategory.OST_Lines);
            //Buscamos si  esta creada la subcategoria
            if (categoriaLines.SubCategories.Contains(nombre))
            {
                Category subCategoriaLine = categoriaLines.SubCategories.get_Item(nombre);
                //Asignamos el GraphicsStyle, en caso contrario permanece en null
                graphicsStyle = subCategoriaLine.GetGraphicsStyle(GraphicsStyleType.Projection);
            }

            #endregion
            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamo Transaction
                tx.Start("Transaction Name");
                //Para cada miembro de la lista creamos Linea de detalle
                foreach (Line line in lines)
                {
                    DetailCurve detailCurve = doc.Create.NewDetailCurve(doc.ActiveView, line);
                    //Si hay GraphicsStyle lo cambiamos, si no se crea con el de por defecto y no lo cambiamos
                    if (graphicsStyle != null) detailCurve.LineStyle = graphicsStyle;
                }
                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
