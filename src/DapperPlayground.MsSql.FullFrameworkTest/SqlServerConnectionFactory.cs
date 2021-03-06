﻿namespace DapperPlayground.MsSql
{
    using System.Data;
    using System.Data.SqlClient;

    public static class SqlServerConnectionFactory
    {
        public static IDbConnection OpenNew()
        {
            const string connectionString = "Data Source=localhost;Initial Catalog=Northwind;Integrated Security=true;";
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}