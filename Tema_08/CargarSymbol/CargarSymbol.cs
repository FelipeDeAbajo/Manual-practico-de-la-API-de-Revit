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

namespace CargarSymbol
{
    [Transaction(TransactionMode.Manual)]
    public class CargarSymbol : IExternalCommand
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

            //Creamos un nombre para el fichero
            string nombreFichero = string.Empty;

            //Creamos un formulario de lectura de ficheros
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            //Obtenemos todas las bibliotecas instaladas
            IDictionary<string, string> keyValuePair = doc.Application.GetLibraryPaths();
            //Obtenemos la que coincide con nuestro nombre
            //Lo pasamos como carpeta inicial al formulario
            openFileDialog.InitialDirectory = keyValuePair["API Revit Manual"];

            //Filtramos tipo de archivos para leer
            openFileDialog.Filter = "Familias de Revit (*.rfa)|*.rfa";

            //Mostramos el formulario
            System.Windows.Forms.DialogResult dialogResult = openFileDialog.ShowDialog();
            //Si cancelamos o cerramos sin seleccionar archivo válido
            if (dialogResult != System.Windows.Forms.DialogResult.OK)
            {
                message = "Seleccion cancelada";
                return Result.Cancelled;

            }
            //asignamos el nombre del fichero
            nombreFichero = openFileDialog.FileName;
            if (!System.IO.File.Exists(nombreFichero))
            {
                message = "El archivo seleccionado no existe";
                return Result.Failed;
            }
            //Abrimos el fichero de familia.
            //No lo pasamos al UIDocument
            Document familyDoc = doc.Application.OpenDocumentFile(nombreFichero);
            
            //Leemos desde el FamilyDocument todos los tipos
            FamilyTypeSet familyTypeSet = familyDoc.FamilyManager.Types;
            FamilyTypeSetIterator familyTypeSetIterator = familyTypeSet.ForwardIterator();
            //Reseteamos el Iteraror
            familyTypeSetIterator.Reset();
             
            //Creamos string para
            string textoSalida = string.Empty;
            //Creamos string para nombre de tipo
            string nombreSimbol = string.Empty;
            while (familyTypeSetIterator.MoveNext())       
            {
                FamilyType familyType = familyTypeSetIterator.Current as FamilyType;
                nombreSimbol = familyType.Name;
                if (string.IsNullOrWhiteSpace(nombreSimbol))
                {
                    //Almacenamos el tipo. Podríamos salir, pero recooremos todos los tipos
                    nombreSimbol = System.IO.Path.GetFileNameWithoutExtension(nombreFichero);
                }
                //Construimos string con todos los tipos
                textoSalida = (textoSalida == string.Empty) ? nombreSimbol : textoSalida + "\n" + nombreSimbol;
            }
            //Mostramos todos los tipos
            TaskDialog.Show("API", textoSalida);
            //Creamos string con nombre completo de archivo.
            //Coincide con el seleccionado en el OpenFileDialog
            string nombrePath = familyDoc.PathName;
            //Cerramos la family. La hemos abierto solo para consultar el nombre
            familyDoc.Close(false);

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");
                //Cargamos solamente el primer tipo
                doc.LoadFamilySymbol(nombrePath, nombreSimbol, new OpcionesCargaFamiliasMin(), out FamilySymbol familySymbol);
                //Antes de crear una FamilyInstance hay que activar el tipo
                familySymbol.Activate();
                //Creamos una FanmilyInstance
                doc.Create.NewFamilyInstance(XYZ.Zero, familySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                //Conformamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
    //Interface minima con  escritura de parametros
    class OpcionesCargaFamiliasMin : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            //Sobreescribimos los parametros existentes, si ya esta leida
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            //Devolvemos la Family recien cargada
            source = FamilySource.Family;
            //Sobreescribimos los parametros existentes, si ya esta leida
            overwriteParameterValues = true;
            return true;
        }
       
    }
}
