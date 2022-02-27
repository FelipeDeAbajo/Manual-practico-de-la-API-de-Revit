#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace RevitAddin20211
{
    [Transaction(TransactionMode.Manual)]
    public class RevitAddin20211 : IExternalCommand
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

            Selection sel = uidoc.Selection;
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(RebarBarType));

            RebarBarType rebarType = col.FirstOrDefault() as RebarBarType;
            col = new FilteredElementCollector(doc).OfClass(typeof(RebarHookType));
            RebarHookType rebarHookType = col.FirstOrDefault() as RebarHookType;

            col = new FilteredElementCollector(doc).OfClass(typeof(RebarShape));
            RebarShape rebarShape = col.Where(x =>x.Name=="41").FirstOrDefault() as RebarShape;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");
            // Retrieve elements from database
          FamilyInstance beam=  doc.GetElement(sel.GetElementIds().First()) as FamilyInstance;
            RebarContainer rebarContainers = CreateRebarContainer(doc, beam);
            AddItemsToRebarContainer(rebarContainers, beam, rebarType, null, rebarShape);
          
                tx.Commit();
            }

            return Result.Succeeded;
        }
        RebarContainer CreateRebarContainer(Autodesk.Revit.DB.Document document, FamilyInstance beam)
        {
            // Create a new rebar container
            ElementId defaultRebarContainerTypeId = RebarContainerType.CreateDefaultRebarContainerType(document);
            RebarContainer container = RebarContainer.Create(document, beam, defaultRebarContainerTypeId);

            // Any items for this container should be presented in schedules and tags as separate subelements
            container.PresentItemsAsSubelements = true;

            return container;
        }
        void AddItemsToRebarContainer(RebarContainer container, FamilyInstance beam, RebarBarType barType, RebarHookType hookType, RebarShape rebarShape)
        {
            // Define the rebar geometry information - Line rebar
            LocationCurve location = beam.Location as LocationCurve;
            XYZ origin = location.Curve.GetEndPoint(0);
            // create rebar along the length of the beam
            XYZ rebarLineEnd = location.Curve.GetEndPoint(1);
            Line line = Line.CreateBound(origin, rebarLineEnd);
            XYZ normal = new XYZ(1, 0, 0);
            Curve rebarLine = line.CreateOffset(0.5, normal);

            // Create the line rebar
            IList<Curve> curves = new List<Curve>();
            curves.Add(rebarLine);

           //  RebarContainerItem item = container.AppendItemFromCurves(RebarStyle.Standard, barType, hookType, hookType, normal, curves, RebarHookOrientation.Right, RebarHookOrientation.Left, true, true);
            RebarContainerItem item = container.AppendItemFromRebarShape(rebarShape, barType, origin, XYZ.BasisY, XYZ.BasisZ);
            
            if (null != item)
            {
                // set specific layout for new rebar as fixed number, with 10 bars, distribution path length of 1.5'
                // with bars of the bar set on the same side of the rebar plane as indicated by normal
                // and both first and last bar in the set are shown
                item.SetLayoutAsFixedNumber(10, 1.5, true, true, true);
            }

            // Hide the new item in the active view
            container.SetItemHiddenStatus(container.Document.ActiveView, item.ItemIndex, false);
        }
    }
}
