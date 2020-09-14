using System;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.PlugIns;

namespace Connect
{
    public class ConnectInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Connect";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("c1762d88-5774-45b2-9a69-fd318279144b");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Povl Filip Sonne-Frederiksen";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }

        public static string Plugin => "Axis";
        public static string Tab => "Connect";


    }
}
