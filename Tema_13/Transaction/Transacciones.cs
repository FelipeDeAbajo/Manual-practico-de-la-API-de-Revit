#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace Transacciones
{
    [Transaction(TransactionMode.Manual)]
    public class Transacciones : IExternalCommand
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
            //Creamos TransactionGroup
           using (TransactionGroup transactionGroup = new TransactionGroup(doc, "Dibujar lineas"))
            {
                //Iniciamos TransactionGroup
                transactionGroup.Start();

                //Creamos Transaction
                Transaction txPlano = new Transaction(doc);
                //Iniciamos Transaction
                txPlano.Start("Transaction Plano de trabajo");

                //Creamos 4 puntos
                XYZ xYZ1 = XYZ.Zero;
                XYZ xYZ2 = new XYZ(10, 0, 0);
                XYZ xYZ3 = new XYZ(10, 10, 0);
                XYZ xYZ4 = new XYZ(0, 10, 0);
                //Creamos un Plane
                Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
                //En la linea 35 iniciamos esta transacción. Nunca se ejecutara
                if (!txPlano.HasStarted()) txPlano.Start("Transaction Plano de trabajo");

                //Creamos un SketchPlane para poder crear ModelLines
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                //Damos nombre al plano
                sketchPlane.Name = "Revit API Manual";

                //Conformamos Transaction
                txPlano.Commit();
                //Anulamos Transaction. No existe el SketchPlane y no podemos crear ModelLines
                // tx.RollBack();

                //En lineas 53 o 55 cerramos la anterior Transaction podemos crear una nueva Transaction
                //encampusala entre llaves. Mejor opción
                using (Transaction tx = new Transaction(doc))
                {
                    //Iniciamos Transaction
                    tx.Start("Transaction 3 lineas");
                    //Creamos una SubTransaction encapsulada
                    using (SubTransaction subTransaction1 = new SubTransaction(doc))
                    {
                        //Iniciamos SubTransaction
                        subTransaction1.Start();
                        ModelLine modelLine = CrearLineas(uidoc, sketchPlane, xYZ1, xYZ2);
                        //Confirmamos SubTransaction
                        subTransaction1.Commit();
                    }

                    //Creamos 2 ModelLine, dentro de la Transaction tx
                    CrearLineas(uidoc, sketchPlane, xYZ2, xYZ3);
                    CrearLineas(uidoc, sketchPlane, xYZ3, xYZ4);

                    //Confirmamos la transacción
                    tx.Commit();
                    //Anulamos la Transaction. Se anulan las 3 ModelLines
                   // tx.RollBack();
                }
                //Aunque no estan las lineas 78 ni 80, al salir del encapsulado la Transaction de linea 59 se cierra
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Transaction 1 linea");
                    CrearLineas(uidoc, sketchPlane, xYZ4, xYZ1);
                    //Confirmamos la transacción
                    tx.Commit();
                    //Anulamos la Transaction. Se anulan la ModelLine
                    //tx.RollBack();
                }
                //Confirmamos TransactionGroup. Tenemos en menú Deshacer 3 Transaction
               // transactionGroup.Commit();
                //Asimilamos TransactionGroup. Tenemos en menú Deshacer 1 sola Transaction
               transactionGroup.Assimilate();
            }
            return Result.Succeeded;
        }
        /// <summary>
        /// Creación de ModelLine
        /// </summary>
        /// <param name="uidoc">UIDocument</param>
        /// <param name="sketchPlane">Plano de trabajo</param>
        /// <param name="xYZ1">Punto 1</param>
        /// <param name="xYZ2">Punto 2</param>
        /// <returns></returns>
        public ModelLine CrearLineas(UIDocument uidoc, SketchPlane sketchPlane, XYZ xYZ1, XYZ xYZ2)
        {
            Document doc = uidoc.Document;
            Line line1 = Line.CreateBound(xYZ1, xYZ2);
            return doc.Create.NewModelCurve(line1, sketchPlane) as ModelLine;
        }
    }
}
