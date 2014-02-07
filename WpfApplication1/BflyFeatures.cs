using System;
using System.Data;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace WpfApplication1
{
    public partial class BflyFeatures
    {
        public BflyFeatures()
        {
            pid = 0;
	        PFs = new double[Constants.NUM_PFS];
	        VF_cs = new int[Constants.NUM_VFS_PER_IMAGE];
	        VF_es = new double[Constants.NUM_VFS_PER_IMAGE];
        }
        public int pid;
	    public double[] PFs;
	    public int[] VF_cs;
	    public double[] VF_es;
    	
	    public bool has_VFs()
        {
            if (VF_cs[0] > 0)
                return true;
            else
                return false;
        }

        public void parse_all_features()
        {
            parse_PFs();
            parse_VFs();
        }

        public void parse_PFs()
        {
            if( pid < 1 || pid > Constants.DB_SIZE ) {
		        MessageBox.Show( "BflyFeatures::parse_PFs: invalid pid" +
                                 "BflyFeatures::parse_PFs");
		        return;
	        }

            try
            {
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                    Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                try
                {
                    MySqlConnection con = new MySqlConnection(connStr);
                    con.Open();
                    
                    
                    string command = String.Format("SELECT * FROM {0} WHERE pid = {1};",
                        Constants.BFLY_FEATURES_TABLE, pid);
                    
                    MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                    DataSet dataset = new DataSet("Results");
                    dadpt.Fill(dataset, "Bfly Features");
                    DataTable table = dataset.Tables["Bfly Features"];

                    for (int i = 0; i < Constants.NUM_PFS; i++)
                    {
                        try
                        {
                            PFs[i] = (double)table.Rows[0][i + Constants.DB_INDEX_OF_FIRST_PF];
                        }
                        catch (MySqlException Ex)
                        {
                            MessageBox.Show("had some issues when MySQL stored the value in scientific notation when it was too small: " + Ex.Message);
                            PFs[i] = 0;
                        }
                    }

                    return;
                    
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error connecting to the server: " + ex.Message);
                    return;
                }
            }
            catch ( MySqlException e )
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "E! QueryImage::parse_PFs()");
            }
        }

        public void parse_VFs()
        {
            if ( pid < 1 || pid > Constants.DB_SIZE )
            {
                MessageBox.Show("BflyFeatures::parse_VFs: invalid pid" +
                                 "BflyFeatures::parse_VFs");
                return;
            }

            try
            {
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                    Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                try
                {
                    MySqlConnection con = new MySqlConnection(connStr);
                    con.Open();


                    string command = String.Format("SELECT * FROM {0} WHERE pid = {1};",
                        Constants.BFLY_FEATURES_TABLE, pid);

                    MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                    DataSet dataset = new DataSet("Results");
                    dadpt.Fill(dataset, "Bfly Features");
                    DataTable table = dataset.Tables["Bfly Features"];

                    for (int db_i = Constants.DB_INDEX_OF_FIRST_VF, local_i = 0; local_i < Constants.NUM_VFS_PER_IMAGE; db_i+= 2, local_i++)
                    {
                        VF_cs[local_i] = int.Parse(table.Rows[0][db_i].ToString());
                        VF_es[local_i] = double.Parse(table.Rows[0][db_i + 1].ToString());
                    }

                    return;

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error connecting to the server: " + ex.Message);
                    return;
                }
            }
            catch (MySqlException e)
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "EXCEPTION! QueryImage::parse_VFs()");

                Environment.Exit(2);
            }
        }
    }
}
