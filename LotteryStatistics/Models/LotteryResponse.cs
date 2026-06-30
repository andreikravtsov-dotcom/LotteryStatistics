namespace LotteryStatistics.Models
{
    public class EestiLotoResponse
    {
        public List<DrawDto> Draws { get; set; } = [];
        public int DrawCount { get; set; }
        public int StatusCode { get; set; }
        public string? ErrorMsg { get; set; }
    }

    public class DrawDto
    {
        public int DrawId { get; set; }
        public string ExternalDrawId { get; set; } = "";
        public long DrawDate { get; set; }
        public string DrawLabel { get; set; } = "";
        public string GameTypeName { get; set; } = "";

        public List<ResultDto> Results { get; set; } = [];
    }

    public class ResultDto
    {
        public int? WinClass { get; set; }
        public string? WinningNumber { get; set; }
        public string? SecWinningNumber { get; set; } 
    }
}
