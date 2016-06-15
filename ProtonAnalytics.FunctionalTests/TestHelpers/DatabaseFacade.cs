using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtonAnalytics.FunctionalTests.TestHelpers
{
    internal static class DatabaseFacade
    {
        public static void ExecuteQuery(string query, object parameters = null)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"];

            using (var connection = new SqlConnection(connectionString.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = query;

                if (parameters != null)
                {
                    var type = parameters.GetType();
                    foreach (var property in type.GetProperties())
                    {
                        var name = property.Name;
                        var value = property.GetValue(parameters);
                        command.Parameters.Add(new SqlParameter(name, value));
                    }
                }

                command.ExecuteNonQuery();
                connection.Close();
            }
        }
    }

}
