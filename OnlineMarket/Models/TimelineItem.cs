using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMarket.Models
{
   
        public class TimelineItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime? Date { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsCurrent { get; set; }
            public string Icon { get; set; }
        }
    
}