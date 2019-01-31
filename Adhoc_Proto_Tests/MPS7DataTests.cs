using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Adhoc.Proto;
using System.Collections;
using System.Collections.Generic;

namespace Adhoc.Proto.Tests
{
    [TestClass]
    public class MPS7DataTests
    {
        const UInt64 UserId_1 = 17337865973615628454;
        const UInt64 UserId_2 = 7299386991593269749;
        const UInt64 UserId_3 = 4660829384912539755;
        const UInt64 UserId_4 = 6123808318520183820;
        MPS7Data mps = new MPS7Data();

        public MPS7DataTests()
        {
            GenerateTestDataRecordMap();
        }

        #region Unit Tests

        [TestMethod]
        public void CalculateTotalAmountTests_Success()
        {
            var testData = new[]
            {
                new
                {
                    RecordType = RecordType.Debit,
                    ExpectedTotal = 3591.06d
                },
                new
                {
                    RecordType = RecordType.Credit,
                    ExpectedTotal = 21632.11d
                }
            };

            foreach (var data in testData)
            {
                Console.WriteLine("Running scenario - RecordType " + data.RecordType);

                Double amount = mps.CalculateTotalAmount(data.RecordType);

                Assert.AreEqual(
                    expected: data.ExpectedTotal,
                    actual: amount,
                    message: "The amounts are not equal.");
            }
        }

        [TestMethod]
        public void CalculateTotalAmountTests_Failure()
        {
            var testData = new[]
            {
                new
                {
                    RecordType = RecordType.StartAutopay,
                },
                new
                {
                    RecordType = RecordType.EndAutopay,
                }
            };

            foreach (var data in testData)
            {
                Console.WriteLine("Running scenario - RecordType " + data.RecordType);

                try
                {
                    Double amount = mps.CalculateTotalAmount(data.RecordType);
                }
                catch (ArgumentException argEx)
                {
                    Assert.AreEqual(
                        expected: typeof(ArgumentException),
                        actual: argEx.GetType(),
                        message: "The actual exception was not expected.");
                }
            }
        }

        [TestMethod]
        public void RetrieveAutopayCountTest()
        {
            var testData = new[]
              {
                new
                {
                    RecordType = RecordType.StartAutopay,
                },
                new
                {
                    RecordType = RecordType.EndAutopay,
                }
            };

            foreach (var data in testData)
            {
                Console.WriteLine("Running scenario - RecordType " + data.RecordType);

                int count = mps.RetrieveAutopayCount(data.RecordType);

                Assert.AreEqual(
                    expected: mps.RecordsMap[data.RecordType].Count,
                    actual: count,
                    message: "The count is not the same.");
            }
        }

        #endregion

        #region Functional Tests

        [TestMethod]
        public void GetBalanceForUserTests()
        {
            var testData = new[]
            {
                new
                {
                    UserId = UserId_1,
                    Balance = 1690.52d
                },
                new
                {
                    UserId = UserId_3,
                    Balance = 183.25d
                }
            };

            foreach (var data in testData)
            {
                Console.WriteLine("Running the scenario - UserId" + data.UserId);

                Double balance = mps.GetBalanceForUser(data.UserId);

                Assert.AreEqual(
                    expected: data.Balance,
                    actual: balance,
                    message: "The balance is not the same.");
            }
        }

        #endregion

