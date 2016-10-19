using SM_Attachment_Download.Model;
using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Diagnostics;
using System.Text;

namespace SM_Attachment_Download
{
    public class RetrieveProgress
    {
        string ConnectionString_Data = "Data Source=ITSMP;User ID=ITSMP;Password=Prod#itsm25Pass;Unicode=True";
        string ConnectionString_Process = "Data Source=ZHANGMEI;User ID=sm;Password=sm;Unicode=True";
        
        /// <summary>
        /// Init Process table.
        /// </summary>
        public void InitProcessTable()
        {
            int count = 0;
            using (OracleConnection conn = new OracleConnection(ConnectionString_Data))
            {
                OracleCommand cmd = new OracleCommand("select distinct topic,type,filename,\"UID\" from SYSATTACHMEM1 where application='incidents'", conn);
                conn.Open();
                OracleDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    string topic = dataReader.GetString(0);
                    int type = dataReader.GetInt32(1);
                    string fileName = dataReader.GetString(2);
                    string uid = dataReader.GetString(3);

                    Progress progressEntity = new Progress();
                    progressEntity.Topic = topic;
                    progressEntity.Type = type;
                    progressEntity.FileName = fileName;
                    progressEntity.UID = uid;
                    AddProcess(progressEntity);
                    count++;
                    if (count % 1000 == 0)
                    {
                        Console.WriteLine("Init count:" + count);
                    }
                }
            }
        }

        public void AddProcess(Progress progressEntity)
        {
            try
            {
                int initStatus = 0;
                using (OracleConnection conn = new OracleConnection(ConnectionString_Process))
                {
                    conn.Open();
                    OracleCommand cmd = new OracleCommand("Insert into ATTACHMENT_PROGRESS(TOPIC,STATUS,TYPE,FILENAME,UID_ATT) values (:TOPIC,:STATUS,:TYPE,:FILENAME,:UID_ATT)", conn);
                    cmd.Parameters.Add(new OracleParameter("TOPIC", progressEntity.Topic));
                    cmd.Parameters.Add(new OracleParameter("STATUS", initStatus));
                    cmd.Parameters.Add(new OracleParameter("TYPE", progressEntity.Type));
                    cmd.Parameters.Add(new OracleParameter("FILENAME", progressEntity.FileName));
                    cmd.Parameters.Add(new OracleParameter("UID_ATT", progressEntity.UID));
                    int result = cmd.ExecuteNonQuery();
                    if (result != 1)
                    {
                        LogHelper.WriteLog(typeof(RetrieveProgress), "Init:" + progressEntity.Topic + " failed..");
                        Console.WriteLine("Init:" + progressEntity.Topic + " failed..");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(RetrieveProgress), ex);
            }
        }

        public void UpdateProress(Progress progressEntity)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(ConnectionString_Process))
                {
                    conn.Open();
                    OracleCommand cmd = new OracleCommand("Update ATTACHMENT_PROGRESS set STATUS=:STATUS,SAVED_FILENAME=:SAVED_FILENAME where TOPIC=:TOPIC and TYPE =:TYPE and UID_ATT=:UID_ATT", conn);
                    cmd.Parameters.Add(new OracleParameter("TOPIC", progressEntity.Topic));
                    cmd.Parameters.Add(new OracleParameter("STATUS", progressEntity.Status));
                    cmd.Parameters.Add(new OracleParameter("TYPE", progressEntity.Type));
                    //cmd.Parameters.Add(new OracleParameter("FILENAME", progressEntity.FileName));
                    cmd.Parameters.Add(new OracleParameter("UID_ATT", progressEntity.UID));
                    cmd.Parameters.Add(new OracleParameter("SAVED_FILENAME", progressEntity.SavedFileName));
                    int result = cmd.ExecuteNonQuery();
                    if (result != 1)
                    {
                        LogHelper.WriteLog(typeof(RetrieveProgress), "Update progress:" + progressEntity.Topic + " failed..");
                    }
                }
            }
            catch (OracleException oracleEx)
            {
                LogHelper.WriteLog(typeof(RetrieveProgress), "progressEntity:" + progressEntity.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(RetrieveProgress), "Update progress:" + progressEntity.Topic + " failed..");
                LogHelper.WriteLog(typeof(RetrieveProgress), ex);
            }
        }
    

        public void UpdateSDInfo(string topic, string category,string subcategory,string product_type,string hp_prod_spec_id)
        {

            try
            {
                using (OracleConnection conn = new OracleConnection(ConnectionString_Process))
                {
                    conn.Open();
                    OracleCommand cmd = new OracleCommand("Update ATTACHMENT_PROGRESS set category=:category,subcategory=:subcategory,product_type=:product_type,hp_prod_spec_id=:hp_prod_spec_id where TOPIC=:TOPIC", conn);
                    cmd.Parameters.Add(new OracleParameter("TOPIC", topic));
                    cmd.Parameters.Add(new OracleParameter("category", category));
                    cmd.Parameters.Add(new OracleParameter("subcategory", subcategory));
                    cmd.Parameters.Add(new OracleParameter("product_type", product_type));
                    cmd.Parameters.Add(new OracleParameter("hp_prod_spec_id", hp_prod_spec_id));
                    int result = cmd.ExecuteNonQuery();
                    if (result != 1)
                    {
                        LogHelper.WriteLog(typeof(RetrieveProgress), "Update progress:" + topic + " failed..");
                    }
                }
            }
            catch (OracleException oracleEx)
            {
                LogHelper.WriteLog(typeof(RetrieveProgress), "progressEntity:");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(RetrieveProgress), "Update progress:" + topic + " failed..");
                LogHelper.WriteLog(typeof(RetrieveProgress), ex);
            }
        }
    }
}
