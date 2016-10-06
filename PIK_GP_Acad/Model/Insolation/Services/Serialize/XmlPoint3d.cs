using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Services
{
    public class XmlPoint3d
    {
        private double x;
        private double y;
        private double z;

        public XmlPoint3d () { }
        public XmlPoint3d (Point3d point)
        {
            x = point.X;
            y = point.Y;
            z = point.Z;
        }

        public Point3d ToPoint ()
        {
            return new Point3d(x, y, z);
        }

        public void FromPoint (Point3d p)
        {
            x = p.X;
            y = p.Y;
            z = p.Z;
        }

        public static implicit operator Point3d (XmlPoint3d x)
        {
            return x.ToPoint();
        }

        public static implicit operator XmlPoint3d (Point3d p)
        {
            return new XmlPoint3d(p);
        }

        [XmlAttribute]
        public double X {
            get { return x; }
            set { x = value; }
        }
        [XmlAttribute]
        public double Y {
            get { return y; }
            set { y = value; }
        }
        [XmlAttribute]
        public double Z {
            get { return z; }
            set { z = value; }
        }
    }
}
