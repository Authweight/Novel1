using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelBuilder
{
    public record Manifest
    {
        public string Title { get; init; }
        public string Author { get; init; }
        public string Cover { get; set; }
        public List<Chapter> Chapters { get; init; }
    }

    public record Chapter
    {
        public string Title { get; init; }
        public string File { get; init; }
    }
}
