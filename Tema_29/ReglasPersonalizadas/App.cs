#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

#endregion

namespace ReglasPersonalizadas
{
    class App : IExternalApplication
    {
        //Definimos instancia
        ReglasPersonalizadas.FlippedWindowCheck flippedWindowCheck;

        public Result OnStartup(UIControlledApplication a)
        {
            //Creamos nueva instancia
            flippedWindowCheck = new ReglasPersonalizadas.FlippedWindowCheck();

            //Agregamos y registramos la regla
            PerformanceAdviser.GetPerformanceAdviser().AddRule(flippedWindowCheck.m_Id, flippedWindowCheck);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            //Eliminamos del registro la regla
            PerformanceAdviser.GetPerformanceAdviser().DeleteRule(flippedWindowCheck.m_Id);
            return Result.Succeeded;
        }
    }
}
