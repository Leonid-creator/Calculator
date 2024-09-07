
namespace Calculator
{
    public class InvalidExpressionException : Exception
    {
        public InvalidExpressionException(string massage)
        {
            Console.WriteLine(massage);
        }
        public InvalidExpressionException()
        {

        }
    }
}
