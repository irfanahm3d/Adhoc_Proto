using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adhoc_Proto
{
    class MPS7Data
    {
        public byte VersionNumber { get; set; }
        public UInt32 RecordsAmount { get; set; }

        public void LoadData()
        {
            byte[] byteArray = ReadBytesFromFile();
            ReadAndValidateHeader(byteArray);
            ReadRecords(byteArray);
        }

        byte[] ReadBytesFromFile()
        {
            using (FileStream fs = new FileStream(@"./Assets/txnlog.dat", FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] txnByteArray = new byte[br.BaseStream.Length];
                    int idx = 0;
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        txnByteArray[idx] = br.ReadByte();
                        idx++;
                    }

                    return txnByteArray;
                }
            }                
        }

        void ReadAndValidateHeader(byte[] byteArray)
        {
            StringBuilder sb = new StringBuilder();
            for (int idx = 0; idx < 4; idx++)
            {
                sb.Append(Convert.ToChar(byteArray[idx]));
            }

            // To validate the file is in the expected format
            if (!sb.ToString().Equals("MPS7"))
            {
                throw new FormatException();
            }

            this.VersionNumber = Convert.ToByte(byteArray[4]);
            this.RecordsAmount = 
                (UInt32)(byteArray[5]) << 24 |
                (UInt32)(byteArray[6]) << 16 |
                (UInt32)(byteArray[7]) << 8 |
                byteArray[8];
        }

        void ReadRecords(byte[] byteArray)
        {
            int idx = 9;
            while (idx < byteArray.Length)
            {
                byte recordTypeEnum = Convert.ToByte(byteArray[idx]);
                UInt32 timestamp =
                    (UInt32)(byteArray[idx + 1]) << 24 |
                    (UInt32)(byteArray[idx + 2]) << 16 |
                    (UInt32)(byteArray[idx + 3]) << 8 |
                    (UInt32)byteArray[idx + 4];
                UInt64 userId =
                    (UInt64)(byteArray[idx + 5]) << 56 |
                    (UInt64)(byteArray[idx + 6]) << 48 |
                    (UInt64)(byteArray[idx + 7]) << 40 |
                    (UInt64)(byteArray[idx + 8]) << 32 |
                    (UInt64)(byteArray[idx + 9]) << 24 |
                    (UInt64)(byteArray[idx + 10]) << 16 |
                    (UInt64)(byteArray[idx + 11]) << 8 |
                    (UInt64)(byteArray[idx + 12]);

                Console.WriteLine("Record Type:" + recordTypeEnum);
                Console.WriteLine("Unix Timestamp:" + timestamp);
                Console.WriteLine("User ID: " + userId);

                if (recordTypeEnum == (byte)0 || recordTypeEnum == (byte)1)
                {
                    Int64 temp = (Int64)
                        ((UInt64)(byteArray[idx + 13]) << 56 |
                        (UInt64)(byteArray[idx + 14]) << 48 |
                        (UInt64)(byteArray[idx + 15]) << 40 |
                        (UInt64)(byteArray[idx + 16]) << 32 |
                        (UInt64)(byteArray[idx + 17]) << 24 |
                        (UInt64)(byteArray[idx + 18]) << 16 |
                        (UInt64)(byteArray[idx + 19]) << 8 |
                        (UInt64)(byteArray[idx + 20]));
                    double amount = BitConverter.Int64BitsToDouble(temp);

                    Console.WriteLine("Amount: " + amount);

                    // increment to bypass the 64-bit floating point
                    idx += 8;
                }

                // skipping the enum, timestamp, and userid
                idx += 12;
                // increment by 1 to get to the next record
                idx += 1;
            }
            Console.ReadKey();
        }
    }
}
