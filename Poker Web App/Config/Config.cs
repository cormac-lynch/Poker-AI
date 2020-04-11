namespace Poker_Web_App.Config
{
    /// <summary>
    /// Config class for storing configuration variables
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// MongoDB database connection string
        /// </summary>
        public static string dbConnectionString = "mongodb://localhost";

        /// <summary>
        /// Name of the particular database
        /// </summary>
        public static string dbName = "poker_ai";

        /// <summary>
        /// Name of the collection
        /// </summary>
        public static string dbCollectionName = "hand_histories";

        /// <summary>
        /// Path of the folder containing XML hand history data
        /// </summary>
        public static string handsFolderPath = "C:\\Users\\User\\Documents\\hands";
    }
}
