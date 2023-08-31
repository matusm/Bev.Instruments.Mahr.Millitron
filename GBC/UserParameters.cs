using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    public class UserParameters
    {
        public UserParameters()
        {

        }

        public void QueryUserInput()
        {

        }

        private string PromptForText(string message)
        {
            Console.Write(message);
            string input = Console.ReadLine();
            return input.Trim();
        }

        private double PromptForNumber(string message)
        {
            string input = PromptForText(message);
            return ParseNumberTolerant(input);
        }

        private double ParseNumberTolerant(string input)
        {
            input = input.Replace(",", ".");
            double result = double.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
            return result;
        }
    }
}
