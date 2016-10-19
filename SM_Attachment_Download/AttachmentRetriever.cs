using SM_Attachment_Download.Model;
using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.IO;
using System.Text;

namespace SM_Attachment_Download
{
    public class AttachmentRetriever
    {
        // HP service manager oracle database connection. 
        string ConnectionString_Data = "Data Source=DATASOURCE;User ID=UID;Password=PWD;Unicode=True";
        //The oracle database contained Process table, the database can be the same one as the attachment database.
        string ConnectionString_Process = "Data Source=DATASOURCE_PROCESS;User ID=UID_PROCESS;Password=PWD_PROCESS;Unicode=True";
        public string Dir
        {
            get
            {
                return "files";
            }
        }
        public void RetrieveAttachment(Progress progressEntity, OracleConnection conn)
        {
            try
            {
                string savedFileName = (progressEntity.Topic + progressEntity.FileName).ToLower();
                if (File.Exists(Dir + "\\" + savedFileName))
                {
                    //Update the progress record to 'completed'.
                    progressEntity.Status = 1;
                    progressEntity.SavedFileName = savedFileName;
                    new RetrieveProgress().UpdateProress(progressEntity);
                    return;
                }
                byte[] decompressedData = RetrieveAttachment(progressEntity.Topic, progressEntity.Type, progressEntity.UID, conn);

                //Save the data to local disk
                FileStream fileStream = new FileStream(Dir + "\\" + savedFileName, FileMode.Create, FileAccess.Write);
                fileStream.Write(decompressedData, 0, decompressedData.Length);
                fileStream.Flush();
                fileStream.Dispose();

                //Update the progress record to 'completed'.
                progressEntity.Status = 1;
                progressEntity.SavedFileName = savedFileName;
                new RetrieveProgress().UpdateProress(progressEntity);

            }
            catch (Exception ex)
            {
                //Update the progress record to 'completed'.
                progressEntity.Status = 2;
                progressEntity.SavedFileName = progressEntity.FileName;
                new RetrieveProgress().UpdateProress(progressEntity);
                LogHelper.WriteLog(typeof(AttachmentRetriever), ex);
            }
        }

        public byte[] RetrieveAttachment(string topic, int type, string uid, OracleConnection conn)
        {
            byte[] buffer = null;
            OracleCommand cmd = new OracleCommand("select data from SYSATTACHMEM1 where application='incidents' and  topic=:topic and type=:type and \"UID\"=:UIDOVSC order by segment", conn);
            cmd.Parameters.Add(new OracleParameter("topic", topic));
            cmd.Parameters.Add(new OracleParameter("type", type));
            cmd.Parameters.Add(new OracleParameter("UIDOVSC", uid));

            OracleDataReader dataReader = cmd.ExecuteReader();
            MemoryStream attachmentStream = new MemoryStream();

            //Because a attachment may store in multi segments, so we need to loop and read all the segments.
            while (dataReader.Read())
            {
                long readStartByte = 0;
                int bufferStartByte = 0;
                int hopeReadSize = 20480;

                long blobDataSize = dataReader.GetBytes(0, 0, null, 0, 0);
                buffer = new byte[blobDataSize];

                long realReadSize = dataReader.GetBytes(0, readStartByte, buffer, bufferStartByte, hopeReadSize);
                while ((int)realReadSize == hopeReadSize)
                {
                    bufferStartByte += hopeReadSize;
                    readStartByte += realReadSize;
                    realReadSize = dataReader.GetBytes(0, readStartByte, buffer, bufferStartByte, hopeReadSize);
                }
                dataReader.GetBytes(0, readStartByte, buffer, bufferStartByte, (int)realReadSize);

                int offsetIndex = DBBlobUtil.GetOffsetIndex(buffer);
                //Write data to stream
                attachmentStream.Write(buffer, offsetIndex, buffer.Length - offsetIndex);
            }

            //Decompress the data, because in database the attachment data is compressed.
            byte[] decompressedData = DBBlobUtil.Decompress(attachmentStream.ToArray());

            return decompressedData;
        }

        public void Retrieve(string condition)
        {
            OracleConnection connData = new OracleConnection(ConnectionString_Data);
            int count = 0;
            using (OracleConnection conn = new OracleConnection(ConnectionString_Process))
            {
                connData.Open();
                using (connData)
                {
                    string queryString = "select distinct TOPIC,STATUS,TYPE,FILENAME,UID_ATT from ATTACHMENT_PROGRESS where STATUS=0";
                    if (!string.IsNullOrEmpty(condition))
                    {
                        queryString = queryString + " and " + condition;
                    }
                    OracleCommand cmd = new OracleCommand(queryString, conn);
                    conn.Open();
                    OracleDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        string topic = dataReader.GetString(0);
                        int status = dataReader.GetInt32(1);
                        int type = dataReader.GetInt32(2);
                        string fileName = dataReader.GetString(3);
                        string uid = dataReader.GetString(4);

                        Progress progressEntity = new Progress();
                        progressEntity.Topic = topic;
                        progressEntity.Type = type;
                        progressEntity.FileName = fileName;
                        progressEntity.UID = uid;
                        progressEntity.Status = status;

                        RetrieveAttachment(progressEntity, connData);
                        count++;
                        if (count % 200 == 0)
                        {
                            Console.WriteLine("Process attachment count:" + count);
                        }
                    }
                }
            }
        }

        public void RetrieveSD()
        {
            OracleConnection conn = new OracleConnection(ConnectionString_Process);
            conn.Open();
            OracleCommand cmd = new OracleCommand("select distinct topic from ATTACHMENT_PROGRESS where category is null and saved_filename like '%.png' or saved_filename like '%.jpg' or saved_filename like '%.jpeg'", conn);

            OracleDataReader dataReader = cmd.ExecuteReader();
            RetrieveProgress progress = new RetrieveProgress();
            while (dataReader.Read())
            {
                var topic = dataReader.GetString(0);

                using(OracleConnection conn_Data = new OracleConnection(ConnectionString_Data))
                {
                    conn_Data.Open();
                    OracleCommand cmd_Data = new OracleCommand("select category,subcategory,product_type,hp_prod_spec_id from incidentsm1 where incident_id=:incident_id", conn_Data);
                    cmd_Data.Parameters.Add(new OracleParameter("incident_id", topic));
                    try
                    {
                        OracleDataReader dataReader_data = cmd_Data.ExecuteReader();
                        if (dataReader_data.Read())
                        {
                            string category = dataReader_data.GetString(0);
                            if (string.IsNullOrEmpty(category))
                            {
                                category = "NULL";
                            }
                            string subcategory = dataReader_data.GetString(1);
                            if (string.IsNullOrEmpty(subcategory))
                            {
                                subcategory = "NULL";
                            }
                            
                            string product_type = "NULL";
                            if (!dataReader_data.IsDBNull(2))
                            {
                                product_type = dataReader_data.GetString(2);
                            }
                            string hp_prod_spec_id = "NULL";
                            
                            //dataReader_data.GetString(3);
                            if (!dataReader_data.IsDBNull(3))
                            {
                                hp_prod_spec_id = dataReader_data.GetString(3);
                            }
                            progress.UpdateSDInfo(topic, category, subcategory, product_type, hp_prod_spec_id);
                        }
                    }
                    catch (Exception ex)
                    {
 
                    }
                }

            }
            conn.Close();
        }
    }
}
