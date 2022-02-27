using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ribbon
{
   internal class GetIconFromDll
    {
        internal static BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                Stream s = a.GetManifestResourceStream(name);
                return BitmapFrame.Create(s);
            }
            catch
            {
                return null;
            }
        }
    }
}
