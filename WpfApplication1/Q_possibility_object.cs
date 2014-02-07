using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class Q_possibility_object
    {
        public Q_possibility_object()
        {
        }

        public Q_possibility_object(string ip, string l)
        {
            img_path = ip;
            label = l;
        }

        public string img_path;
        public string label;
        public int pid;
    }
}
