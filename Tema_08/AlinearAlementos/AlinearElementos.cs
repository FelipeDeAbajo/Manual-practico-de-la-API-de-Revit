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

namespace AlinearElementos
{
    [Transaction(TransactionMode.Manual)]
    public class AlinearElementos : IExternalCommand
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
            if(walls.Count !=2)
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
            Line baseLine = GetCenterline(walls[0]);
            Line line = GetCenterline(walls[1]);
            //Convertimo la linea en UnBound
            line.MakeUnbound();
            //Proyectamos una linea sobre la otra 
            IntersectionResult result = line.Project(baseLine.Origin);
            //Si la proyección tiene resultado, hay intersección
            if (result != null)
            {
                //Obtenemos el punro proyectado
                XYZ point = result.XYZPoint;
                //Calcylamos el vector de traslación, desde el punto proyectado a su proyección
                XYZ vector = baseLine.Origin - point;
                //Creamos una transaction
                using (Transaction tx = new Transaction(doc))
                {
                    try
                    {
                        //Iniciamos la transaction
                        tx.Start("Transaction alinear");
                        //Desplazamos el muro. Para alinear deben coincidir 
                        ElementTransformUtils.MoveElement(doc, walls[1].Id, vector);
                        //Ceamos la alineación
                        dimension = doc.Create.NewAlignment(view, baseLine.Reference, line.Reference);
                        //Confirmamos la transaction
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                   //S falla anulamos la transaction
                        tx.RollBack();
                    }
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
