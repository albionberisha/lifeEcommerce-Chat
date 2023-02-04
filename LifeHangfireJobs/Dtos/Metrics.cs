namespace LifeHangfireJobs.Dtos
{
    public class Metrics
    {
        public int MostSoldProduct { get; set; }
        public double MostExpensiveOrder { get; set; }
        public double CheapestOrder { get; set; }
        public DateOnly BestDay { get; set; }

    }
}
