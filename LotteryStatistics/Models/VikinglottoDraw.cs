using System;
using System.Collections.Generic;
using System.Text;

namespace LotteryStatistics.Models
{
    public class VikinglottoDraw: LotteryDraw
    {
        public List<int> MainNumbers { get; set; } = [];
        public int VikingNumber { get; set; }
    }
}
