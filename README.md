#HP Service Manager Attachment Donload
> This project was created to retrieve images from Service Manager by connect to database directly. And these images were used to train neural network, [ImagesCategory](https://github.com/daizhen/ImagesCategory "ImagesCategory") which aims to predict the ticket category triplets by only looking at the image itself. 
> 
> During the process, I found some piece of code are reusable. Such as the code to retrieve attachment from database, decode the raw bytes, combine attachment segments and decompress the data to get the readable data files.

The core method is `AttachmentRetriever.RetrieveAttachment(string topic, int type, string uid, OracleConnection conn) `

```csharp
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
```

