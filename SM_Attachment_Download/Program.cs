using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;
using System.Data;
using System.IO;
using System.IO.Compression;
using SM_Attachment_Download.Model;
namespace SM_Attachment_Download
{
    class Program
    {
        public static int GetOffsetIndex(byte[] dbData)
        {
            int dataStartIndex = 0;
            if (dbData != null && dbData.Length > 8)
            {
                //check if data start with _RCFM*=
                if (dbData[0] == 0x5f &&
                    dbData[1] == 0x52 &&
                    dbData[2] == 0x43 &&
                    dbData[3] == 0x46 &&
                    dbData[4] == 0x4d &&
                    dbData[5] == 0x2a &&
                    dbData[6] == 0x3d)
                {
                    if (dbData[7] == 0x2D)
                    {
                        dataStartIndex = 9;
                    }
                    else if (dbData[7] == 0x2E)
                    {
                        dataStartIndex = 10;
                    }
                    else if (dbData[7] == 0x2F)
                    {
                        dataStartIndex = 11;
                    }
                    else
                    {
                        dataStartIndex = 12;
                    }
                }
            }
            return dataStartIndex;
        }
        public static byte[] Decompress_1(byte[] buf,int skipCount)
        {

            try
            {
                long totalLength = 0;
                int size = 0;
                MemoryStream ms = new MemoryStream(), msD = new MemoryStream();
                ms.Write(buf, 0, buf.Length);
                ms.Seek(skipCount, SeekOrigin.Begin);
                GZipStream zip;
                zip = new GZipStream(ms, CompressionMode.Decompress);
                byte[] db;
                bool readed = false;
                while (true)
                {
                    size = zip.ReadByte();
                    if (size != -1)
                    {
                        if (!readed) readed = true;
                        totalLength++;
                        msD.WriteByte((byte)size);
                    }
                    else
                    {
                        if (readed) break;
                    }
                }
                zip.Close();
                db = msD.ToArray();
                msD.Close();
                return db;
            }
            catch (Exception ex)
            {
 
            }
            return null;
        }

        public static byte[] Decompress(byte[] buf, int skipCount)
        {
            MemoryStream msOut = new MemoryStream();
            MemoryStream msIn = new MemoryStream();
            msIn.Write(buf, skipCount, buf.Length - skipCount);
            msIn.Position = 0;
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(msOut);
            CopyStream(msIn, outZStream);

            return msOut.ToArray();
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }
        static void Main(string[] args)
        {
            //LogHelper.WriteLog(typeof(Program), "test");

            string condition = "";
            if (args.Length > 0)
            {
                condition = args[0];
            }
            /*
            RetrieveProgress retrieveProgress = new RetrieveProgress();
            retrieveProgress.InitProcessTable();
            */

            AttachmentRetriever retriever = new AttachmentRetriever();
            //retriever.Retrieve(condition);

            retriever.RetrieveSD();
            //retriever.RetrieveAttachment(new Progress() { FileName = "sqts site English menu.JPG", Topic = "SD21589241", UID = "54ebcfbc0006f11680921cf0", Type = 5 });

            //retriever.RetrieveAttachment(new Progress() { FileName = "Request Template - Business Service-original.xlsx", Topic = "SD22312686", UID = "5666a9b20013918280e103a8", Type = 5 });
            //retrieveProgress.AddProcess(new Progress() {  FileName="test.x", SavedFileName="xxx", Topic="xx", Type=5, UID="xxx"});
            /*
            long blobDataSize = 0; //BLOB数据体实际大小
            long readStartByte = 0;//从BLOB数据体的何处开始读取数据
            int bufferStartByte = 0;//将数据从buffer数组的何处开始写入
            int hopeReadSize = 10240; //希望每次从BLOB数据体中读取数据的大小
            long realReadSize = 0;//每次实际从BLOB数据体中读取数据的大小
            byte[] buffer = null;
            string ConnectionString = "Data Source=ITSMP;User ID=ITSMP;Password=Prod#itsm25Pass;Unicode=True"; //写连接串 
            // OracleConnection conn = new OracleConnection(ConnectionString); //创建一个新连接
            using (OracleConnection conn = new OracleConnection(ConnectionString))
            {
                OracleCommand cmd = new OracleCommand("select data,topic,filename from SYSATTACHMEM1 where topic='SD21683861'", conn);
                //conn.Open();
                OracleDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.Read())
                {
                    string fileName = dataReader.GetString(2);
                    blobDataSize = dataReader.GetBytes(0, 0, null, 0, 0); //获取这个BLOB数据体的总大小
                    buffer = new byte[blobDataSize];

                    realReadSize = dataReader.GetBytes(0, readStartByte, buffer, bufferStartByte, hopeReadSize);
                    //循环，每次读取1024byte大小
                    while ((int)realReadSize == hopeReadSize)
                    {
                        bufferStartByte += hopeReadSize;
                        readStartByte += realReadSize;
                        realReadSize = dataReader.GetBytes(0, readStartByte, buffer, bufferStartByte, hopeReadSize);
                    }
                    //读取BLOB数据体最后剩余的小于1024byte大小的数据
                    dataReader.GetBytes(0, readStartByte, buffer, bufferStartByte, (int)realReadSize);

                    FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    int offsetIndex = GetOffsetIndex(buffer);

                    byte[] deCompressedData = Decompress(buffer, offsetIndex);

                    fileStream.Write(deCompressedData, 0, deCompressedData.Length);
                    fileStream.Flush();
                    fileStream.Dispose();


                }
            }
             * */
        }
    }
}
