namespace ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var reader = new DataReader();
            reader.ImportAndPrintData("data.csv");
        }
    }
}
