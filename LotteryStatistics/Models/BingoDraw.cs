using System;
using System.Collections.Generic;
using System.Text;

namespace LotteryStatistics.Models
{
    public class BingoDraw: LotteryDraw
    {
        public List<int> Numbers { get; set; } = [];
    }
}
