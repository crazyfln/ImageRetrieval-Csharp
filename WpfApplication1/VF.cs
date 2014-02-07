using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class VF : IComparable
    {
        public int c;
        public double e;

        public VF()
        {
            c = 0;
            e = 0;
        }

        public VF( int cc, double ee )
        {
            c = cc;
            e = ee;
        }

        public VF( VF two )
        {
            c = two.c;
            e = two.e;
        }

        public int CompareTo(Object obj)
        {
            VF otherVF = obj as VF;
            if (otherVF != null)
                return this.c.CompareTo(otherVF.c);
            else
                throw new ArgumentException("Object is not a VF");
        }

        public static bool operator<(VF one, VF two)
        {
            return one.c < two.c;
        }

        public static bool operator>(VF one, VF two)
        {
            return one.c > two.c;
        }
    }
}
