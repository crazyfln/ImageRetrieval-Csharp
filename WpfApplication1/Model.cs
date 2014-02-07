using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;

namespace WpfApplication1
{
    public partial class Model
    {
        public Model()
        {
            knn_list = new MyList();
            load_bfly_features();
            load_vf_counter();
            new_vf = false;
        }

        public DataTable get_knn(BflyObject q)
        {
            return this.PF_and_VF_knn(q);
        }

        public void do_qvm(BflyObject q, D_small_display_object[] Ds)
        {
            standard_qvm(q, Ds);
            //krystof_qvm(q, Ds);
            return;
        }

        public DataTable PF_only_knn(BflyObject q)
        {
            knn_list.reset();

            double temp_distance;
            int query_pid = q.pid;
            //find the knn
            for (int i = 0; i < bfly_features.GetLength(0); i++)
            {
                //don't look at the query image..
                if (bfly_features[i].pid == query_pid)
                    continue;

                temp_distance = get_PF_distance(q, i);
                knn_list.conditional_insert(bfly_features[i].pid, temp_distance);
            }
            DataTable ordered_res = new DataTable();
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
                int[] pids = knn_list.get_pids();
                string command = String.Format("SELECT * FROM {0} WHERE ", Constants.BFLY_FEATURES_TABLE);
                for (int i = 0; i < Constants.NUM_D_RESULTS; i++)
                {
                    command += String.Format("pid = {0}", pids[i]);
                    if (i < Constants.NUM_D_RESULTS)
                        command += " || ";
                    else
                        command += ";";
                }

                MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                DataSet dataset = new DataSet("Results");
                dadpt.Fill(dataset, "Bfly Features");
                DataTable table = dataset.Tables["Bfly Features"];
                ordered_res = table.Clone();
                con.Close();

                for (int i = 0; i < pids.Length; i++)
                {
                    foreach (DataRow j in table.Rows)
                    {
                        if (pids[i] == (int)j["pid"])
                        {
                            ordered_res.Rows.Add(j.ItemArray);
                            break;
                        }
                    }
                }
            }
            catch( MySqlException e)
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "Model::PF_only_knn");
                Environment.Exit(2);
            }
            return ordered_res;
        }

        public DataTable PF_and_VF_knn(BflyObject q)
        {
            knn_list.reset();

            double temp_distance;
            int query_pid = q.pid;
            bool q_has_VFs = q.has_VFs();

            for (int i = 0; i < bfly_features.GetLength(0); i++)
            {
                if (bfly_features[i].pid == query_pid)
                    continue;

                temp_distance = get_PF_distance(q, i);

                if (q_has_VFs && bfly_features[i].has_VFs())
                    temp_distance *= ( 1 - get_VF_cosine_theta(q, i) );

                bool inserted = knn_list.conditional_insert(bfly_features[i].pid, temp_distance);
            }
            DataTable ordered_res = new DataTable();
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
                int[] pids = knn_list.get_pids();
                string command = String.Format("SELECT * FROM {0} WHERE ", Constants.BFLY_OBJECTS_TABLE);
                for (int i = 0; i < Constants.NUM_D_RESULTS; i++)
                {
                    command += String.Format("pid = {0}", pids[i]);
                    if (i < Constants.NUM_D_RESULTS - 1)
                        command += " || ";
                    else
                        command += ";";
                }
                MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                DataSet dataset = new DataSet("Results");
                dadpt.Fill(dataset, "Bfly Objects");
                DataTable table = dataset.Tables["Bfly Objects"];
                ordered_res = table.Clone();
                con.Close();
            
                for (int i = 0; i < pids.Length; i++)
                {
                    foreach (DataRow j in table.Rows)
                    {
                        if ( pids[i] == (int)j["pid"] )
                        {
                            ordered_res.Rows.Add(j.ItemArray);
                            break;
                        }
                    }
                }
                
            }
            catch (MySqlException e)
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "Model::PF_and_VF_knn");
                Environment.Exit(2);
            }
            return ordered_res;
        }

        public void update_q_mu_and_precision(BflyObject q, D_small_display_object[] ds)
        {
            double new_mu = 0;
            float new_prec = 0;
            int num_pos = 0;
            float average_precision = 0;//////////////////////
            float temp = 0;
            int i, j;  //////////////////////////////
            for (i = 0; i < ds.Length; i++)
            {//////////////////////////////
                if (ds[i].rf_pos)
                {
                    num_pos++;
                }
                j = i + 1;
                temp += (float)num_pos / j;//////////////////////////
            }
            new_prec = ((float)num_pos) / ((float)ds.Length);
            if (num_pos > 0)
                average_precision = (float)temp / ((float)ds.Length);//////////////////////////
            else
                average_precision = 0;
            new_mu = q.mu * (1 + new_prec - q.precision_pre);
            if (new_mu > 1)
                new_mu = 1;
            else if (new_mu <= 0)
                new_mu = Constants.MINIMUM_ETA;

            q.mu = (float)new_mu;
            q.precision_pre = new_prec;
            q.average_pre = average_precision;//////////////////////////

            try
            {
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                   Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                MySqlConnection con = new MySqlConnection(connStr);
                con.Open();
                if (con.State == ConnectionState.Closed)
                {
                    MessageBox.Show("\n\nFAILED TO CONNECT TO DB. EXITING. (press return)\n", "Model::update_q_mu_and_precision");
                    Environment.Exit(1);
                }
                string command1 = String.Format("UPDATE {0} SET eta = {1}, average_pre = {2}, precision_pre = {3} WHERE pid = {4};", Constants.BFLY_OBJECTS_TABLE, q.mu, q.average_pre, q.precision_pre, q.pid);
                MySqlCommand cmd1 = new MySqlCommand(command1, con);
                cmd1.ExecuteNonQuery();
                string command2 = String.Format("UPDATE {0} SET eta = {1}, average_pre = {2}, precision_pre = {3} WHERE pid = {4};", Constants.BFLY_FEATURES_TABLE, q.mu, q.average_pre, q.precision_pre, q.pid);
                MySqlCommand cmd2 = new MySqlCommand(command2, con);
                cmd2.ExecuteNonQuery();
                con.Close();
            }
            catch(MySqlException e)
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "Model::update_q_mu_and_precision");
            }
        }

        public void update_D_VFs(BflyObject q, D_small_display_object[] ds)
        {
            List<D_small_display_object> R_prime = new List<D_small_display_object>();
            List<D_small_display_object> R_double_prime = new List<D_small_display_object>();
            List<D_small_display_object> R = new List<D_small_display_object>();

            for (int i = 0; i < ds.Length; i++)
            {
                if (ds[i].rf_pos)
                {
                    R.Add(ds[i]);
                    if (ds[i].has_VFs())
                        R_double_prime.Add(ds[i]);
                    else
                        R_prime.Add(ds[i]);
                }
            }
            if (R.Count == 0)
                return;

            if( R_double_prime.Count == 0 )
            {
		        foreach( D_small_display_object d in R_prime ) 
                {
			       d.VF_cs[0] = this.vf_counter;
			       d.VF_es[0] = 1;
			       this.bfly_features[d.pid - 1].VF_cs[0] = this.vf_counter;
			       this.bfly_features[d.pid - 1].VF_es[0] = 1;
			        //d->has_VFs = true;
		        }
		        new_vf = true;//////////////////////////////////////////////////////////////////////////////
		        store_VF_changes_in_DB( R_prime );
		        return;
	        }

            ArrayList temp_VFs = new ArrayList();
            foreach (D_small_display_object dd in R_double_prime)
            {
                int j = 0;
                while (j < Constants.NUM_VFS_PER_IMAGE && dd.VF_cs[j] > 0 && dd.VF_es[j] > 0)
                {
                    temp_VFs.Add(new VF(dd.VF_cs[j], dd.VF_es[j]));
                    j++;
                }
            }
            
            VF[] new_VFs = normalize_VF_vector(temp_VFs);

	        foreach( D_small_display_object d in R_prime ) {
		        for( int i = 0; i < new_VFs.Length; i++ ) {
			        d.VF_cs[i] = new_VFs[i].c;
			        d.VF_es[i] = new_VFs[i].e;
			        this.bfly_features[d.pid - 1].VF_cs[i] = new_VFs[i].c;
			        this.bfly_features[d.pid - 1].VF_es[i] = new_VFs[i].e;
		        }
		        //d->has_VFs = true;
	        }

            double mu = q.mu;
	        foreach( D_small_display_object d in R ) {
		        temp_VFs = new ArrayList();
		        for( int i = 0; i < Constants.NUM_VFS_PER_IMAGE; i++ ) {
			        if( d.VF_cs[i] <= 0 )
				        break;
			        temp_VFs.Add( new VF(d.VF_cs[i], d.VF_es[i] * mu) );
		        }
		        temp_VFs.Add( new VF( this.vf_counter, 1 - mu ) );
		        new_VFs = normalize_VF_vector( temp_VFs );
		        for( int i = 0; i < Constants.NUM_VFS_PER_IMAGE; i++ ) {
			        if( i < new_VFs.Length ) {
				        d.VF_cs[i] = new_VFs[i].c;
				        d.VF_es[i] = new_VFs[i].e;
				        this.bfly_features[d.pid - 1].VF_cs[i] = new_VFs[i].c;
				        this.bfly_features[d.pid - 1].VF_es[i] = new_VFs[i].e;
			        }
			        else {
				        d.VF_cs[i] = 0;
				        d.VF_es[i] = 0;
				        this.bfly_features[d.pid - 1].VF_cs[i] = 0;
				        this.bfly_features[d.pid - 1].VF_es[i] = 0;
			        }
		        }
	        }
            new_vf = true;
            //store the changes in the database
            store_VF_changes_in_DB(R_prime);
            store_VF_changes_in_DB(R_double_prime);

            return;
        }

        public void update_q_VFs(BflyObject q, D_small_display_object[] ds)
        {
            ArrayList temp_VFs = new ArrayList();
            for (int i = 0; i < ds.Length; i++)
            {
		        if( ds[i].rf_pos ) {
			        for( int j = 0; j < Constants.NUM_VFS_PER_IMAGE; j++ ) {
				        if( ds[i].VF_cs[j] <= 0 )
					        break;
				        temp_VFs.Add( new VF(ds[i].VF_cs[j], ds[i].VF_es[j]) );
			        }
		        }
            }

            VF[] new_VFs = normalize_VF_vector(temp_VFs);
            //store them back in the query
            for (int i = 0; i < Constants.NUM_VFS_PER_IMAGE; i++)
            {
                if (i < new_VFs.Length)
                {
                    q.VF_cs[i] = new_VFs[i].c;
                    q.VF_es[i] = new_VFs[i].e;
                }
                else
                {
                    q.VF_cs[i] = 0;
                    q.VF_es[i] = 0;
                }
            }
            store_Q_VF_change_in_DB(q);

            return;
        }

        public int increment_VF_counter()
        {
            if (new_vf == true)
            {
                vf_counter++;
                new_vf = false;
            }

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
                string command = String.Format("UPDATE {0} SET vf_counter = {1};", Constants.VF_COUNTER_TABLE, vf_counter);
                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.ExecuteNonQuery();
                con.Close();

            }
            catch( MySqlException e )
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "Model::increment_VF_counter");
            }
            return vf_counter;
        }

        public int return_vf()
        {
            int counter = this.vf_counter - 1;
            return counter;
        }

        private double get_VF_cosine_theta(BflyObject q, int index)
        {
            if (!q.has_VFs() || !bfly_features[index].has_VFs())
                return 0;

            BflyFeatures d = bfly_features[index];

            double cos_theta = 0;
            int Q_index = 0;
            int D_index = 0;
            int Q_c;
            int D_c;

            while (true)
            {
                if (Q_index >= Constants.NUM_VFS_PER_IMAGE || D_index >= Constants.NUM_VFS_PER_IMAGE)
                {
                    if (cos_theta > 1)
                        return 1;
                    else
                        return cos_theta;
                }

                Q_c = q.VF_cs[Q_index];
                D_c = d.VF_cs[D_index];

                if (Q_c == 0 || D_c == 0)
                    return cos_theta;
                
                if (Q_c == D_c)
                {
                    cos_theta += q.VF_es[Q_index] * d.VF_es[D_index];
                    Q_index++;
                    D_index++;
                }
                else if (Q_c > D_c)
                    D_index++;
                else
                    Q_index++;

            }
        }

        private double get_PF_distance(BflyObject q, int index)
        {
            double distance = 0;
            double temp1;
            double temp2;

            bool list_full = (knn_list.get_length() == Constants.NUM_D_RESULTS);
            double max_distance = Constants.MAX_PF_DISTANCE;

            if (list_full)
                max_distance = knn_list.get_longest_distance();

            for (int i = 0; i < Constants.NUM_PFS; i++)
            {
                if (list_full && distance >= max_distance)
                    return distance;

                temp1 = q.PFs[i];
                temp2 = bfly_features[index].PFs[i];
                distance += (temp1 - temp2) * (temp1 - temp2);
                if (distance < 0)
                    distance = Constants.MAX_PF_DISTANCE;
            }
            return distance;
        }



        private BflyFeatures[] bfly_features;
        private MyList knn_list;
        private int vf_counter;
        private bool new_vf;
               
        private void load_bfly_features()
        {
            try
            {
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                    Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                try
                {
                    MySqlConnection con = new MySqlConnection(connStr);
                    con.Open();
                    string command = String.Format("SELECT * FROM {0};", Constants.BFLY_FEATURES_TABLE);
                    MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                    DataSet dataset = new DataSet("Results");
                    dadpt.Fill(dataset, "Bfly Features");
                    DataTable table = dataset.Tables["Bfly Features"];

                    bfly_features = new BflyFeatures[Constants.DB_SIZE];
                    
                    for (int i = 0; i < Constants.DB_SIZE; i++)
                    {
                        bfly_features[i] = new BflyFeatures();
                    }
                    //int t = (int)table.Rows[0][45];
                    int row_counter = 0;
                    //int num = table.Columns.Count;
                    try
                    {
                        for (int j = 0; j < table.Rows.Count; j++)
                        {
                            bfly_features[row_counter].pid = (int)table.Rows[j][0];
                            for (int i = 0; i < Constants.NUM_PFS; i++)
                            {
                                double m = (double)table.Rows[j][i + Constants.DB_INDEX_OF_FIRST_PF];
                                bfly_features[row_counter].PFs[i] = (double)table.Rows[j][i + Constants.DB_INDEX_OF_FIRST_PF];
                            }

                            for (int db_i = Constants.DB_INDEX_OF_FIRST_VF, local_i = 0; local_i < Constants.NUM_VFS_PER_IMAGE; db_i += 2, local_i++)
                            {
                                bfly_features[row_counter].VF_cs[local_i] = int.Parse(table.Rows[j][db_i].ToString());
                                bfly_features[row_counter].VF_es[local_i] = double.Parse(table.Rows[j][db_i + 1].ToString());
                            }
                            row_counter++;
                        }

                    }
                    catch
                    {
                        //for some reason this pulls an empty row once all the valid rows have been processed
                        //this can be ignored
                        StringBuilder sb = new StringBuilder("row_counter = ");
                        sb.Append(row_counter);
                        MessageBox.Show(sb.ToString(), "Model::load_bfly_features()" );
                    }
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
                MessageBox.Show(e_string, "Model::load_bfly_features()");
            }
        }
        
        private void load_vf_counter()
        {
            try
            {
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                    Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                try
                {
                    MySqlConnection con = new MySqlConnection(connStr);
                    con.Open();
                    string command = String.Format("SELECT * FROM {0};", Constants.VF_COUNTER_TABLE);
                    MySqlDataAdapter dadpt = new MySqlDataAdapter(command, con);
                    DataSet dataset = new DataSet("Results");
                    dadpt.Fill(dataset, "VF Counter");
                    DataTable table = dataset.Tables["VF Counter"];

                    this.vf_counter = (int)table.Rows[0][0];
                    con.Close();
                    return;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error connecting to the server: " + "Model::load_vf_counter()" + ex.Message);
                    return;
                }
            }
            catch (MySqlException e)
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "Model::load_vf_counter()");
            }
        }

        private void standard_qvm(BflyObject q, D_small_display_object[] Ds)
        {
            int num_pos = 0;
	        int num_neg = 0;
            //for (int i = 0; i < Constants.NUM_D_RESULTS; i++)
            for (int i = 0; i < Ds.Length; i++)/////////////////////////////
            {
                if (Ds[i].rf_pos)
                    num_pos++;
                else if (Ds[i].rf_neg)
                    num_neg++;
            }

            double orig_pf;
            double plus_val;
            double minus_val;

            for (int i = 0; i < Constants.NUM_PFS; i++)
            {
                orig_pf = q.PFs[i];
                q.PFs[i] = Constants.ALPHA * q.PFs[i];
               // for (int j = 0; j < Constants.NUM_D_RESULTS; j++)
                for (int j = 0; j < Ds.Length; j++)/////////////////////////////////
                {
                    if( Ds[j].rf_pos )
                    {
                        plus_val = Constants.BETA * Ds[j].PFs[i] / num_pos;
                        q.PFs[i] += plus_val;
                    }
                    else if (Ds[j].rf_neg)
                    {
                        minus_val = Constants.GAMMA * Ds[j].PFs[i] / num_neg;
                        q.PFs[i] -= minus_val;
                    }
                }
            }

            double magnitude = 0;
            for (int i = 0; i < Constants.NUM_PFS; i++)
            {
                magnitude += q.PFs[i] * q.PFs[i];
            }
            magnitude = Math.Sqrt(magnitude);
            for (int i = 0; i < Constants.NUM_PFS; i++)
            {
                q.PFs[i] /= magnitude;
            }
            return;
        }

        private void store_VF_changes_in_DB( List<D_small_display_object> ds )
        {
            try
            {
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                   Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                MySqlConnection con = new MySqlConnection(connStr);
                con.Open();
                if (con.State == ConnectionState.Closed)
                {
                    MessageBox.Show("\n\nFAILED TO CONNECT TO DB. EXITING. (press return)\n", "Model::Store_VF_changes_in_DB");
                    Environment.Exit(1);
                }
                foreach (D_small_display_object d in ds)
                {
                    int counter = 0;

                    for (int j = 0; j < Constants.NUM_VFS_PER_IMAGE; j++)
                    {
                        if ((d.VF_cs[j] != 0) && (d.VF_es[j] != 0))
                            counter++;
                    }
                    d.local_vf_counter = counter;
                    string command = String.Format("UPDATE {0} SET number_of_vf = {1},", Constants.BFLY_FEATURES_TABLE, counter);
                    for (int i = 0; i < Constants.NUM_VFS_PER_IMAGE; i++)
                    {
                        command += String.Format(" vf_c_{0} = {1}, vf_e_{2} = {3}", i + 1, d.VF_cs[i], i + 1, d.VF_es[i]);
                        if (i == Constants.NUM_VFS_PER_IMAGE - 1)
                            command += String.Format(" WHERE pid = {0};", d.pid);
                        else
                            command += String.Format(",");
                    }
                    MySqlCommand cmd = new MySqlCommand(command, con);
                    int r = cmd.ExecuteNonQuery();
                    if (r <= 0)
                    {
                        MessageBox.Show("Failed to connect to update VF data.", "Model::update_D_VFs");
                    }
                    cmd.Dispose();
                    string command1 = String.Format("UPDATE {0} SET number_of_vf = {1} WHERE pid = {2};", Constants.BFLY_OBJECTS_TABLE, counter, d.pid);
                    MySqlCommand cmd1 = new MySqlCommand(command1, con);
                    int rr = cmd1.ExecuteNonQuery();
                    if (rr <= 0)
                    {
                        MessageBox.Show("Failed to connect to update VF data.", "Model::update_D_VFs");
                    }
                    cmd1.Dispose();
                }
                con.Close();
            }
            catch( MySqlException e )
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "Model::store_VF_changes_in_DB");
                Environment.Exit(2);
            }
        }

        private VF[] normalize_VF_vector(ArrayList temp_VFs)
        {
	        //if there are no VFs, return an empty array
	        if( temp_VFs.Count == 0 ) {
		        VF[] a = new VF[1];
		        a[0] = new VF(0,0);
		        return a;
	        }//

            //add together the support values of the same concept
            temp_VFs.Sort();

            VF[] VF_arr = (VF[])(temp_VFs.ToArray(typeof(VF)));
            int small_i = 0;
            int big_i = 0;
            int temp_c;
            int real_length = VF_arr.Length;
            double magnitude = 0;
            while (small_i < VF_arr.Length)
            {
                temp_c = VF_arr[small_i].c;
                while (big_i + 1 < VF_arr.Length && VF_arr[big_i + 1].c == temp_c)
                {
                    big_i++;
                    VF_arr[small_i].e += VF_arr[big_i].e;
                    VF_arr[big_i].c = -1; //make this "invalidated", because it's been added to the other one
                    real_length--;
                }
                magnitude += (VF_arr[small_i].e) * (VF_arr[small_i].e);
                big_i++;
                small_i = big_i;
            }
	        //handle the situation where an image has more VFs than allowed
	        //by removing the concepts with the smallest support values
	        if( real_length > Constants.NUM_VFS_PER_IMAGE ) {
        		
		        //MessageBox::Show( "An image wants more VFs than it can have. No worries, it's being dealt with. Just thought you might like to know.",
			                      //"Model::normalize_VF_vector", MessageBoxButtons::OK, MessageBoxIcon::Information );
		        //char *a = "Model::normalize_VF_vector";
		        //LPCWSTR WName = reinterpret_cast<LPCWSTR>("Model::normalize_VF_vector");
		        //HWND hwnd = FindWindow(NULL, (LPCWSTR)a); 
		        //if( hwnd ) {
		        //	SendMessage(hwnd,WM_CLOSE,0,0); 
		        //}
		        int num_to_remove = real_length - Constants.NUM_VFS_PER_IMAGE;
		        int orig_real_length = real_length;
		        //VF_arr->Sort();
		        MyList l = new MyList( num_to_remove );
		        for( int i = 0; i < VF_arr.Length; i++ ) {
			        if( VF_arr[i].c < 0 )
				        continue;
			        l.conditional_insert( VF_arr[i].c, VF_arr[i].e );
		        }
		        int[] remove_these = l.get_pids();
		        for( int i = 0; i < VF_arr.Length; i++ ) {
			        for( int j = 0; j < remove_these.Length; j++ ) {
				        if( VF_arr[i].c == remove_these[j] ) {
					        VF_arr[i].c = -1;
					        real_length--;
				        }
			        }
		        }
	        }
	        //and normalize the values, too
	        magnitude = Math.Sqrt(magnitude);
	        VF[] VF_arr2 = new VF[real_length];
	        int arr2_index = 0;
	        for( int i = 0; i < VF_arr.Length; i++ ) {
		        if( VF_arr[i].c > 0 ) {
			        VF_arr2[arr2_index] = new VF( VF_arr[i].c, Math.Abs(VF_arr[i].e / magnitude) );
			        arr2_index++;
		        }
	        }

	        return VF_arr2;
        }

        private void store_Q_VF_change_in_DB(BflyObject q)
        {
            try
            {
                string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                   Constants.BFLY_DB_SERVER, Constants.BFLY_DB_USER, Constants.BFLY_DB_PASS, Constants.BFLY_DB);

                MySqlConnection con = new MySqlConnection(connStr);
                con.Open();
                if (con.State == ConnectionState.Closed)
                {
                    MessageBox.Show("\n\nFAILED TO CONNECT TO DB. EXITING. (press return)\n", "Model::Store_Q_VF_change_in_DB");
                    Environment.Exit(1);
                }

                int counter = 0;
                for (int j = 0; j < Constants.NUM_VFS_PER_IMAGE; j++)
                {
                    if ((q.VF_cs[j] != 0) && (q.VF_es[j] != 0))
                        counter++;
                }
                q.local_vf_counter = counter;
                
                string command = String.Format("UPDATE {0} SET number_of_vf = {1},", Constants.BFLY_FEATURES_TABLE, counter);
                for (int i = 0; i < Constants.NUM_VFS_PER_IMAGE; i++)
                {
                    command += String.Format(" vf_c_{0} = {1}, vf_e_{2} = {3}", i + 1, q.VF_cs[i], i + 1, q.VF_es[i]);
                    if (i == Constants.NUM_VFS_PER_IMAGE - 1)
                        command += String.Format(" WHERE pid = {0};", q.pid);
                    else
                        command += String.Format(",");
                }
                MySqlCommand cmd = new MySqlCommand(command, con);
                int r = cmd.ExecuteNonQuery();
                if (r <= 0)
                {
                    MessageBox.Show("Failed to connect to update VF data.", "Model::update_Q_VFs");
                }
                cmd.Dispose();
                string command1 = String.Format("UPDATE {0} SET number_of_vf = {1} WHERE pid = {2};", Constants.BFLY_OBJECTS_TABLE, counter, q.pid);
                MySqlCommand cmd1 = new MySqlCommand(command1, con);
                int rr = cmd1.ExecuteNonQuery();
                if (rr <= 0)
                {
                    MessageBox.Show("Failed to connect to update VF data.", "Model::update_QVFs");
                }
                cmd1.Dispose();

            }
            catch( MySqlException e ) 
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string, "Model::store_Q_VF_changes_in_DB");
                Environment.Exit(2);
            }

            return;
        }
    }
}
