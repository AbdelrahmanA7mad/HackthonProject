using System;

namespace ManageMentSystem.Models
{
    public class BackupInfo
    {
        public string FileName { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FormattedSize => FormatFileSize(SizeInBytes);
        public string FormattedDate => CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}


