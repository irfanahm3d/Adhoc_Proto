using System;

namespace Adhoc_Proto
{
    class Program
    {
        static void Main(string[] args)
        {
            MPS7Data data = new MPS7Data();
            data.LoadData();
            Console.WriteLine("Total amount (in dollars) of debits: $" + data.GetTotalDebitAmount());
            Console.WriteLine("Total amount (in dollars) of credits: $" + data.GetTotalCreditAmount());
            Console.WriteLine("Total number of autopays started: " + data.GetStartAutopayCount());
            Console.WriteLine("Total number of autopays ended: " + data.GetEndAutopayCount());
            Console.WriteLine("balance of user ID 2456938384156277127: $" + data.GetBalanceForUser(2456938384156277127));
            Console.ReadKey();
        }
    }
}
