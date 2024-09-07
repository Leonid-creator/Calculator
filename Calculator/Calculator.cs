using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace Calculator
{
    public class Calculator
    {
        public byte Mode;
        public string Expression;
        public string DataFilePath;
        public string ResultFilePath;

        private readonly Regex InvalidDoubleOperatorExpr = new(@"(\+\s*){2,}|(-\s*){2,}|(\*\s*){2,}|(/\s*){2,}");
        private readonly Regex InvalidCharacterExpr = new(@"[^\d\+\-\*\/\(\)\.\s]");
        private readonly Regex InvalidExtraDotExpr = new(@"\d+\.\d+\.+");
        private readonly Regex CorrectExpression = new(@"\(?\-?\d+(\.\d+)?\)?\s*[\+\-\*\/]\s*\(?\-?\d+(\.\d+)?\)?|^\(?\-?\d+(\.\d+)?\)?$");

        private readonly Regex ZeroDecimalPartExpr = new(@"\.[0]+$");
        private readonly Regex SingleOperandExpr = new(@"^\-?\d+(\.\d+)?$");
        private readonly Regex DataFileNameExpr = new(@"(?<=\\)[A-Za-z0-9]+(?=\.[A-Za-z]+)");
        private readonly Regex BracketsExpr = new(@"\([^()]*\)");
        private readonly Regex MultiplyExpr = new(@"\-?\d+(\.\d+)?\s*[\*]\s*\-?\d+(\.\d+)?");
        private readonly Regex DivideExpr = new(@"\-?\d+(\.\d+)?\s*[\/]\s*\-?\d+(\.\d+)?");
        private readonly Regex PlusMinusExpr = new(@"\-?\d+(\.\d+)?\s*[\+\-]\s*\-?\d+(\.\d+)?");
        private readonly Regex SimpleExpression = new(@"^\(?(?<operand_1>\-?\d+(\.\d+)?)\)?\s*(?<operator>[\+\-\*\/])\s*\(?(?<operand_2>\-?\d+(\.\d+)?)\)?$");

        public Calculator(string[] args)
        {
            if (args.Length == 1)
            {
                if (CorrectExpression.IsMatch(args[0]))
                {
                    Expression = args[0];
                    Mode = 1;
                }
                if (DataFileNameExpr.IsMatch(args[0]))
                {
                    DataFilePath = args[0];
                    string fileName = Path.GetFileNameWithoutExtension(DataFilePath);
                    ResultFilePath = DataFileNameExpr.Replace(DataFilePath, $"result_{fileName}");
                    Mode = 2;
                }
            }
            else if (args.Length == 2 && DataFileNameExpr.IsMatch(args[0]))
            {
                if (args[0] == args[1])
                {
                    throw new InvalidExpressionException("ResultFilePath and DataFilePath can`t be the same!");
                }
                else
                {
                    DataFilePath = args[0];
                    ResultFilePath = args[1];
                    Mode = 2;
                }
            }
            else
            {
                throw new InvalidExpressionException("Сould not determine argument type!");
            }
        }

        public string Calculate()
        {
            if (Mode == 1)
            {
                return CalculateLine(Expression);
            }
            else if (Mode == 2)
            {
                CalculateFile();
                return $"The result file was created at the path {ResultFilePath}";
            }
            else
            {
                return "Failed to determine mode!";
            }
        }

        public virtual void CalculateFile()
        {
            if (!File.Exists(DataFilePath))
            {
                throw new FileNotFoundException();
            }
            else
            {
                using (StreamReader reader = new StreamReader(DataFilePath))
                using (StreamWriter writer = new StreamWriter(ResultFilePath))
                {
                    int lineCount = 0;
                    string dataLine = string.Empty;
                    if ((dataLine = reader.ReadLine()) == null)
                    {
                        throw new FileEmptyException();
                    }
                    else
                    {
                        do
                        {
                            try
                            {
                                writer.WriteLine(CalculateLine(dataLine, lineCount));
                            }
                            catch
                            {
                                writer.WriteLine("Invalid data line");
                            }
                            lineCount++;
                        } while ((dataLine = reader.ReadLine()) != null);
                    }
                }
            }
        }

        public string Simplify(string inputExpr)
        {
            string oldExpr = inputExpr;
            string replacement;
            string toCalc;
            if (BracketsExpr.IsMatch(inputExpr))
            {
                toCalc = BracketsExpr.Match(inputExpr).ToString();
                toCalc = toCalc.Trim('(', ')');
                replacement = CalculateLine(toCalc);
                inputExpr = BracketsExpr.Replace(inputExpr, replacement, 1);
            }
            else if (MultiplyExpr.IsMatch(inputExpr))
            {
                toCalc = MultiplyExpr.Match(inputExpr).ToString();
                inputExpr = MultiplyExpr.Replace(inputExpr, CalculateLine(toCalc), 1);
            }
            else if (DivideExpr.IsMatch(inputExpr))
            {
                toCalc = DivideExpr.Match(inputExpr).ToString();
                inputExpr = DivideExpr.Replace(inputExpr, CalculateLine(toCalc), 1);
            }
            else if (PlusMinusExpr.IsMatch(inputExpr))
            {
                toCalc = PlusMinusExpr.Match(inputExpr).ToString();
                inputExpr = PlusMinusExpr.Replace(inputExpr, CalculateLine(toCalc), 1);
            }
            if (oldExpr == inputExpr)
            {
                throw new InvalidExpressionException($"Failed to simplify expression \"{inputExpr}\"");
            }
            return inputExpr;
        }

        public virtual string CalculateLine(string inputExpr, int line = 0)
        {
            ExpressionIsValid(inputExpr, line);
            decimal result = 0;
            while (!SimpleExpression.IsMatch(inputExpr))
            {
                if (SingleOperandExpr.IsMatch(inputExpr))
                {
                    return inputExpr;
                }
                inputExpr = Simplify(inputExpr);
            }
            Match match = SimpleExpression.Match(inputExpr);
            GroupCollection groups = match.Groups;
            string action = groups["operator"].ToString();
            decimal x = decimal.Parse(groups["operand_1"].ToString());
            decimal y = decimal.Parse(groups["operand_2"].ToString());

            switch (action)
            {
                case "+":
                    result = x + y;
                    break;
                case "-":
                    result = x - y;
                    break;
                case "*":
                    result = x * y;
                    break;
                case "/":
                    result = x / y;
                    break;
            }
            string strResult = result.ToString();
            strResult = ZeroDecimalPartExpr.Replace(strResult, string.Empty);
            return strResult;
        }

        private bool ExpressionIsValid(string expression, int line)
        {
            int bracketBalance = 0;
            line++;
            if (expression.Length == 0)
            {
                throw new InvalidExpressionException($"Error (line {line}): Line is empty");
            }
            for (int i = 0; i < expression.Length; i++)
            {
                if (expression[i] == '(')
                {
                    bracketBalance++;
                }
                else if (expression[i] == ')')
                {
                    bracketBalance--;
                }
                if (bracketBalance < 0)
                {
                    throw new InvalidExpressionException($"Error (line {line}): Extra closed bracket at position {i + 1}!");
                }
            }
            if (bracketBalance > 0)
            {
                throw new InvalidExpressionException($"Error (line {line}): Extra {bracketBalance} open brackets");
            }
            if (InvalidDoubleOperatorExpr.IsMatch(expression))
            {
                throw new InvalidExpressionException($"Error (line {line}): Detected double operator");
            }
            if (InvalidCharacterExpr.IsMatch(expression))
            {
                throw new InvalidExpressionException($"Error (line {line}): Detected invalid character");
            }
            if (InvalidExtraDotExpr.IsMatch(expression))
            {
                throw new InvalidExpressionException($"Error (line {line}): Extra dot");
            }
            if (!CorrectExpression.IsMatch(expression))
            {
                throw new InvalidExpressionException($"Error (line {line}): Any correct expression is not detected");
            }
            return true;
        }
    }
}