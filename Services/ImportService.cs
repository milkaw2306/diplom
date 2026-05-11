using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Dapper;

namespace Diplom_zxc.Services
{
    public class ImportService
    {
        private readonly string _connectionString;
        private readonly int _currentUserId;
        private static readonly string[] SupportedFormats = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        public event EventHandler<ImportProgressEventArgs>? ProgressChanged;
        public event EventHandler? ImportCompleted;

        public ImportService(string connectionString, int userId)
        {
            _connectionString = connectionString;
            _currentUserId = userId;
        }

        public async Task<int> ImportDraggedFiles(string[] filePaths, int targetFolderId)
        {
            int count = 0;
            for (int i = 0; i < filePaths.Length; i++)
            {
                if (await ImportSinglePhoto(filePaths[i], targetFolderId))
                {
                    count++;
                    ProgressChanged?.Invoke(this, new ImportProgressEventArgs
                    {
                        CurrentFile = Path.GetFileName(filePaths[i]),
                        ProcessedCount = count
                    });
                }
            }
            ImportCompleted?.Invoke(this, EventArgs.Empty);
            return count;
        }

        private async Task<bool> ImportSinglePhoto(string filePath, int folderId)
        {
            try
            {
                byte[] data = await File.ReadAllBytesAsync(filePath);
                var info = new FileInfo(filePath);

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                await conn.ExecuteAsync(
                    "INSERT INTO Photos (FolderId, UserId, FileName, OriginalName, FileData, FileSize) VALUES (@FolderId, @UserId, @FileName, @OriginalName, @FileData, @FileSize)",
                    new
                    {
                        FolderId = folderId,
                        UserId = _currentUserId,
                        FileName = Guid.NewGuid() + Path.GetExtension(filePath),
                        OriginalName = Path.GetFileName(filePath),
                        FileData = data,
                        FileSize = info.Length
                    });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> ImportFolderWithStructure(string folderPath, int? targetFolderId = null)
        {
            int count = 0;
            var files = Directory.GetFiles(folderPath)
                .Where(f => SupportedFormats.Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            for (int i = 0; i < files.Count; i++)
            {
                if (await ImportSinglePhoto(files[i], targetFolderId ?? 0))
                {
                    count++;
                    ProgressChanged?.Invoke(this, new ImportProgressEventArgs
                    {
                        CurrentFile = Path.GetFileName(files[i]),
                        ProcessedCount = count
                    });
                }
            }

            ImportCompleted?.Invoke(this, EventArgs.Empty);
            return count;
        }
    }

    public class ImportProgressEventArgs : EventArgs
    {
        public string? CurrentFile { get; set; }
        public int ProcessedCount { get; set; }
    }
}