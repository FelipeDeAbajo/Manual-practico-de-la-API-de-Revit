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

namespace CrearArchivoLocal
{
    [Transaction(TransactionMode.Manual)]
    public class CrearArchivoLocal : IExternalCommand
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

            // Access current selection
            // Create the new local at the given path
            WorksharingUtils.CreateNewLocal(centralPath, localPath);

            // Select specific worksets to open
            // First get a list of worksets from the unopened document
            IList<WorksetPreview> worksets = WorksharingUtils.GetUserWorksetInfo(localPath);
            List<WorksetId> worksetsToOpen = new List<WorksetId>();

            foreach (WorksetPreview preview in worksets)
            {
                // Match worksets to open with criteria
                if (preview.Name.StartsWith("O"))
                    worksetsToOpen.Add(preview.Id);
            }

            // Setup option to open the target worksets
            // First close all, then set specific ones to open
            WorksetConfiguration worksetConfig = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            worksetConfig.Open(worksetsToOpen);

            // Open the new local
            OpenOptions options1 = new OpenOptions();
            options1.SetOpenWorksetsConfiguration(worksetConfig);
            //-------
            // Setup options
            OpenOptions options1 = new OpenOptions();

            // Default config opens all.  Close all first, then open last viewed to get the correct settings.
            WorksetConfiguration worksetConfig = new WorksetConfiguration(WorksetConfigurationOption.OpenLastViewed);
            options1.SetOpenWorksetsConfiguration(worksetConfig);

            // Open the document
            Document openedDoc = app.OpenDocumentFile(GetWSAPIModelPath("WorkaredFileSample.rvt"), options1);
            //-------
            Document openedDoc = app.OpenDocumentFile(localPath, options1);


            return Result.Succeeded;
        }
    }
}
