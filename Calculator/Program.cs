
using System.Data;

namespace Calculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string result = null;
            Calculator calculator;
            try
            {
                if (args.Length == 0)
                {
                    args = SetArgs();
                }
                calculator = new Calculator(args);
                result = calculator.Calculate();
                Console.WriteLine(result);
                Console.WriteLine("Press any key...");
                Console.ReadLine();
            }
            catch (FileEmptyException ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
            catch (InvalidExpressionException ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found");
            }
            catch (IOException)
            {
                Console.WriteLine("An I/O error occurred while reading a file");
            }
        }

        public static string[] SetArgs()
        {
            byte mode = 0;
            string[] args;

            Console.WriteLine("Select mode:");
            Console.WriteLine("1 - Calculate line");
            Console.WriteLine("2 - Calculate file (only data file path)");
            Console.WriteLine("3 - Calculate file (data file path + result file path)");

            mode = byte.Parse(Console.ReadLine());

            switch (mode)
            {
                case 1:
                    args = new string[1];
                    Console.WriteLine("Enter expression:");
                    args[0] = Console.ReadLine();
                    break;
                case 2:
                    args = new string[1];
                    Console.WriteLine("Enter path to data file:");
                    args[0] = Console.ReadLine();
                    break;
                case 3:
                    args = new string[2];
                    Console.WriteLine("Enter path to data file:");
                    args[0] = Console.ReadLine();
                    Console.WriteLine("Enter path to result file:");
                    args[1] = Console.ReadLine();
                    break;
                default:
                    args = new string[1];
                    Console.WriteLine("Wrong mode");
                    break;
            }
            return args;
        }
    }
}
