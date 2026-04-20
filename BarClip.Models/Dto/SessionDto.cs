using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarClip.Models.Dto
{
    public class SessionDto
    {
        public required Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Title { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
