using System;
using System.Collections.Generic;
using System.Text;

namespace LotteryStatistics.Models
{
    public  class EurojackpotDraw : LotteryDraw
    {
        public List<int> MainNumbers { get; set; } = [];
        public List<int> EuroNumbers { get; set; } = [];
    }
}
