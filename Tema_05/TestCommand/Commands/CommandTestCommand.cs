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

namespace TestCommand.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    public class CommandTestCommand : IExternalCommand
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
            System.Windows.Forms.MessageBox.Show(doc.Title);
            // Access current selection

            Selection sel = uidoc.Selection;

            // Retrieve elements from database

            FilteredElementCollector col
              = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Wall));

            // Filtered element collector is iterable
            int contador = 0;
            foreach (Element e in col)
            {
                contador++;
                Debug.Print(e.Name);
            }
            System.Windows.Forms.MessageBox.Show(contador.ToString());
            // Modify document within a transaction

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Change ProjectInfo");
                doc.ProjectInformation.Author = "Defecto";
                commandData.JournalData.Add("Dato Test", "Cambiado el autor del proyecto");
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
