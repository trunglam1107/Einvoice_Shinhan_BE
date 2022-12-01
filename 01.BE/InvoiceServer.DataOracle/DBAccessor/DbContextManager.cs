using System;

namespace InvoiceServer.Data.DBAccessor
{
    public static class DbContextManager
    {
        /// <summary>
        /// The DbContext is specified in connection string at name = "DataClassesDataContext"
        /// </summary>
        public static IDbContext GetContext()
        {
            return new DataClassesDataContext();
        }

        /// <summary>
        /// Constructs a new context instance using the given string as the name or connection
        /// string for the database to which a connection will be made.  See the class</summary>
        /// remarks for how this is used to create a connection.
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"><c>nameOrConnectionString</c> is null or empty.</exception>
        public static IDbContext GetContext(string nameOrConnectionString)
        {
            if (string.IsNullOrWhiteSpace(nameOrConnectionString))
                throw new ArgumentException("Connection string is not specified.", "nameOrConnectionString");

            return new DataClassesDataContext(nameOrConnectionString);
        }
    }
}
