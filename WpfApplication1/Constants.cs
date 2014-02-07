using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public static class Constants
    {
        public static long MAX_VF_NUMBER = 100000;
        public static int DB_SIZE = 10038;
        public static string LARGE_IMG_FOLDER = "C:\\ucr_db\\ucr_img_db\\";
        public static string SMALL_IMG_FOLDER = "C:\\ucr_db\\ucr_img_db\\low\\";

        public static int NUM_D_RESULTS = 60; //CHANGE TO 20 FOR PERFORMANCE TESTING
        public static int NUM_Q_POSSIBILITIES = 100;

        //used as the maximum possible PF distance between two images
        public static int MAX_PF_DISTANCE = 9999;

        public static string BFLY_DB = "ucr_img_db";
        public static string BFLY_DB_SERVER = "localhost";
        public static string BFLY_DB_USER = "root";
        public static string BFLY_DB_PASS = "root";

        public static string BFLY_OBJECTS_TABLE = "ucr_objects";//
        public static string BFLY_FEATURES_TABLE = "ucr_features";
        public static string VF_COUNTER_TABLE = "vf_counter";

        public static int NUM_PFS = 22;
        public static int NUM_VFS_PER_IMAGE = 250;
        public static int DB_INDEX_OF_FIRST_PF = 1;
        public static int DB_INDEX_OF_FIRST_VF = 25;

        public static double MINIMUM_ETA = 0.0001;

        //Query Vector Modification (QVM) constants
        //values used in the paper: alpha: 0.1, beta: 0.8, gamma: 0.1
        public static double ALPHA = 0.1;
        public static double BETA = 0.8;
        public static double GAMMA = 0.1;
    }
}
