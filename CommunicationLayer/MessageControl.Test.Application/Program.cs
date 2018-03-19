using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageControl.Controllers;

namespace MessageControl.Test.Application
{
    class Program
    {
        static void Main(string[] args)
        {

            var input = new Input("192.168.1.236", 10000);
            var mess = new MessageSystem(input);

            mess.Attach("Mother", sup);

            input.Start();
            Console.ReadKey();

        }

        public static string sup(string x, string y)
        {
            return null;
        }

    }
}
