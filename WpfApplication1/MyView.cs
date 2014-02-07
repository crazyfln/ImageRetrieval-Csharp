using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;


namespace WpfApplication1
{
    public partial class MyView
    {
        private Window1 window;
        
        public MyView( Window1 win )
        {
            this.window = win;
        }

        public bool update_D_displays( MySqlDataReader res )
        {
            return true;
        }

        private void setup_D_objects()
        {
            return;
        }

        private void setup_Q_objects()
        {
            return;
        }
    }
}
