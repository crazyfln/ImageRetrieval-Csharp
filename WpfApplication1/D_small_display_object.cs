using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls; 

namespace WpfApplication1
{
    public class D_small_display_object : BflyObject
    {
        public D_small_display_object()
        {
            rf_pos = rf_neg = false;
        }

        public D_small_display_object(string picture, string t)
        {
            img_path = picture;
            label = t;
            label_color = "Black";

            rf_pos = rf_neg = false;
        }

        public D_small_display_object(D_small_display_object d)
        {
            img_path = d.img_path;
            label_color = d.label_color;
            label = d.label;
            rf_pos = d.rf_pos;
            rf_neg = d.rf_neg;
        }

        public string img_path;
        public string label_color;
        public string label;

        public bool rf_pos;
        public bool rf_neg;
    }
}
