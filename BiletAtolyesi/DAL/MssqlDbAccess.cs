using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace ProtectionConnLib_4.DataAccessModel
{
    public class MssqlDbAccess : IDisposable
    {
        public string ConnectionString
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            }
        }
        private volatile SqlConnection _conn = null;
        private readonly object _lock = new object();

        private SqlConnection CheckConnection()
        {
            if (_conn == null || _conn.ConnectionString == "")
            {
                lock (_lock)
                {
                    if (_conn == null || _conn.ConnectionString == "")
                    {
                        _conn = new SqlConnection(ConnectionString);
                        _conn.Open();
                        return _conn;
                    }
                }
            }
            if (_conn.State != ConnectionState.Open)
                _conn.Open();

            return _conn;
        }

        private void CloseConnection(IDbConnection conn)
        {
            conn.Close();
        }

        private SqlCommand CreateCommand(SqlConnection conn)
        {
            return conn.CreateCommand();
        }

        public List<T> QueryList<T>(string procName, List<SqlParameter> parameter) where T : new()
        {
            using (var conn = CheckConnection())
            {

                var comm = CreateCommand(conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.CommandText = string.Format(procName);
                comm.CommandTimeout = 1000000;
                foreach (var o in parameter)
                {
                    comm.Parameters.Add(o);
                }
                var list = MapData<T>(comm.ExecuteReader());
                CloseConnection(conn);
                return list;
            }
        }
        public T QueryItem<T>(string procName, List<SqlParameter> parameter) where T : new()
        {
            var item = new T();
            using (var conn = CheckConnection())
            {
                var comm = CreateCommand(conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.CommandText = string.Format(procName);
                foreach (var o in parameter)
                {
                    comm.Parameters.Add(o.ParameterName, o.Value);
                }
                var list = MapData<T>(comm.ExecuteReader());
                if (list.Count > 0) item = list[0];
                CloseConnection(conn);
                return item;
            }
        }
        public void ExecuteNonQuery(string procName, List<SqlParameter> parameters)
        {
            using (var conn = CheckConnection())
            {
                var comm = CreateCommand(conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.CommandText = string.Format(procName);
                foreach (var o in parameters)
                {
                    comm.Parameters.Add(o.ParameterName, o.Value);
                }
                comm.ExecuteNonQuery();
                CloseConnection(conn);
            }
        }
        public object ExecuteScalar(string procName, List<SqlParameter> parameters)
        {
            using (var conn = CheckConnection())
            {
                var comm = CreateCommand(conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.CommandText = string.Format(procName);
                var parm = new SqlParameter()
                {
                    Direction = ParameterDirection.ReturnValue,
                    SqlDbType = SqlDbType.Int
                };
                comm.Parameters.Add(parm);
                foreach (var o in parameters)
                {
                    comm.Parameters.Add(o.ParameterName, o.Value);
                }
                comm.ExecuteScalar();
                CloseConnection(conn);
                return comm.Parameters[0].Value.ToString();
            }
        }

        private static List<T> MapData<T>(IDataReader dr) where T : new()
        {
            var pocoType = typeof(T);
            var entitys = new List<T>();
            var hashtable = new Hashtable();
            var properties = pocoType.GetProperties();
            foreach (var info in properties)
            {
                hashtable[info.Name.ToUpper()] = info;
            }
            while (dr.Read())
            {
                var newObject = new T();
                for (var index = 0; index < dr.FieldCount; index++)
                {
                    var name = dr.GetName(index).ToUpper();
                    var info = (PropertyInfo)hashtable[name];
                    if ((info == null) || !info.CanWrite) continue;
                    var value = dr.GetValue(index);
                    if (dr.GetValue(index) == null || dr.GetValue(index) == DBNull.Value) continue;
                    if (dr.GetValue(index) is long)
                    {
                        value = Convert.ToInt32(dr.GetValue(index));
                    }
                    info.SetValue(newObject, value, null);
                }
                entitys.Add(newObject);
            }
            dr.Close();
            return entitys;
        }

        public void Dispose()
        {
            _conn.Close();
        }
    }

    public enum AccessEnvironment
    {
        Test = 1,
        Product = 2
    }
}
