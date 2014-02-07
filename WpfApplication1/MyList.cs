using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public partial class MyList
    {
        public MyList()
        {
            max_length = Constants.NUM_D_RESULTS;
            pids = new List<int>();
            distances = new List<double>();

            for( int i = 0; i < Constants.NUM_D_RESULTS; i++ ) {
		        pids.Add(-1);
		        distances.Add(-1);
	        }
            
            longest_distance = length = 0;
        }
        public MyList(int n)
        {
            pids = new List<int>();////////////
	        distances = new List<double>();//////////////
	        max_length = n;
	        for( int i = 0; i < n; i++ ) {/////////////
		        pids.Add(-1);///////////////
		        distances.Add(-1);////////////////
	        }///////////////////

	        longest_distance = length = 0;/////////////////
        }

        public double get_longest_distance()
        {
            return longest_distance;
        }

        public int get_length()
        {
            return length;
        }

        public bool conditional_insert(int pid, double distance)
        {
            if ((distance >= longest_distance && length == max_length) || distance < 0)
                return false;

            int i = 0;

            while (distance > distances[i] && distances[i] >= 0 && i < max_length)
            {
                i++;
            }

            if (length == max_length)
            {
                pids.RemoveAt(max_length - 1);
                distances.RemoveAt(max_length - 1);
            }
            else
            {
                length++;
            }

            pids.Insert(i, pid);
            distances.Insert(i, distance);

            longest_distance = distances[length - 1];

            return true;
        }

        public void reset()
        {
            longest_distance = length = 0;
            for (int i = 0; i < max_length; i++)
            {
                pids[i] = -1;
                distances[i] = -1;
            }
        }

        public int[] get_pids()
        {
            int[] r = new int[length];

	        for( int i = 0; i < length; i++ ) {
		        r[i] = pids[i];
	        }

	        return r;
        }

        private List<int> pids;
        private List<double> distances;
        private double longest_distance;
        private int length;
        private int max_length;
    }
}
