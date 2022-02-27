#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
#endregion

namespace GraficosTemp
{
    [Transaction(TransactionMode.Manual)]
    public class GraficosTemp : IExternalCommand
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

            //Generamos un Guid
            Guid nGuid = Guid.NewGuid();

            // Accedemos a la selección actual
            Selection sel = uidoc.Selection;

            // Chequeamos que solo tenemos un objeto seleccionado
            if (sel.GetElementIds().Count != 1)
            {
                message = "Se debe seleccionar un solo elemento";
                return Result.Failed;
            }

            // Chequeamos que el objeto seleccionado es Muro
            if (doc.GetElement(sel.GetElementIds().First()) is Wall wall)
            {
                //Npmbre completo del actual fichero GraficosTemp.dll
                string filename = System.Reflection.Assembly.GetExecutingAssembly().Location;
                //Carpeta del actual GraficosTemp.dll
                string folder = (new System.IO.FileInfo(filename)).Directory.FullName;

                MultiServerService externalService = ExternalServiceRegistry.GetService(
                    ExternalServices.BuiltInExternalServices.TemporaryGraphicsHandlerService) as MultiServerService;

                MyGraphicsService myGraphicsService = new MyGraphicsService(nGuid, wall.Id);

                externalService.AddServer(myGraphicsService);
                externalService.SetActiveServers(new List<Guid> { myGraphicsService.GetServerId() });

                TemporaryGraphicsManager mgr = TemporaryGraphicsManager.GetTemporaryGraphicsManager(doc);

                //Calculamos punto medio wall
                XYZ controlPoint = ((LocationCurve)wall.Location).Curve.Evaluate(0.5, true);
                //Asignamos imagen y punto
                InCanvasControlData data = new InCanvasControlData(folder + "\\excel.bmp", controlPoint);
                //Asignamos el control
                mgr.AddControl(data, doc.ActiveView.Id);

            }
            else
            {
                message = "Se debe seleccionar muro";
                return Result.Failed;
            }

            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Gráfico temporal creado");
            return Result.Succeeded;
        }

        public class MyGraphicsService : ITemporaryGraphicsHandler
        {
            //Propiedad guid
            Guid _guid;
            //Propiedad ElemenId
            ElementId _elementId;
            //Constructor explicito
            public MyGraphicsService(Guid guid, ElementId elementId)
            {
                _guid = guid;
                _elementId = elementId;
            }
            public void OnClick(TemporaryGraphicsCommandData data)
            {
                //Npmbre completo del actual fichero GraficosTemp.dll
                string filename = System.Reflection.Assembly.GetExecutingAssembly().Location;
                //Carpeta del actual GraficosTemp.dll
                string folder = (new System.IO.FileInfo(filename)).Directory.FullName;
                //Nombre completo del archivo csv
                string path = folder + "\\excel.csv";

                //Escritura del id en el fichero csv
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(_elementId);
                }

                //Arrancamos excel con el csv
                Process pr = new Process();
                pr.StartInfo = new ProcessStartInfo()
                {
                    FileName = "excel.exe",
                    Arguments = path
                };
                pr.Start();

            }
            public string GetName()
            { return "Vinculo a excel"; }
            public string GetDescription()
            { return "Graphics service. Excel"; }
            public string GetVendorId()
            { return "FdAA"; }
            public ExternalServiceId GetServiceId()
            { return ExternalServices.BuiltInExternalServices.TemporaryGraphicsHandlerService; }
            public Guid GetServerId()
            {
                return _guid;
            }
        }
    }
}
