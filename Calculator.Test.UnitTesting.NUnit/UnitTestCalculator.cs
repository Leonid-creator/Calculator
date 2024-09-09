using Moq;
using System.Data;

namespace Calculator.Test.UnitTesting.NUnit
{
    public class Tests
    {
        [TestCase("2+2")]
        [TestCase("3*(-10)")]
        [TestCase("2.5*2")]
        [TestCase("1+2-3/4*5")]
        [TestCase("2.5*2")]
        public void Calculate_ValidExpression_CallCalculateLine_Once(string expression)
        {
            string[] args = { expression };
            object[] objArgs = new object[] { args };
            var calculatorMock = new Mock<Calculator>(objArgs);
            string result = calculatorMock.Object.Calculate();
            calculatorMock.Verify(m => m.CalculateLine(expression, 0), Times.Once);
        }

        [TestCase(@"TestData\TestFile.txt")]
        public void Calculate_Path_CallCalculateFile_Once(string path)
        {
            string[] args = { path, @"TestData\ResTestFile.txt" };
            object[] objArgs = new object[] { args };
            var calculatorMock = new Mock<Calculator>(objArgs);
            string result = calculatorMock.Object.Calculate();
            calculatorMock.Verify(m => m.CalculateFile(), Times.Once);
        }

        [Test]
        public void CalculateFile_Path_ResultFile()
        {
            string dataFilePath = @"TestData\TestFile.txt";
            string resultFilePath = @"TestData\result_TestFile.txt";
            string[] args = { dataFilePath, resultFilePath };
            Calculator calculator = new Calculator(args);
            calculator.CalculateFile();

            using (StreamReader readerResult = new StreamReader(@"TestData\result_TestFile.txt"))
            using (StreamReader readerCheck = new StreamReader(@"TestData\PredefinedResult.txt"))
            {
                string result = readerResult.ReadLine();
                string check = readerCheck.ReadLine();
                while (result != null)
                {
                    Assert.That(result, Is.EqualTo(check));
                    result = readerResult.ReadLine();
                    check = readerCheck.ReadLine();
                }
            }
        }

        [TestCase("13+7", ExpectedResult = "20")]
        [TestCase("3*(-10)", ExpectedResult = "-30")]
        [TestCase("41-1", ExpectedResult = "40")]
        [TestCase("100/2", ExpectedResult = "50")]
        [TestCase("2.5*2", ExpectedResult = "5")]
        public string CalculateLine_SimpleExpression_Result(string expression)
        {
            string[] emptyArray = { string.Empty };
            Calculator calculator = new Calculator(emptyArray);
            return calculator.CalculateLine(expression);
        }

        [TestCase("(6+6)-5", ExpectedResult = "12-5")]
        [TestCase("1+2-3/4*5", ExpectedResult = "1+2-3/20")]
        [TestCase("1+2-3/2", ExpectedResult = "1+2-1.5")]
        [TestCase("1+2-3", ExpectedResult = "3-3")]
        [TestCase("1-2+3", ExpectedResult = "-1+3")]
        [TestCase("5*(5.5-10)", ExpectedResult = "5*-4.5")]
        public string Simplify_ComplexExpression_SimpleExpression(string expression)
        {
            string[] emptyArray = { string.Empty };
            Calculator calculator = new Calculator(emptyArray);
            return calculator.Simplify(expression);
        }

        [TestCase("((6+6) * 3")]
        [TestCase("(4 + 2) * 3)")]
        [TestCase("a * 7")]
        [TestCase("#5 + 8")]
        [TestCase("3 * + 5")]
        [TestCase("2 + * 3")]
        [TestCase("4 +")]
        [TestCase("2.5.7")]
        [TestCase("a 5 b")]
        [TestCase("5 @ 3")]
        public void ExpressionIsValid_InvalidExpression_CatchException(string expression)
        {
            string[] emptyArray = { string.Empty };
            Calculator calculator = new Calculator(emptyArray);
            Assert.Throws<InvalidExpressionException>(() => calculator.CalculateLine(expression));
        }
    }
}