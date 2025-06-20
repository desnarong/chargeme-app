using System;
using System.IO;

namespace OCPP.Core.Server
{
    public class CleanupOldLogFiles
    {
        public static void CleanupOldLog(string logDirectory, int retentionDays)
        {
            if (!Directory.Exists(logDirectory))
                return;

            var logFiles = Directory.GetFiles(logDirectory, "*.txt");
            foreach (var file in logFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTime < DateTime.UtcNow.AddDays(-retentionDays))
                {
                    fileInfo.Delete();
                }
            }
        }
    }
}
