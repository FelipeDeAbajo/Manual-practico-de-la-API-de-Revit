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

namespace AgregarTexto
{
    [Transaction(TransactionMode.Manual)]
    public class AgregarTexto : IExternalCommand
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
            catch(Exception ex)
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
            TextRange range = formatText.AsTextRange();
            //establecemos el inicio del nuevo texto
            range.Start = range.End - 1;
            // Establecemos Longitud en 0 para insertar
            range.Length = 0;
            string someNewText = "\rEste es un nuevo párrafo\vEsta es una nueva línea sin un salto de párrafo\r";
            formatText.SetPlainText(range, someNewText);

            // Obtenemos rango para todo el texto
            range = formatText.AsTextRange();
            range.Start = range.End - 1;
            range.Length = 0;
            string someListText = "\rLista con viñetas. Item 1\rItem 2\vSegunda línea para Item 2\rItem 3";
            formatText.SetPlainText(range, someListText);
            range.Start++;
            range.Length = someListText.Length;
            formatText.SetListType(range, ListType.Bullet);

            if (formatText.GetAllCapsStatus(range) != FormatStatus.None)
            {
                formatText.SetAllCapsStatus(range, false);
            }
            //Crear Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciar Transaction
                tx.Start("Transaction agregar texto");
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
