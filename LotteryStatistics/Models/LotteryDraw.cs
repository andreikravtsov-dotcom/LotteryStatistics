using System;
using System.Collections.Generic;
using System.Text;

namespace LotteryStatistics.Models
{
    public abstract class LotteryDraw
    {
        public int DrawId { get; set; }
        public int DrawLabel { get; set; }
        public DateTime DrawDate { get; set; }
    }
}
