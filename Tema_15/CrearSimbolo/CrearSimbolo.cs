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

namespace CrearSimbolo
{
    [Transaction(TransactionMode.Manual)]
    public class CrearSimbolo : IExternalCommand
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
            XYZ textLoc = XYZ.Zero;
            try
            {
                //PickPoint siempre entre try{} catch{}
                textLoc = uidoc.Selection.PickPoint("Seleccionar punto para el simbolo.");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            //Siempre existe por lo menos un AnnotationSymbolType
            IList<Element> symbols = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericAnnotation).WhereElementIsElementType().ToElements();
            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");

                //creamos y guardamos FamilyInstance
                FamilyInstance familyInstance = doc.Create.NewFamilyInstance(textLoc, symbols.FirstOrDefault() as FamilySymbol, uidoc.ActiveView);
                //Obtenemos AnnotationSymbol
                AnnotationSymbol symbol = familyInstance as AnnotationSymbol;

                //Obtenemos todas las directrices, Redundante, sabemos que ahora no hay
                IList<Leader> leaders = symbol.GetLeaders();
              
                // Si hubiese alguna directriz la eliminamos
                if (leaders != null && leaders.Count > 0)
                {
                    for (int i = leaders.Count; i > 0; i--)
                    {
                        symbol.removeLeader();
                    }
                }

                // Añadimos una directriz
                symbol.addLeader();
                //Obtenemos la directriz recien creada
                Leader leader = symbol.GetLeaders()[0];
                //Ponemos el fin de la directriz en (0,0,0)
                leader.End = XYZ.Zero;
       
                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
