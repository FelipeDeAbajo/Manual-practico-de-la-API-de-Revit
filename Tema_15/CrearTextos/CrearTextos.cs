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

namespace CrearTextos
{
    [Transaction(TransactionMode.Manual)]
    public class CrearTextos : IExternalCommand
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
                textLoc = uidoc.Selection.PickPoint("Seleccionar punto para el texto.");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            //Obtenemos ei tipo por defecto
            ElementId defaultTextTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
            //Establecemos ancho
            //Depende de la escala de impresión de la vista
            double noteWidth = 0.2;

            // Nos aseguramos que el ancho está dentro de tolerancias
            double minWidth = TextNote.GetMinimumAllowedWidth(doc, defaultTextTypeId);
            double maxWidth = TextNote.GetMaximumAllowedWidth(doc, defaultTextTypeId);
            if (noteWidth < minWidth)
            {
                noteWidth = minWidth;
            }
            else if (noteWidth > maxWidth)
            {
                noteWidth = maxWidth;
            }

            //Creamos TextNoteOptions configuración básica
            TextNoteOptions opts = new TextNoteOptions(defaultTextTypeId);
            //Alineacion horizonrtal
            opts.HorizontalAlignment = HorizontalTextAlignment.Left;
            //Angulo del tecto
            opts.Rotation = Math.PI / 4;
            //Mantener legible
            opts.KeepRotatedTextReadable = true;
            //Alineación vertical
            opts.VerticalAlignment = VerticalTextAlignment.Top;

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");

                //Creamos TextNote
                TextNote textNote = TextNote.Create(doc, doc.ActiveView.Id, textLoc, noteWidth, "Texto de ejemplo", opts);
                //Añadimos directriz Recta Right
                textNote.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_R);
                //Alineacion de Directriz
                textNote.LeaderRightAttachment = LeaderAtachement.Midpoint;
                //Obtenemos directriz recien añadida
                Leader leader = textNote.GetLeaders()[0];
                //Punto final de la directriz
                leader.End = XYZ.Zero;

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
