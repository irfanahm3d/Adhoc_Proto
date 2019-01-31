using System;

namespace Adhoc.Proto
{
    class Program
    {
        static int Main(string[] args)
        {
            int exitCode = 0;
            MPS7Data data = new MPS7Data();
            try
            {
                data.LoadData();

                Console.WriteLine("Total amount (in dollars) of debits: $" + data.GetTotalDebitAmount());
                Console.WriteLine("Total amount (in dollars) of credits: $" + data.GetTotalCreditAmount());
                Console.WriteLine("Total number of autopays started: " + data.GetStartAutopayCount());
                Console.WriteLine("Total number of autopays ended: " + data.GetEndAutopayCount());
                Console.WriteLine("balance of user ID 2456938384156277127: $" + data.GetBalanceForUser(2456938384156277127));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                exitCode = -1;
            }

            Console.ReadKey();
            return exitCode;
        }
    }
}
