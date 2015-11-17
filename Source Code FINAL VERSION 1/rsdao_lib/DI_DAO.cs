using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using rsbo_lib;
using System.Data;

namespace rsdao_lib
{
    public class DI_DAO : BASE_DAO
    {

        public DI_DAO() : base(){}

        private SqlConnection sourceConnection;

        public SqlConnection connectSource(string connectionString)
        {
            if (sourceConnection != null)
            {
                if (sourceConnection.State == ConnectionState.Open)
                    sourceConnection.Close();
            }
            sourceConnection = new SqlConnection(connectionString);
            return sourceConnection;
        }

        public List<string> getTablesFromSource()
        {
            string strSelect = "SELECT '['+SCHEMA_NAME(schema_id)+'].['+name+']' AS TableName FROM sys.tables ORDER BY TableName ASC";
            List<string> tables = new List<string>();
            SqlCommand command = new SqlCommand(strSelect, sourceConnection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            foreach (DataRow row in table.Rows)
            {
                tables.Add(row["TableName"].ToString());
            }
            return tables;
        }


        public DataTable selectSourceData(string strSelect, Dictionary<string, object> lstParams)
        {
            SqlCommand command = new SqlCommand(strSelect, sourceConnection);
            if (lstParams != null)
                foreach (KeyValuePair<string, object> pair in lstParams)
                {
                    command.Parameters.AddWithValue(pair.Key, pair.Value);
                }
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table; 
        }

        public bool insertData(string strCommand,Dictionary<string,object> lstParams)
        {
            bool result = false;
            command.CommandText = strCommand;
            if (lstParams != null)
                foreach (KeyValuePair<string, object> pair in lstParams)
                {
                    command.Parameters.AddWithValue(pair.Key, pair.Value);
                }
            try
            {
                command.ExecuteNonQuery();
                result = true;
            }
            catch (SqlException sqe)
            {
                if (sqe.Number != 2627)
                {
                    throw sqe;
                }
            }
            command.Parameters.Clear();
            return result;
        }

        public DataTable getQueryParameter(string strSelect)
        {
            command.CommandText = strSelect;
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            command.ExecuteNonQuery();
            command.Parameters.Clear();
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public void addSchedule(DISchedule schedule)
        {
            string strQuery = "INSERT INTO [DI].[DATA_INTERGRATION_SCHEDULE]([StartTime],[StopTime],[Log]) VALUES (GETDATE(),GETDATE(),@Log)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Log", schedule.Log);
            executeNonQuery(strQuery, parameters);
        }


        public List<DIDataSource> getDataSource()
        {
            string strSelect = "SELECT [DataID],[ConnectionString],[Type] FROM [DI].[DATA_SOURCE]";
            SqlDataReader dr = executeReader(strSelect);

            List<DIDataSource> list = new List<DIDataSource>();
            while (dr.Read())
            {
                DIDataSource obj = new DIDataSource();
                obj.DataID = dr.GetString(dr.GetOrdinal("DataID"));
                obj.ConnectionString = dr.GetString(dr.GetOrdinal("ConnectionString"));
                obj.Type = dr.GetString(dr.GetOrdinal("Type"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<Query> getQueries(string DataID)
        {
            string strSelect = "SELECT [QueryID],[DescTable],[OrderNo],[String],[Description],[DataID] FROM [DI].[QUERY] WHERE [DataID] = @DataID ORDER BY [OrderNo] ASC";
            Dictionary<string,object> parameters = new Dictionary<string,object>();
            parameters.Add("@DataID",DataID);
            SqlDataReader dr = executeReader(strSelect,parameters);

            List<Query> list = new List<Query>();
            while (dr.Read())
            {
                Query obj = new Query();
                obj.QueryID = dr.GetString(dr.GetOrdinal("QueryID"));
                obj.DescTable = dr.GetString(dr.GetOrdinal("DescTable"));
                obj.OrderNo = dr.GetInt32(dr.GetOrdinal("OrderNo"));
                obj.String = dr.GetString(dr.GetOrdinal("String"));
                obj.Description = dr.GetString(dr.GetOrdinal("Description"));
                obj.DataID = dr.GetString(dr.GetOrdinal("DataID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<DIParameter> getParamters(string QueryID)
        {
            string strSelect = "SELECT [ParameterID],[QueryID],[Value],[Type] FROM [DI].[PARAMETER] WHERE [QueryID] = @QueryID ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@QueryID", QueryID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<DIParameter> list = new List<DIParameter>();
            while (dr.Read())
            {
                DIParameter obj = new DIParameter();
                obj.ParameterID = dr.GetString(dr.GetOrdinal("ParameterID"));
                obj.QueryID = dr.GetString(dr.GetOrdinal("QueryID"));
                obj.Value = dr.GetString(dr.GetOrdinal("Value"));
                obj.Type = dr.GetString(dr.GetOrdinal("Type"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<DIMapping> getMappings(string QueryID)
        {
            string strSelect = "SELECT [MappingID],[DesColumnID],[SrcColumnName],[QueryID] FROM [DI].[RESULT_MAPPING] WHERE [QueryID] = @QueryID ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@QueryID", QueryID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<DIMapping> list = new List<DIMapping>();
            while (dr.Read())
            {
                DIMapping obj = new DIMapping();
                obj.MappingID = dr.GetInt32(dr.GetOrdinal("MappingID"));
                obj.DesColumnID = dr.GetString(dr.GetOrdinal("DesColumnID"));
                obj.SrcColumnName = dr.GetString(dr.GetOrdinal("SrcColumnName"));
                obj.QueryID = dr.GetString(dr.GetOrdinal("QueryID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<DITableSource> getTableSource(string QueryID)
        {
            string strSelect = "SELECT [TableID],[QueryID],[Type] FROM [DI].[TABLE_SOURCE] WHERE [QueryID] = @QueryID";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@QueryID", QueryID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<DITableSource> list = new List<DITableSource>();
            while (dr.Read())
            {
                DITableSource obj = new DITableSource();
                obj.TableID = dr.GetString(dr.GetOrdinal("TableID"));
                obj.QueryID = dr.GetString(dr.GetOrdinal("QueryID"));
                obj.Type = dr.GetString(dr.GetOrdinal("Type"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<DITableSourceType> getTableSourceType()
        {
            string strSelect = "SELECT [TableTypeID],[Description],[Name] FROM [DI].[TABLE_SOURCE_TYPE]";
            SqlDataReader dr = executeReader(strSelect);

            List<DITableSourceType> list = new List<DITableSourceType>();
            while (dr.Read())
            {
                DITableSourceType obj = new DITableSourceType();
                obj.TableTypeID = dr.GetString(dr.GetOrdinal("TableTypeID"));
                obj.Description = dr.GetString(dr.GetOrdinal("Description"));
                obj.Name = dr.GetString(dr.GetOrdinal("Name"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }


        public void addDataSource(DIDataSource source)
        {
            string strQuery = "INSERT INTO [DI].[DATA_SOURCE]([DataID],[ConnectionString],[Type]) VALUES (@DataID,@ConnectionString,@Type)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@DataID", source.DataID);
            parameters.Add("@ConnectionString", source.ConnectionString);
            parameters.Add("@Type", source.Type);
            executeNonQuery(strQuery, parameters);
        }

        public void addTableSource(DITableSource table)
        {
            string strQuery = "INSERT INTO [DI].[TABLE_SOURCE]([TableID],[QueryID],[Type]) VALUES(@TableID,@QueryID,@Type)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@TableID", table.TableID);
            parameters.Add("@QueryID", table.QueryID);
            parameters.Add("@Type", table.Type);
            executeNonQuery(strQuery, parameters);
        }
        
        public void removeDataSource(string DataID)
        {
            string strQuery = "DELETE [DI].[DATA_SOURCE] WHERE [DataID] = @DataID";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@DataID", DataID);
            executeNonQuery(strQuery, parameters);
        }

        public void removeTableSource(string DataID)
        {
            string strQuery = "DELETE [DI].[TABLE_SOURCE] WHERE [DataID] = @DataID";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@DataID", DataID);
            executeNonQuery(strQuery, parameters);
        }


        public void updateDataSouce(DIDataSource datasouce)
        {
            string strQuery = "UPDATE [DI].[DATA_SOURCE] SET [ConnectionString] = @ConnectionString,[Type] = @Type WHERE [DataID] = @DataID";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@DataID", datasouce.DataID);
            parameters.Add("@ConnectionString", datasouce.ConnectionString);
            parameters.Add("@Type", datasouce.Type);
            executeNonQuery(strQuery, parameters);
        }

        public void CLEAN_ALL_INPUT_DATA()
        {
            string strSelect = "DELETE  [RS].[USER_TBL]";
            executeNonQuery(strSelect);

            strSelect = "DELETE [RS].[ITEM_TBL]";
            executeNonQuery(strSelect);

            strSelect = "DELETE [RS].[TRANSACTION_TBL]";
            executeNonQuery(strSelect);

            //strSelect = "DELETE [RS].[PRICE_TBL]";
            //executeNonQuery(strSelect);

        }
    }
}
