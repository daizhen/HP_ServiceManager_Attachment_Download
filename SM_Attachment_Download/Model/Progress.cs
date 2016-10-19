using System;
using System.Collections.Generic;
using System.Text;

namespace SM_Attachment_Download.Model
{
    public class Progress
    {
        public string Topic
        {
            get;
            set;
        }

        public int Type
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public string UID
        {
            get;
            set;
        }

        public string SavedFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Init:0
        /// Completed:1
        /// Error:2
        /// </summary>
        public int Status
        {
            get;
            set;
        }

        public override string ToString()
        {
            return "Topic:" + Topic + " \r\n Type:" + Type + "\r\n Filename:" + FileName + "\r\n UID:" + UID + "\r\n SaveFileName:" + SavedFileName + "\r\nStatus:" + Status;
        }
    }
}