        private void GenerateTestDataRecordMap()
        {
            IDictionary<UInt64, IList<IRecord>> debitTransactions = new Dictionary<UInt64, IList<IRecord>>();
            debitTransactions.Add(UserId_1, new List<IRecord>
            {
                new AccountRecord((byte)RecordType.Debit, 220, UserId_1, 200.45),
                new AccountRecord((byte)RecordType.Debit, 421, UserId_1, 100.55),
                new AccountRecord((byte)RecordType.Debit, 122, UserId_1, 123.38),
                new AccountRecord((byte)RecordType.Debit, 723, UserId_1, 865.43),
                new AccountRecord((byte)RecordType.Debit, 945, UserId_1, 20.12),
            });
            debitTransactions.Add(UserId_2, new List<IRecord>
            {
                new AccountRecord((byte)RecordType.Debit, 220, UserId_2, 2000.45),
                new AccountRecord((byte)RecordType.Debit, 421, UserId_2, 50.55),
                new AccountRecord((byte)RecordType.Debit, 122, UserId_2, 13.38),
            });
            debitTransactions.Add(UserId_3, new List<IRecord>
            {
                new AccountRecord((byte)RecordType.Debit, 220, UserId_3, 20.75),
                new AccountRecord((byte)RecordType.Debit, 421, UserId_3, 10.45),
                new AccountRecord((byte)RecordType.Debit, 723, UserId_3, 65.43),
                new AccountRecord((byte)RecordType.Debit, 945, UserId_3, 120.12),
            });

            IDictionary<UInt64, IList<IRecord>> creditTransactions = new Dictionary<UInt64, IList<IRecord>>();
            creditTransactions.Add(UserId_1, new List<IRecord>
            {
                new AccountRecord((byte)RecordType.Credit, 345, UserId_1, 3000.45),
            });
            creditTransactions.Add(UserId_2, new List<IRecord>
            {
                new AccountRecord((byte)RecordType.Credit, 305, UserId_2, 5000),
                new AccountRecord((byte)RecordType.Credit, 421, UserId_2, 230.45),
            });
            creditTransactions.Add(UserId_3, new List<IRecord>
            {
                new AccountRecord((byte)RecordType.Credit, 987, UserId_3, 400),
            });
            creditTransactions.Add(UserId_4, new List<IRecord>
            {
                new AccountRecord((byte)RecordType.Credit, 550, UserId_3, 12000.75),
                new AccountRecord((byte)RecordType.Credit, 422, UserId_3, 1000.46)
            });

            IDictionary<UInt64, IList<IRecord>> startAutopayTransactions = new Dictionary<UInt64, IList<IRecord>>();
            startAutopayTransactions.Add(UserId_1, new List<IRecord>
            {
                new AutopayRecord((byte)RecordType.StartAutopay, 400, UserId_1),
            });
            startAutopayTransactions.Add(UserId_2, new List<IRecord>
            {
                new AutopayRecord((byte)RecordType.StartAutopay, 500, UserId_2),
                new AutopayRecord((byte)RecordType.StartAutopay, 800, UserId_2),
            });
            startAutopayTransactions.Add(UserId_4, new List<IRecord>
            {
                new AutopayRecord((byte)RecordType.StartAutopay, 700, UserId_4),
                new AutopayRecord((byte)RecordType.StartAutopay, 910, UserId_4),
                new AutopayRecord((byte)RecordType.StartAutopay, 7100, UserId_4),
            });

            IDictionary<UInt64, IList<IRecord>> endAutopayTransactions = new Dictionary<UInt64, IList<IRecord>>();
            endAutopayTransactions.Add(UserId_2, new List<IRecord>
            {
                new AutopayRecord((byte)RecordType.StartAutopay, 500, UserId_2),
                new AutopayRecord((byte)RecordType.StartAutopay, 676, UserId_2),
            });
            endAutopayTransactions.Add(UserId_4, new List<IRecord>
            {
                new AutopayRecord((byte)RecordType.EndAutopay, 840, UserId_4),
                new AutopayRecord((byte)RecordType.EndAutopay, 1340, UserId_4)
            });

            mps.RecordsMap.Add(RecordType.Debit, debitTransactions);
            mps.RecordsMap.Add(RecordType.Credit, creditTransactions);
            mps.RecordsMap.Add(RecordType.StartAutopay, startAutopayTransactions);
            mps.RecordsMap.Add(RecordType.EndAutopay, endAutopayTransactions);
        }
    }
}
