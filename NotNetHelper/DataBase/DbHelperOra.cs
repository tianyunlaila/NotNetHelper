using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotNetHelper
{
    /// <summary>
    /// Class OracleHelper.
    /// </summary>
    public abstract class DbHelperOra
    {
        //public static string ConnectionString;

        #region 方法

        public static string GetConnectString(string oracleService, string oracleUser, string oraclePassword)
        {
            string strConnString = @"data source=" + oracleService + @";user id=" + oracleUser + @";password=" + oraclePassword;
            return strConnString;
        }
        /// <summary>
        /// 验证连接Oracle数据库
        /// </summary>
        /// <param name="oracleService">Oracle数据库的服务名称</param>
        /// <param name="oracleUser">Oracle数据库的用户名</param>
        /// <param name="oraclePassword">Oracle数据库的密码</param>
        /// <returns>如果为空，则验证成功，否则返回错误信息</returns>
        public static string ValidateOraCnn(string oracleService, string oracleUser, string oraclePassword)
        {
            string strConnString = @"data source=" + oracleService + @";user id=" + oracleUser + @";password=" + oraclePassword;
            OracleConnection connection = new OracleConnection(strConnString);
            try
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                    return "";
                }
                else
                {
                    connection.Close();
                    connection.Dispose();
                    return "";
                }
            }

            catch (Exception e)
            {
                connection.Close();
                connection.Dispose();
                return e.Message;
            }
        }


        /// <summary>
        /// 执行SQL的查询、插入,更新和删除语句，在不使用 DataSet 的情况下更改数据库中的数据
        /// </summary>
        /// <param name="commandText">所要执行的SQL语句</param>
        /// <returns>返回影响结果集的行数，对于查询返回-1</returns>
        public static void ExecuteNonQuery(string ConnectionString, string commandText, out Exception e, params OracleParameter[] cmdParms)
        {
            e = null;
            using (OracleConnection connection = new OracleConnection(ConnectionString))
            {
                OracleCommand command = new OracleCommand(commandText, connection);
                if (cmdParms != null)
                {
                    foreach (OracleParameter parameter in cmdParms)
                    {
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
                try
                {
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    command.Connection.Close();
                    e = ex;
                    connection.Dispose();
                }
                command.Connection.Close();
                command.Dispose();
            }
        }
        /// <summary>
        /// 执行SQL的查询语句或存储过程，在不使用 DataSet 的情况下更改数据库中的数据
        /// </summary>
        /// <param name="commandText">所要执行的SQL语句或存储过程</param>
        /// <returns>返回的结果集中第一行的第一列</returns>
        public static object ExecuteScalar(string ConnectionString, string commandText, out Exception e)
        {
            e = null;
            object obj = null;
            OracleConnection connection = new OracleConnection(ConnectionString);
            OracleCommand command = new OracleCommand(commandText, connection);
            try
            {
                command.Connection.Open();
                obj = command.ExecuteScalar();
                if (obj == System.DBNull.Value)
                    obj = null;
            }
            catch (Exception ex)
            {
                command.Connection.Close();
                e = ex;
                connection.Dispose();
                return null;
            }
            command.Connection.Close();
            command.Dispose();
            connection.Dispose();
            return obj;
        }


        /// <summary>
        /// 执行SQL的查询语句或存储过程
        /// </summary>
        /// <param name="commandText">所要执行的SQL语句或存储过程，将结果集中的记录和架构填充到DataTable</param>
        /// <returns>返回所影响的行数</returns>
        public static int FillTable(string ConnectionString, DataTable table, string commandText, out Exception e)
        {
            e = null;
            int i = 0;
            OracleConnection connection = new OracleConnection(ConnectionString);
            OracleDataAdapter adapter = new OracleDataAdapter(commandText, connection);
            try
            {
                i = adapter.Fill(table);
            }
            catch (Exception ex)
            {
                i = -1;
                e = ex;
            }
            adapter.Dispose();
            connection.Dispose();
            return i;
        }


        /// <summary>
        /// 执行SQL的查询语句或存储过程
        /// </summary>
        /// <param name="commandText">所要执行的SQL语句或存储过程，将结果集中的记录填充到DataRow</param>
        public void FillRow(string ConnectionString, ref DataRow dr, string commandText, out Exception e)
        {
            dr = null;
            DataTable dt = null;
            FillTable(ConnectionString, dt, commandText, out e);
            if (dt.Rows.Count > 0)
                dr = dt.Rows[0];
        }

        /// <summary>
        /// 填充SQL或存储过程的表构架信息

        /// </summary>
        /// <param name="commandText">将结果集中架构填充到DataTable</param>
        public static void FillSchema(string ConnectionString, DataTable table, string commandText, out Exception e)
        {
            e = null;
            OracleConnection connection = new OracleConnection(ConnectionString);
            OracleDataAdapter adapter = new OracleDataAdapter(commandText, connection);
            try
            {
                adapter.FillSchema(table, SchemaType.Source);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            adapter.Dispose();
            connection.Dispose();
        }

        /// <summary>
        /// 执行sql语句，并统计总记录数
        /// </summary>
        /// <param name="commandText">Sql查询语句</param>
        /// <param name="e">错误输出</param>
        /// <returns>返回查找到的数据行数</returns>
        public static int GetCount(string ConnectionString, string commandText, out Exception e)
        {
            e = null;
            int RecordCount = 0;
            OracleConnection connection = new OracleConnection(ConnectionString);
            connection.Open();
            OracleCommand command = new OracleCommand(commandText, connection);
            try
            {
                OracleDataReader countreader = command.ExecuteReader();
                if (countreader.Read())
                    RecordCount = Int32.Parse(countreader[0].ToString());
                return RecordCount;
            }
            catch (Exception err)
            {
                e = err;
                return 0;
            }
        }

        /// <summary>
        /// Updates the table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="e">The e.</param>
        /// <returns>System.Int32.</returns>
        public static int UpdateTable(DataTable table, string ConnectionString, string commandText, out Exception e)
        {
            int i = 0;
            e = null;
            OracleConnection connection = new OracleConnection(ConnectionString);

            OracleDataAdapter adapter = new OracleDataAdapter(commandText, connection);
            adapter.MissingSchemaAction = MissingSchemaAction.Ignore;
            OracleCommandBuilder ocb = new OracleCommandBuilder(adapter);
            //DataSet pDataSet = new DataSet();
            //pDataSet.Tables.Add(table);
            try
            {
                i = adapter.Update(table);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            adapter.Dispose();
            connection.Dispose();
            return i;
        }

        /// <summary>  
        /// Execute a select query that will return a result set  
        /// </summary>  
        /// <param name="connString">Connection string</param>  
        //// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>  
        /// <param name="commandText">the stored procedure name or PL/SQL command</param>  
        /// <param name="commandParameters">an array of OracleParamters used to execute the command</param>  
        /// <returns></returns>  
        public static OracleDataReader ExecuteReader(string ConnectionString, string commandText, params OracleParameter[] cmdParms)
        {
            OracleConnection connection = new OracleConnection(ConnectionString);
            OracleCommand command = new OracleCommand(commandText, connection);
            if (cmdParms != null)
            {
                foreach (OracleParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }
            try
            {
                command.Connection.Open();
                OracleDataReader rdr = command.ExecuteReader(CommandBehavior.CloseConnection);
                command.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                connection.Close();
                return null;
            }
        }
        #endregion

    }
}
