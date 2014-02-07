using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace WpfApplication1
{
    public class BflyObject : BflyFeatures
    {
        public BflyObject()
        {
            photo_bottom = new StringBuilder("");
            photo_top = new StringBuilder("");
        }

        public StringBuilder photo_top;
        public StringBuilder photo_bottom;
        public int group_index;
        public float mu;
        public float precision_pre;
        public float average_pre;
        public int local_vf_counter;

        public String print_info()
        {
            StringBuilder temp = new StringBuilder( "" );
            temp.Append( "pid: ");
            temp.Append( this.pid );
            temp.Append( "\r Group: ");
            temp.Append( this.group_index );
            temp.Append("\r\nEta: ");
            temp.Append(this.mu);
            temp.Append("\r\nPrecision_pre: ");
            temp.Append(this.precision_pre);
            temp.Append("\r\nAverage_precision: ");//////////////////////////////////////////
            temp.Append(this.average_pre);////////////////////////////////////////	
            temp.Append("\r\nNumber of VFs: ");///////////////////////////
            temp.Append(this.local_vf_counter);///////////////////
            return temp.ToString();       
        }

        public void parse_metadata(DataRow row)
        {
            if (row["photo_bottom"].ToString() != null)
            {
                parse_photos(row["photo_top"].ToString(), row["photo_bottom"].ToString());
            }
            else
            {
                string str = "";
                parse_photos(row["photo_top"].ToString(), str);
            }

            pid = (int)row["pid"];
            group_index = (int)row["group_index"];
            mu = (float)row["eta"];
            precision_pre = (float)row["precision_pre"];
            average_pre = (float)row["average_pre"]; //////////////////////////// 
            local_vf_counter = (int)row["number_of_vf"];////////////////////////
        }

        private void parse_photos(String top, String bottom)
        {
            StringBuilder temp = new StringBuilder("");
            temp.Append(Constants.LARGE_IMG_FOLDER);
            temp.Append(top);
            temp.Append(".jpg");
            try
            {
                photo_top.Remove(0, photo_top.Length);
                photo_top.Append(temp.ToString());
            }
            catch (Exception e)
            {
                string e_string = e.ToString();
            }
            temp.Remove(0, temp.Length);

	        temp.Append(Constants.LARGE_IMG_FOLDER);
	        temp.Append(bottom);
	        temp.Append(".jpg");
        	
	        photo_bottom.Remove(0, photo_bottom.Length);
	        photo_bottom.Append(temp.ToString());
        }
    }
}
