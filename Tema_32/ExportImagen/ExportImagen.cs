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

namespace ExportImagen
{
    [Transaction(TransactionMode.Manual)]
    public class ExportImagen : IExternalCommand
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

            #region Exportar
            //Obtenemos vista actual
            ViewPlan view = doc.ActiveView as ViewPlan;

            //Necisitanos path. Por ejemplo del rvt
            if (string.IsNullOrEmpty(doc.PathName))
            {
                message = "Proyecto no salvado";
                return Result.Cancelled;
            }
            //Obtenemos carpeta
            string dirName = System.IO.Path.GetDirectoryName(doc.PathName);
            //Construimos nombre sin extensión
            string filePath = dirName + "\\" + doc.Title;

            //Construimos lista de vistas
            IList<ElementId> ImageExportList = new List<ElementId>() { view.Id };

            //Construimos ImageExportOptions
            ImageExportOptions imageExportOptions = new ImageExportOptions
            {
                ZoomType = ZoomFitType.FitToPage,
                PixelSize = 500,
                FilePath = filePath,
                FitDirection = FitDirectionType.Horizontal,
                HLRandWFViewsFileType = ImageFileType.JPEGLossless,
                ImageResolution = ImageResolution.DPI_300,
                ShadowViewsFileType = ImageFileType.JPEGMedium,
                ExportRange = ExportRange.SetOfViews,               
            };

            //Asignamos lista de vistas
            imageExportOptions.SetViewsAndSheets(ImageExportList);

            //Exportamos a fichero
            doc.ExportImage(imageExportOptions);

            #endregion

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction ExportImagen");

                #region Importar. Crear ImageType
                //Obtenemos toso los ficheros de la carpeta
                string[] files = System.IO.Directory.GetFiles(dirName);
                //Obtenemos carpeta
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(dirName);
                //Obtenemos el ultimo fichero creado. La imagen
                string filename = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName;

                //Creamos ImageTypeOptions
                ImageTypeOptions imageTypeOptions = new ImageTypeOptions(filename, false, ImageTypeSource.Import);
                //Creamos ImageType
                ImageType imageType = ImageType.Create(doc, imageTypeOptions);
                #endregion

                #region Crear instancia
                //Creamos ImagePlacementOptions
                ImagePlacementOptions imagePlacementOptions = new ImagePlacementOptions();
                //Situamos en 0,0,0
                imagePlacementOptions.Location = XYZ.Zero;
                //Creamos instancia
                ImageInstance.Create(doc, view, imageType.Id, imagePlacementOptions);
                #endregion
                //Confirmamos Transaction
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
