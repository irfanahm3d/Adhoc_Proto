using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Adhoc_Proto
{
    class MPS7Data
    {
        const string binaryDataFilePath = @"./Assets/txnlog.dat";
        public byte VersionNumber { get; set; }
        public UInt32 RecordsAmount { get; set; }

        IDictionary<RecordType, IDictionary<UInt64, IList<IRecord>>> RecordsMap 
            = new Dictionary<RecordType, IDictionary<UInt64, IList<IRecord>>>();

        /// <summary>
        /// Get total amount (in dollars) of debits.
        /// </summary>
        /// <returns>Amount (in dollars) of debits</returns>
        public Double GetTotalDebitAmount()
        {
            return CalculateTotalAmount(RecordType.Debit);
        }

        /// <summary>
        /// Get total amount (in dollars) of credits.
        /// </summary>
        /// <returns>Amount (in dollars) of credits</returns>
        public Double GetTotalCreditAmount()
        {
            return CalculateTotalAmount(RecordType.Credit);
        }

        /// <summary>
        /// Get total count of StartAutopays.
        /// </summary>
        /// <returns>Count of StartAutopays</returns>
        public int GetStartAutopayCount()
        {
            return RetrieveAutopayCount(RecordType.StartAutopay);
        }

        /// <summary>
        /// Get total count of EndAutopays.
        /// </summary>
        /// <returns>Count of EndAutopays</returns>
        public int GetEndAutopayCount()
        {
            return RetrieveAutopayCount(RecordType.EndAutopay);            
        }

        /// <summary>
        /// Get balance of a specific user.
        /// </summary>
        /// <param name="userID">The user id</param>
        /// <returns>The final balance (credit - debit) of the user</returns>
        public Double GetBalanceForUser(UInt64 userID)
        {
            Double userDebitAmount = 0.0d;
            
            foreach (AccountRecord record in this.RecordsMap[RecordType.Debit][userID])
            {
                userDebitAmount += record.Amount;
            }

            Double userCreditAmount = 0.0d;
            foreach (AccountRecord record in this.RecordsMap[RecordType.Credit][userID])
            {
                userCreditAmount += record.Amount;
            }

            return userCreditAmount - userDebitAmount; 
        }

        /// <summary>
        /// Reads the binary data file, verifies it is the correct format
        /// and populates the records.
        /// </summary>
        public void LoadData()
        {
            byte[] byteArray = ReadBytesFromFile();
            ReadAndValidateHeader(byteArray);
            ReadRecords(byteArray);
        }

        /// <summary>
        /// Calculates the total amount for debits or credits, depending on which
        /// RecordType has been specified.
        /// </summary>
        /// <param name="type">
        /// The RecordType for which to calculate the total amount/
        /// Limited to RecordType.Debit and RecordType.Credit.
        /// </param>
        /// <returns>The total amount</returns>
        Double CalculateTotalAmount(RecordType type)
        {
            if (type != RecordType.Debit || type != RecordType.Credit)
            {
                throw new ArgumentException();
            }

            Double totalAmount = 0.0d;
            foreach (IList<IRecord> transactionList in this.RecordsMap[type].Values)
            {
                foreach (AccountRecord record in transactionList)
                {
                    totalAmount += record.Amount;
                }
            }

            return totalAmount;
        }

        /// <summary>
        /// Retrieves the count of the RecordType.
        /// </summary>
        /// <param name="type">The RecordType to get a count for.</param>
        /// <returns>The total count</returns>
        int RetrieveAutopayCount(RecordType type)
        {
            return this.RecordsMap[type].Values.Count;
        }

        /// <summary>
        /// Reads the binary data file.
        /// </summary>
        /// <returns>Returns a byte array of the binary data.</returns>
        byte[] ReadBytesFromFile()
        {
            using (FileStream fs = new FileStream(binaryDataFilePath, FileMode.Open, FileAccess.Read))
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

        /// <summary>
        /// Reads the header of the MPS7 binary data file to validate it is in the 
        /// expected file format, and retrieves the version number and number of 
        /// records.
        /// </summary>
        /// <param name="byteArray">The byte array of the binary data</param>
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

        /// <summary>
        /// Reads an individual record.
        /// </summary>
        /// <param name="recordInfo">A reference to the record to be populated</param>
        /// <returns>The index value by which to offset the byte array</returns>
        int ReadRecordInfo(byte[] recordInfo, ref IRecord record)
        {
            int idx = 0;
            byte recordTypeEnum = Convert.ToByte(recordInfo[idx]);
            UInt32 timestamp =
                (UInt32)(recordInfo[1]) << 24 |
                (UInt32)(recordInfo[2]) << 16 |
                (UInt32)(recordInfo[3]) << 8 |
                (UInt32)recordInfo[4];
            UInt64 userId =
                (UInt64)(recordInfo[5]) << 56 |
                (UInt64)(recordInfo[6]) << 48 |
                (UInt64)(recordInfo[7]) << 40 |
                (UInt64)(recordInfo[8]) << 32 |
                (UInt64)(recordInfo[9]) << 24 |
                (UInt64)(recordInfo[10]) << 16 |
                (UInt64)(recordInfo[11]) << 8 |
                (UInt64)(recordInfo[12]);

            if (recordTypeEnum == 0x0 || recordTypeEnum == 0x1)
            {
                Int64 temp = (Int64)
                    ((UInt64)(recordInfo[13]) << 56 |
                    (UInt64)(recordInfo[14]) << 48 |
                    (UInt64)(recordInfo[15]) << 40 |
                    (UInt64)(recordInfo[16]) << 32 |
                    (UInt64)(recordInfo[17]) << 24 |
                    (UInt64)(recordInfo[18]) << 16 |
                    (UInt64)(recordInfo[19]) << 8 |
                    (UInt64)(recordInfo[20]));
                double amount = BitConverter.Int64BitsToDouble(temp);

                record = new AccountRecord(recordTypeEnum, timestamp, userId, amount);

                // increment to bypass the 64-bit floating point
                idx += 8;
            }
            else
            {
                record = new AutopayRecord(recordTypeEnum, timestamp, userId);
            }

            // skipping the enum, timestamp, and userid by 12
            // and increment by 1 to get to the next record
            idx += 13;

            return idx;
        }

        /// <summary>
        /// Populates the records map with all the records. 
        /// The data is structured such that all records of a particular record type
        /// are stored under one mapping. Within that records are stored based on a
        /// userID.
        /// </summary>
        /// <param name="record">The record to be stored</param>
        void PopulateRecordsMap(IRecord record)
        {
            IDictionary<UInt64, IList<IRecord>> usersTransactionMap = null;

            // If a record type is not found in the mapping then create a new transaction map
            // and add the record to the mapping.
            if (!this.RecordsMap.TryGetValue(record.Type, out usersTransactionMap))
            {
                IList<IRecord> transactionList = new List<IRecord>
                {
                    record
                };

                usersTransactionMap = new Dictionary<UInt64, IList<IRecord>>
                {
                    { record.UserID, transactionList }
                };

                this.RecordsMap.Add(record.Type, usersTransactionMap);
            }
            else
            {
                IList<IRecord> transactionList = null;

                // if no transaction list found for the user then create a new one and 
                // add to it. Otherwise retrieve the one found and append to it.
                if (!usersTransactionMap.TryGetValue(record.UserID, out transactionList))
                {
                    transactionList = new List<IRecord>
                    {
                        record
                    };
                    usersTransactionMap.Add(record.UserID, transactionList);
                }
                else
                {
                    transactionList.Add(record);
                }
            }            
        }

        /// <summary>
        /// Reads the byte array to retrieve the individual records.
        /// </summary>
        /// <param name="byteArray">The byte array of the binary data</param>
        void ReadRecords(byte[] byteArray)
        {
            int recordCount = 0;
            int idx = 9;
            while (idx < byteArray.Length)
            {
                IRecord record = null;
                
                // Copying a block from the original array with the 
                // potential size of 21 bytes.
                // RecordType = 1 byte
                // Timestamp = 4 bytes
                // UserID = 8 bytes
                // Amount = 8 bytes
                // Amount only exists if the type is Debit or Credit.
                byte[] recordInfo = new byte[21];
                Buffer.BlockCopy(byteArray, idx, recordInfo, 0, 21);

                idx += ReadRecordInfo(recordInfo, ref record);
                PopulateRecordsMap(record);
                recordCount++;
                //Console.WriteLine(recordCount);
            }

            // In the scenario the records read are not equal to the record amount
            // read from the header
            if (recordCount != this.RecordsAmount)
            {
                //throw new Exception();
            }
        }
    }
}
