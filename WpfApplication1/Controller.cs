using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Windows;

namespace WpfApplication1
{
    public class Controller
    {
        public Controller()
        {
            Random rand = new Random((int)System.DateTime.Now.Ticks);
            model = new Model();
        }

        public Controller(Model m, MyView v)
        {
            model = m;
            view = v;
            Random rand = new Random((int)System.DateTime.Now.Ticks);
        }

        public DataTable generate_query( int pid, ref int num_specimens )
        {
            DataTable table = new DataTable();
            try
            {
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                    Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                MySqlConnection con = new MySqlConnection(connStr);
                con.Open();
                if (con.State == ConnectionState.Closed)
                {
                    Console.Write("\n\nFAILED TO CONNECT TO DB. EXITING. (press return)\n");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                string command = String.Format("SELECT * FROM {0} WHERE pid = {1};", Constants.BFLY_OBJECTS_TABLE, pid);
                MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                DataSet dataset = new DataSet("Results");
                dadpt.Fill(dataset, "Bfly Objects");
                table = dataset.Tables["Bfly Objects"];

                string command1 = String.Format("SELECT group_index, COUNT(*) FROM {0} WHERE group_index = {1};", Constants.BFLY_FEATURES_TABLE, table.Rows[0]["group_index"]);
                MySqlDataAdapter dadpt1 = new MySqlDataAdapter(command1, con);
                DataSet dataset1 = new DataSet("Result");
                dadpt1.Fill(dataset1, "Bfly Features");
                DataTable table1 = dataset1.Tables["Bfly Features"];

                num_specimens = (int)table1.Rows[0][1];
                con.Close();

            }
            catch( MySqlException e )
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "Controller::generate_query(): MySQL++ EXCEPTION!");
                
            }
            return table;
        }

