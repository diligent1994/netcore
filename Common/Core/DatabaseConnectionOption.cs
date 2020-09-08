namespace Common.Core
{
    public class DatabaseConnectionOption
    {
        /// <summary>
        /// Gets the type definition of the appserver.
        /// </summary>
        /// <value>
        /// The type of the server.
        /// </value>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the connection string for the database.
        /// </summary>
        public string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the database type sring format.
        /// </summary>
        public string DatabaseTypeLiterial
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the database type sring format.
        /// </summary>
        public int MaxRowCount
        {
            get;
            set;
        }

    }
}
