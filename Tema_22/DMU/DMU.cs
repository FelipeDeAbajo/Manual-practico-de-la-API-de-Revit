#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace DMU
{
    [Transaction(TransactionMode.Manual)]
    public class DMU : IExternalCommand
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

            //Creamos una nueva instancia de la clase DMUUpdater
            DMUUpdater dMUUpdater = new DMUUpdater(app.ActiveAddInId);

            //Registramos el Updater. No es opcional
            UpdaterRegistry.RegisterUpdater(dMUUpdater, doc, false);

            //Creamos un filtro de clase
            ElementClassFilter elementClassFilterRun = new ElementClassFilter(typeof(StairsRun));

            //Agragamos disparadores. para StairsRun. Agregación de Elements y 
            UpdaterRegistry.AddTrigger(dMUUpdater.GetUpdaterId(), elementClassFilterRun, Element.GetChangeTypeElementAddition());
            //Para cualquier modificacion de tramo
           // UpdaterRegistry.AddTrigger(dMUUpdater.GetUpdaterId(), elementClassFilterRun, Element.GetChangeTypeAny());
           //Solo para cambios geométricos
            UpdaterRegistry.AddTrigger(dMUUpdater.GetUpdaterId(), elementClassFilterRun, Element.GetChangeTypeGeometry());
           

            return Result.Succeeded;
        }
        //Método auxiliar. Area desde lista de XYZ
        static internal double Area(List<XYZ> vertices)
        {
            double suma = 0;
            int i = 0;
            //Sólo calculamos si el polígono tiene al menos tres vértices
            if (vertices.Count >= 3)
            {
                //Producto en cruz desde 1 hasta n-1
                for (i = 0; i <= vertices.Count - 2; i++)
                {
                    suma += vertices[i].X * vertices[i + 1].Y - vertices[i].Y * vertices[i + 1].X;
                }
                //Ahora el último con el primero
                i = vertices.Count - 1;
                suma += vertices[i].X * vertices[0].Y - vertices[i].Y * vertices[0].X;
            }
            return Math.Abs(suma) / 2;
        }
    }
    public class DMUUpdater : IUpdater
    {
        static AddInId m_appId;
        static UpdaterId m_updaterId;

        //El constructor toma el AddInId del Application.ActiveAddInId
        public DMUUpdater(AddInId id)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, Guid.NewGuid());
        }

        public void Execute(UpdaterData data)
        {
            //Obtenemos el Document
            Document doc = data.GetDocument();

            //Para cada Element añadido. 
            foreach (ElementId addedElemId in data.GetAddedElementIds())
            {
                //Parseamos a Tramo de escalera
                if (doc.GetElement(addedElemId) is StairsRun stairsRun)
                {
                    //Si fuese segmentos rectos
                    // List<XYZ> xYZs = stairsRun.GetFootprintBoundary().ToList().Select(x => x.GetEndPoint(0)).ToList();

                    List<XYZ> xYZs = new List<XYZ>();
                    //Obtenemos contorno del Tramo
                    CurveLoop curves = stairsRun.GetFootprintBoundary();
                    //Para cada curve. Pueden no ser Line
                    foreach (Curve curve in curves)
                    {
                        //Obtenemos punto extremos el Lines y puntos intermedios si no es Line
                        IList<XYZ> temp = curve.Tessellate();
                        //Añadimos a la lista
                        xYZs = xYZs.Concat(temp.ToList()).ToList();
                    }
                    //Obtenemos area desde metodo auxiliar
                    double area = DMU.Area(xYZs);
                    Parameter parameter = stairsRun.LookupParameter("AreaTramo");
                    //Asignamos parametro
                   if(parameter!=null) parameter.Set(area);
                }
            }

            // Cambiamos dato de área.
            foreach (ElementId addedElemId in data.GetModifiedElementIds())
            {
                if (doc.GetElement(addedElemId) is StairsRun stairsRun)
                {
                    //Si fuese segmentos rectos
                    // List<XYZ> xYZs = stairsRun.GetFootprintBoundary().ToList().Select(x => x.GetEndPoint(0)).ToList();
                   
                    List<XYZ> xYZs = new List<XYZ>();
                    //Obtenemos contorno del Tramo
                    CurveLoop curves = stairsRun.GetFootprintBoundary();
                    //Para cada curve. Pueden no ser Line
                    foreach (Curve curve in curves)
                    {
                        //Obtenemos punto extremos el Lines y puntos intermedios si no es Line
                        IList<XYZ> temp = curve.Tessellate();
                        //Añadimos a la lista
                        xYZs = xYZs.Concat(temp.ToList()).ToList();
                    }
                    //Obtenemos area desde metodo auxiliar
                    double area = DMU.Area(xYZs);
                    Parameter parameter = stairsRun.LookupParameter("AreaTramo");
                    //Asignamos parametro
                    if (parameter != null) parameter.Set(area);
                }
            }



        }

        public string GetAdditionalInformation()
        {
            return "Ejemplo de Updater. Para cualquier tramo de escalera se inserta el área en planta. En creación y modificación";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls | ChangePriority.InteriorWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "Revit API Manual Updater. Area de tramos de escalera";
        }
    }

}
