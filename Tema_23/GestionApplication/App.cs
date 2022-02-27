#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace GestionApplication
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {

            return Result.Succeeded;
        }
    }

}
