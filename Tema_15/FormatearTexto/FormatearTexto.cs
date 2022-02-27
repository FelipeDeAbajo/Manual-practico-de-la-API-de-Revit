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

namespace FormatearTexto
{
    [Transaction(TransactionMode.Manual)]
    public class FormatearTexto : IExternalCommand
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
            Reference reference = null;
            try
            {
                //PickObject siempre entre try{} catch{}
                reference = sel.PickObject(ObjectType.Element, new TextNoteSelectionFilter(), "Seleccionar texto");

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            //Obtenemos TextNote
            TextNote textNote = doc.GetElement(reference.ElementId) as TextNote;

            //Accedemos al texto con formato
            //FormattedText se utiliza para crear, editar y formatear texto en un TextNote
            //o para consultar el texto y las propiedades de formato de un TextNode
            FormattedText formatText = textNote.GetFormattedText();

            //Un TextRange consta de un inicio, que es un índice de base cero en el texto, y una longitud,
            //que es el número de caracteres en el rango. La longitud puede ser cero.

            // italicize "New"
            TextRange range = new TextRange(0, 3);
            formatText.SetItalicStatus(range, true);

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciar Transaction
                tx.Start("Transaction Name");
                // Pasamos la palabra  "párrafo" a negrita
                range = formatText.Find("párrafo", 0, false, true);
                if (range.Length > 0)
                    formatText.SetBoldStatus(range, true);

                // Pasamos la palabra  "Item" a subrrayado
                range = formatText.Find("Item", 0, false, true);
                if (range.Length > 0)
                    formatText.SetUnderlineStatus(range, true);

                // Pasamos a mayusculas
                formatText.SetAllCapsStatus(true);
              
                //Asignamos el resultado a la TextNote
                textNote.SetFormattedText(formatText);
                //Confirmar Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
    public class TextNoteSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element is TextNote)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }

}
