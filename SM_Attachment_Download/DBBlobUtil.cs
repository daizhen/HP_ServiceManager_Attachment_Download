using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SM_Attachment_Download
{
    public class DBBlobUtil
    {
        /// <summary>
        /// Get the data start index, the bytes from 0 to the start index will be ignored.
        /// </summary>
        /// <param name="dbData"></param>
        /// <returns></returns>
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

        public static byte[] Decompress(byte[] buf)
        {
            MemoryStream msOut = new MemoryStream();
            MemoryStream msIn = new MemoryStream();
            msIn.Write(buf, 0, buf.Length);
            msIn.Position = 0;
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(msOut);
            CopyStream(msIn, outZStream);

            return msOut.ToArray();
        }
        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[10240];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

    }
}
