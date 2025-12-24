namespace dRz.SpecSpdsConsole
{
    internal class TestClass
    {
        public Statistics Stats { get; set; } = new Statistics();


    }
    public class Statistics
    {


        public string TmrID => _tmrID;
        public string _tmrID = "100";


    }
}
