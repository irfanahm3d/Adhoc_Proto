using System;

namespace Adhoc.Proto
{
    internal enum RecordType
    {
        Debit=0x00,
        Credit=0x01,
        StartAutopay=0x02,
        EndAutopay=0x03
    }

    internal interface IRecord
    {
        RecordType Type { get; }
        UInt32 Timestamp { get; }
        UInt64 UserID { get; }
    }

    internal class AccountRecord : IRecord
    {
        public RecordType Type { get; private set; }
        public UInt32 Timestamp { get; private set; }
        public UInt64 UserID { get; private set; }
        public Double Amount { get; private set; }

        public AccountRecord(byte type, UInt32 timestamp, UInt64 userid, Double amount)
        {
            RecordType parsedType;
            if (!Enum.TryParse(type.ToString(), false, out parsedType))
            {
                throw new ArgumentException();
            }
            this.Type = parsedType;
            this.Timestamp = timestamp;
            this.UserID = userid;
            this.Amount = amount;
        }
    }

    internal class AutopayRecord : IRecord
    {
        public RecordType Type { get; private set; }
        public UInt32 Timestamp { get; private set; }
        public UInt64 UserID { get; private set; }

        public AutopayRecord(byte type, UInt32 timestamp, UInt64 userid)
        {
            RecordType parsedType;
            if (!Enum.TryParse(type.ToString(), false, out parsedType))
            {
                throw new ArgumentException();
            }
            this.Type = parsedType;
            this.Timestamp = timestamp;
            this.UserID = userid;
        }
    }
}