        public DataTable generate_queries( ref int[] num_specimens )
        {
            DataTable table = new DataTable();
            DataTable table1 = new DataTable();
            try
            {
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                    Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                MySqlConnection con = new MySqlConnection(connStr);
                con.Open();
                if (con.State == ConnectionState.Closed)
                {
                    Console.Write("\n\nFAILED TO CONNECT TO DB. EXITING. (press return)\n");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                string command = String.Format("SELECT * FROM {0} WHERE ", Constants.BFLY_OBJECTS_TABLE);
                int temp;
                List<int> nums = new List<int>();
                Random rnd = new Random();
                for (int i = 0; i < Constants.NUM_Q_POSSIBILITIES; i++)
                {
                    while( true )
                    {
                        temp = rnd.Next(Constants.DB_SIZE) + 1;
                        if (!nums.Contains(temp))
                        {
                            nums.Add(temp);
                            break;
                        }
                    }
                    command += String.Format("pid = {0}", temp);
                    if (i < Constants.NUM_Q_POSSIBILITIES - 1)
                        command += " || ";
                    else
                        command += ";";
                }
                MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                DataSet dataset = new DataSet("Results");
                dadpt.Fill(dataset, "Bfly Objects");
                table = dataset.Tables["Bfly Objects"];
                Console.WriteLine("The number of rows is: {0}", table.Rows.Count);
                //for (int i = 0; i < Constants.NUM_Q_POSSIBILITIES; i++)
                //{
                //    command = String.Format
                //}

                string command1 = String.Format("SELECT group_index, COUNT(*) FROM {0} WHERE", Constants.BFLY_FEATURES_TABLE);
                try
                {
                    for (int i = 0; i < Constants.NUM_Q_POSSIBILITIES; i++)
                    {
                        command1 += String.Format(" group_index = {0}", int.Parse(table.Rows[i]["group_index"].ToString()));
                        if (i == Constants.NUM_Q_POSSIBILITIES - 1)
                            command1 += " GROUP BY group_index;";
                        else
                            command1 += " ||";
                    }
                }
                catch (Exception e)
                {
                    string e_string = e.ToString();
                    MessageBox.Show(e_string);
                }
                MySqlDataAdapter dadpt1 = new MySqlDataAdapter(command1, con);
                DataSet dataset1 = new DataSet("Result");
                dadpt1.Fill(dataset1, "Bfly Features");
                table1 = dataset1.Tables["Bfly Features"];

                for (int i = 0; i < Constants.NUM_Q_POSSIBILITIES; i++)
                {
                    for (int j = 0; j < table1.Rows.Count; j++)
                    {
                        if (table.Rows[i]["group_index"] == table1.Rows[j]["group_index"])
                        {
                            num_specimens[i] = int.Parse(table1.Rows[j][1].ToString());
                            break;
                        }
                    }
                }

                con.Close();

            }
            catch( MySqlException e )
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "Controller::generate_queries(): MySQL++ EXCEPTION!");
                Environment.Exit(2);
            }
            return table;
        }

        public DataTable new_query(BflyObject q)
        {
            try
            {
                int pid = q.pid;
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                    Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                MySqlConnection con = new MySqlConnection(connStr);
                con.Open();
                if (con.State == ConnectionState.Closed)
                {
                    Console.Write("\n\nFAILED TO CONNECT TO DB. EXITING. (press return)\n");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                string command = String.Format("SELECT * FROM {0} WHERE pid = {1};", Constants.BFLY_OBJECTS_TABLE, pid);
                MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                DataSet dataset = new DataSet("Results");
                try
                {
                    dadpt.Fill(dataset, "Bfly Objects");
                }
                catch (MySqlException e)
                {
                    string e_string = e.ToString();
                    MessageBox.Show("Controller::new_query: Querying for nonexistant image (pid not in db).", "Something smells foul..");
                }
                DataTable table = dataset.Tables["Bfly Objects"];

                q.parse_metadata(table.Rows[0]);
                q.parse_VFs();
                q.parse_PFs();

                //return model.get_knn(q);
            }
            catch( MySqlException e )
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "E! Controller::new_query");
                Environment.Exit(2);
            }
            DataTable returnTable = new DataTable();
            try
            {
                returnTable = model.get_knn(q);
            }
            catch (Exception e)
            {
                string e_string = e.ToString();
            }
            return returnTable;
        }

        public void parse_meta(ref BflyObject q)
        {
            try
            {
                int pid = q.pid;
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                    Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                MySqlConnection con = new MySqlConnection(connStr);
                con.Open();
                if (con.State == ConnectionState.Closed)
                {
                    Console.Write("\n\nFAILED TO CONNECT TO DB. EXITING. (press return)\n");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                string command = String.Format("SELECT * FROM {0} WHERE pid = {1};", Constants.BFLY_OBJECTS_TABLE, pid);
                MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                DataSet dataset = new DataSet("Results");
                try
                {
                    dadpt.Fill(dataset, "Bfly Objects");
                }
                catch (MySqlException e)
                {
                    string e_string = e.ToString();
                    MessageBox.Show("Controller::new_query: Querying for nonexistant image (pid not in db).", "Something smells foul..");
                }
                DataTable table = dataset.Tables["Bfly Objects"];

                q.parse_metadata(table.Rows[0]);


                //return model.get_knn(q);
            }
            catch (MySqlException e)
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "E! Controller::new_query");
                Environment.Exit(2);
            }
        }

        public DataTable process_RF(BflyObject q, D_small_display_object[] displays)
        {
            //NOTE: when changing this, also need to change Model::get_knn()
            return this.process_RF_with_VFs(q, displays);
            //return this->process_RF_no_VFs( q, displays );
        }

        public int get_vf()
        {
            return model.return_vf();
        }

        private DataTable process_RF_no_VFs(BflyObject q, D_small_display_object[] displays)
        {
            model.do_qvm(q, displays);
            DataTable table = model.get_knn(q);
            return table;
        }

        private DataTable process_RF_with_VFs( BflyObject q, D_small_display_object[] displays )
        {
            model.update_q_mu_and_precision( q, displays );
            model.update_D_VFs(q, displays);
            model.do_qvm(q, displays);
            model.update_q_VFs(q, displays);
            model.increment_VF_counter();
            DataTable table = model.get_knn(q);
            return table;
        }

        private MyView view;
        private Model model;
    }
}
