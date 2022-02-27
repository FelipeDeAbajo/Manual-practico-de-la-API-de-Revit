#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace AnalisisVisualizacion
{
    [Transaction(TransactionMode.Manual)]
    public class AnalisisVisualizacion : IExternalCommand
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

            #region Definir SpatialFieldManager
            //Obtenemos SpatialFieldManager de vista actual
            SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
            //Si es nulo la creamos
            if (null == sfm)
            {
                sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView, 1);
            }
            //Limpiamos de anteriores análisis
            sfm.Clear();
            #endregion

            #region Definir AnalysisResultSchema 
            string nombreSchemaAndStyle = "Revit API Manual";//Nombre Schema
            AnalysisResultSchema resultSchema = null;
            int schemaIndex = -1;//Indice temporal

            //Obtenemos lista de indices de AnalysisResultSchema registrados
            IList<int> registrados = sfm.GetRegisteredResults();

            //Recorremos la lista buscando mismo nombre
            foreach (int indice in registrados)
            {
                //Obtenemos  AnalysisResultSchema para cada indice
                AnalysisResultSchema schemaTemp = sfm.GetResultSchema(indice);
                string nameTemp = schemaTemp.Name;
                //Si existe le igualamos
                if (nameTemp == nombreSchemaAndStyle)
                {
                    resultSchema = schemaTemp;
                    schemaIndex = indice;
                    break;
                }
            }

            //Si el AnalysisResultSchema es nulo lo creamos
            if (resultSchema == null)
            {
                resultSchema = new AnalysisResultSchema(nombreSchemaAndStyle, "Descripción: " + nombreSchemaAndStyle);
                schemaIndex = sfm.RegisterResult(resultSchema);
            }
            #endregion

            //Seleccionamos Face. Considere try {} catch{}
            Reference reference = uidoc.Selection.PickObject(ObjectType.Face, "Seleccionar una face");
            Face face = doc.GetElement(reference).GetGeometryObjectFromReference(reference) as Face;

            //Lista de valores para cada punto
            List<double> doubleList = new List<double>();

            //Lista de ValueAtPoint para todos los puntos
            IList<ValueAtPoint> valList = new List<ValueAtPoint>();

            //Lista de puntos UV
            IList<UV> uvPts = new List<UV>();
            #region Creación de punto y valores Random
            BoundingBoxUV bb = face.GetBoundingBox();
            UV min = bb.Min;
            UV max = bb.Max;

            //Incremento en U y V de 1/10
            double incrementoU = (max.U - min.U) / 10;
            double incrementoV = (max.V - min.V) / 10;

            Random random = new Random();

            //Dividimos la Face en cuadricula de 10 x 10
            for (int i = 0; i <= 10; i++)
            {
                for (int j = 0; j <= 10; j++)
                {
                    UV temp = (new UV(min.U + incrementoU * j, min.V + incrementoV * i));
                    uvPts.Add(temp);
                    doubleList.Add(random.Next(10));
                    valList.Add(new ValueAtPoint(doubleList));
                    doubleList.Clear();
                }
            }
            #endregion
            #region Crear SpatialFieldPrimitive
            //Creamos FieldDomainPoints, como es Face es UV
            FieldDomainPointsByUV pnts = new FieldDomainPointsByUV(uvPts);

            //Creamos FieldValues
            FieldValues vals = new FieldValues(valList);

            //Transfor desplazamiento Z=1
            Transform transform = Transform.CreateTranslation(XYZ.BasisZ);

            //Añadimos SpatialFieldPrimitive (Face), con la Transform
            int idx = sfm.AddSpatialFieldPrimitive(face, transform);

            //Actualizamos la SpatialFieldPrimitive con los datos anteriores
            sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, schemaIndex);

            #endregion
            //Creamos AnalysisDisplayStyle
            AnalysisDisplayStyle analysisDisplayStyle = null;
            //Buscamos analysisDisplayStyle con nombre "Revit API Manual"
            FilteredElementCollector col = new FilteredElementCollector(doc);
            ICollection<Element> collection = col.OfClass(typeof(AnalysisDisplayStyle)).ToElements();
            analysisDisplayStyle = collection.Where(x => x.Name == nombreSchemaAndStyle).ToList().FirstOrDefault() as AnalysisDisplayStyle;

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");

                //Si no existe AnalysisDisplayStyle le creamos
                if (analysisDisplayStyle == null)
                {
                    //Creamos AnalysisDisplayColoredSurfaceSettings Mostramos la rejilla
                    AnalysisDisplayColoredSurfaceSettings coloredSurfaceSettings = new AnalysisDisplayColoredSurfaceSettings();
                    coloredSurfaceSettings.ShowGridLines = true;
                    coloredSurfaceSettings.GridLineWeight = 5;

                    //Creamos AnalysisDisplayColorSettings. Degradado
                    AnalysisDisplayColorSettings colorSettings = new AnalysisDisplayColorSettings();
                    colorSettings.ColorSettingsType = AnalysisDisplayStyleColorSettingsType.GradientColor;

                    //Definimos colores
                    Color orange = new Color(255, 205, 0);
                    Color purple = new Color(200, 0, 200);
                    colorSettings.MaxColor = orange;
                    colorSettings.MinColor = purple;

                    //Creamos AnalysisDisplayLegendSettings. 10 tramos. Redondeo a 1...
                    AnalysisDisplayLegendSettings legendSettings = new AnalysisDisplayLegendSettings();
                    legendSettings.NumberOfSteps = 10;
                    legendSettings.Rounding = 1;
                    legendSettings.ShowDataDescription = false;
                    legendSettings.ShowLegend = true;

                    //Creamos el AnalysisDisplayStyle
                    analysisDisplayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(doc, nombreSchemaAndStyle, coloredSurfaceSettings, colorSettings, legendSettings);
                }

                //Assignamos a la vista el AnalysisDisplayStyle
                doc.ActiveView.AnalysisDisplayStyleId = analysisDisplayStyle.Id;

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }

    }
}
