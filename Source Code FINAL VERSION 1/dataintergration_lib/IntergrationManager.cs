using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsdao_lib;
using rsbo_lib;
using System.Data;
using System.Data.SqlClient;

namespace dataintergration_lib
{
    public class IntergrationManager
    {
        public void execute()
        {
            DI_DAO dao = null;
            DISchedule schedule = new DISchedule();
            schedule.Log = string.Empty;
            try
            {
                dao = new DI_DAO();
                dao.beginTransaction();
                //modify
                dao.CLEAN_ALL_INPUT_DATA();

                List<DIDataSource> lstDataSource = dao.getDataSource();
                foreach (DIDataSource source in lstDataSource)
                {
                    schedule.Log += "Update data from Database: " + source.DataID + "\n";
                    source.queries = dao.getQueries(source.DataID);
                    foreach (Query query in source.queries)
                    {
                        query.parameters = dao.getParamters(query.QueryID);
                        query.mappings = dao.getMappings(query.QueryID);
                        List<DITableSource> lstTableSources = dao.getTableSource(query.QueryID);
                        // build query to data source
                        foreach (DITableSource table in lstTableSources)
                        {
                            query.String = query.String.Replace(table.Type, table.TableID);
                        }
                        // Create connection to Data Source
                        dao.connectSource(source.ConnectionString);

                        // Create parameter for query
                        Dictionary<string, object> lstParams = new Dictionary<string, object>();
                        foreach (DIParameter param in query.parameters)
                        {
                            if (param.Type != "QUERY")
                            {
                                lstParams.Add(param.ParameterID, param.Value);
                            }
                            else
                            {
                                lstParams.Add(param.ParameterID, dao.getQueryParameter(param.Value).Rows[0][0]);
                            }
                        }

                        // query data from source to DataTable
                        DataTable result = dao.selectSourceData(query.String, lstParams);
                        //System.Console.WriteLine(query.String);

                        // Build Insert comand
                        int counter = 0;//number of added records
                        foreach (DataRow row in result.Rows)
                        {
                            string insertCommand = "INSERT INTO " + query.DescTable + "(";
                            string values = "VALUES(";
                            Dictionary<string, object> lstParamsforInsert = new Dictionary<string, object>();
                            foreach (DIMapping mapping in query.mappings)
                            {
                                insertCommand += mapping.DesColumnID + ",";
                                values += "@" + mapping.SrcColumnName + ",";
                                lstParamsforInsert.Add("@" + mapping.SrcColumnName, row[mapping.SrcColumnName]);
                            }
                            insertCommand = insertCommand.Substring(0, insertCommand.Length - 1);
                            values = values.Substring(0, values.Length - 1);
                            insertCommand += ")" + values + ")";
                            if (dao.insertData(insertCommand, lstParamsforInsert))
                            {
                                counter++;
                            }
                        }
                        schedule.Log += counter + " records have been loaded to Table " + query.DescTable + "\n";

                    }
                    schedule.Log += "===============================================\n";
                }
                dao.addSchedule(schedule);
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public List<string> getListTables(string connectionString, out string message)
        {
            DI_DAO dao = null;
            List<string> tables = null;
            try
            {
                dao = new DI_DAO();
                dao.connectSource(connectionString);
                tables = dao.getTablesFromSource();
                message = string.Empty;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return tables;
        }

        public List<DITableSourceType> getTableSourceTypes()
        {
            DI_DAO dao = null;
            List<DITableSourceType> list = null;
            try
            {
                dao = new DI_DAO();
                dao.beginTransaction();
                list = dao.getTableSourceType();
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
            return list;
        }

        public List<Query> getQuery()
        {
            DI_DAO dao = null;
            List<Query> list = null;
            try
            {
                dao = new DI_DAO();
                dao.beginTransaction();
                //list = dao.getQueries();
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
            return list;
        }
        
        public List<DIDataSource> getDataSources()
        {
            DI_DAO dao = null;
            List<DIDataSource> list = null;
            try
            {
                dao = new DI_DAO();
                dao.beginTransaction();
                list = dao.getDataSource();
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
            return list;
        }

        public void addNewSource(DIDataSource datasource, List<DITableSource> tables)
        {
            DI_DAO dao = null;
            try
            {
                dao = new DI_DAO();
                dao.beginTransaction();
                dao.addDataSource(datasource);
                foreach (DITableSource table in tables)
                {
                    dao.addTableSource(table);
                }
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public void removeDataSource(string DataID)
        {
            DI_DAO dao = null;
            try
            {
                dao = new DI_DAO();
                dao.beginTransaction();
                dao.removeDataSource(DataID);
                dao.removeTableSource(DataID);
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public void updateSource(DIDataSource datasouce)
        {
            DI_DAO dao = null;
            try
            {
                dao = new DI_DAO();
                dao.beginTransaction();
                dao.updateDataSouce(datasouce);
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }
    }
}
