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

namespace CrearCota
{
    [Transaction(TransactionMode.Manual)]
    public class CrearCota : IExternalCommand
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

            // Accedemos a la selección actual
            Selection sel = uidoc.Selection;

            // Chequeamos que solo tenemos dos muros seleccionado           
            List<Wall> walls = sel.GetElementIds().Select(x => doc.GetElement(x)).Cast<Wall>().ToList();
            if (walls.Count != 2)
            {
                message = "Se debe seleccionar dos muros";
                return Result.Failed;
            }

            //Creamos una cota
            Dimension dimension = null;
            //Obtenemos la vista actual
            View view = doc.ActiveView;
            //Obtenemos las lineas centrales de los dos muros
            //Llamamos al método GetCenterline
            Line baseLine =  GetCenterline(walls[0]);
            Line line = GetCenterline(walls[1]);
            #region Cota Centro muro
            //Construimos Line
            Line lineCota = Line.CreateBound(baseLine.GetEndPoint(0), line.GetEndPoint(0));

            //Construimos ReferenceArray
            ReferenceArray referenceArrayCenter = new ReferenceArray();

            //Agregamos Reference
            referenceArrayCenter.Append(baseLine.Reference);
            referenceArrayCenter.Append(line.Reference);
            #endregion

            #region referencias nucleo
            //Obtenemos UniqueId y Reference 0
            string uniqueIdWall0 = walls[0].UniqueId;
            string refStringWall0 = string.Format("{0}:{1}:{2}", uniqueIdWall0, -9999, 4);
            Reference core_centreWall0 = Reference.ParseFromStableRepresentation(doc, refStringWall0);

            //Obtenemos UniqueId y Reference 1
            string uniqueIdWall1 = walls[1].UniqueId;
            string refStringWall1 = string.Format("{0}:{1}:{2}", uniqueIdWall1, -9999, 3);
            Reference core_innerWall1 = Reference.ParseFromStableRepresentation(doc, refStringWall1);

            //Construimos ReferenceArray
            ReferenceArray referenceArrayCore = new ReferenceArray();

            //Agregamos Reference
            referenceArrayCore.Append(core_centreWall0);
            referenceArrayCore.Append(core_innerWall1);

            //Construimos Line
            Line lineCotaCore = Line.CreateBound(baseLine.GetEndPoint(1), line.GetEndPoint(1));
            #endregion

            #region Cota perpendicular 
            //Convertimo la linea en UnBound
            line.MakeUnbound();
            //Proyectamos punto medio de una linea sobre la otra 
            IntersectionResult result = line.Project(baseLine.Evaluate(0.5, true));
            //Si la proyección tiene resultado, hay intersección
            Line lineAlineada = null;
            if (result != null)
            {
                //Obtenemos el punro proyectado
                XYZ point = result.XYZPoint;
                //Calculamos el vector de traslación, desde el punto proyectado a su proyección
                XYZ vector = baseLine.Origin - point;
                //Construimos Line
                lineAlineada = Line.CreateBound(point, baseLine.Evaluate(0.5, true));
            }
            #endregion
         
            //Creamos una transaction
            using (Transaction tx = new Transaction(doc))
            {
                try
                {
                    //Iniciamos la transaction
                    tx.Start("Transaction cotas");
                    //Ceamos las 3 Dimension
                    dimension = doc.Create.NewDimension(view, lineCota, referenceArrayCenter);
                    dimension = doc.Create.NewDimension(view, lineAlineada, referenceArrayCenter);
                    dimension = doc.Create.NewDimension(view, lineCotaCore, referenceArrayCore);

                    //Confirmamos la transaction
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    //Si falla anulamos la transaction
                    tx.RollBack();
                }
            }

            return Result.Succeeded;
        }
        private static Line GetCenterline(Wall wall)
        {
            //Creamos nuevas Options
            Options options = new Options();
            //Necesitamos obtener las referencias
            options.ComputeReferences = true;
            //Las lineas de eje de muro no son vicibles
            options.IncludeNonVisibleObjects = true;
            //Suponemos que la vista actual en Vista de plano.
            //Aplicamos esta vista a las Options
            options.View = wall.Document.ActiveView;
            //Obtenemos GeometryElement
            GeometryElement geoElem = wall.get_Geometry(options);
            //Iteramos buscandi Lines
            foreach (GeometryObject item in geoElem)
            {
                Line lineObj = item as Line;
                //De las dos lineas buscamos la que tiene Reference
                if (lineObj != null && lineObj.Reference != null)
                {
                    return lineObj;
                }
            }
            return null;
        }
    }
}