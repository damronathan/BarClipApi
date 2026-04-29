using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarClipApi.Models.Requests
{
    public class VideoRequest
    {
        public string UserId { get; set; }
        public Guid VideoId { get; set; }
        public Guid SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OrderNumber { get; set; }
        public bool IsFull { get; set; }
    }

}
