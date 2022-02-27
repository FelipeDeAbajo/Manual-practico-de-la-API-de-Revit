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

namespace EditorTextoOpciones
{
    [Transaction(TransactionMode.Manual)]
    public class EditorTextoOpciones : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            TextEditorOptions editorOptions = TextEditorOptions.GetTextEditorOptions();
            editorOptions.ShowBorder = false;
            editorOptions.ShowOpaqueBackground = false;

            return Result.Succeeded;
        }
    }
}
