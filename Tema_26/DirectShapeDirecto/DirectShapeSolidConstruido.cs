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

namespace DirectShapeSolidConstruido
{
    [Transaction(TransactionMode.Manual)]
    public class DirectShapeSolidConstruido : IExternalCommand
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
            // Definimos centro
            XYZ center = XYZ.Zero;
            double radius = 2.0;

            //Dos puntos del diámetro
            XYZ profilePlus = center + new XYZ(0, radius, 0);
            XYZ profileMinus = center - new XYZ(0, radius, 0);

            //Creamos un semicírculo para girarlo 360
            List<Curve> profile = new List<Curve>();
            profile.Add(Line.CreateBound(profilePlus, profileMinus));
            profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

            //CurveLoop con semicírculo
            CurveLoop curveLoop = CurveLoop.Create(profile);

            //Creamos SolidOptions material y estilo = -1
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            //Definimos Frame y comprobamos 
            Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
            if (Frame.CanDefineRevitGeometry(frame) == false)
            {
                message = "Imposible crear DirectShape";
                return Result.Failed;
            }

            //Creamos un sólido de revolución
            Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);
            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name Solid");

                // Creamos una DirectShape en el doc categoría puertas
                Autodesk.Revit.DB.DirectShape ds = Autodesk.Revit.DB.DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Doors));

                //Completamos datos
                ds.ApplicationId = "Revit API Manual";
                ds.ApplicationDataId = "Revit API Manual. Creación esfera";
                //Asignamos geometría
                ds.SetShape(new GeometryObject[] { sphere });

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
