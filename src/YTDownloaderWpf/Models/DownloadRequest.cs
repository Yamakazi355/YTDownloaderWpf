using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YtDownloaderWpf.Models
{
    public class DownloadRequest
    {
        public string Url { get; set; }

        public DownloadMode Mode { get; set; }

        public string OutputFormat { get; set; }

        public string Quality { get; set; }
    }
}
