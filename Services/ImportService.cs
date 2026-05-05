using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MySql.Data.MySqlClient;
using Dapper;

namespace Diplom_zxc.Services
{
    public class ImportService
    {
        private readonly string _connectionString;
        private readonly int _currentUserId;
        private static readonly string[] SupportedFormats = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" };

        public event EventHandler<ImportProgressEventArgs>? ProgressChanged;
        public event EventHandler? ImportCompleted;

        public ImportService(string connectionString, int userId)
        {
            _connectionString = connectionString;
            _currentUserId = userId;
        }

        public async Task<int> ImportFolderWithStructure(string sourceFolderPath, int? targetFolderId = null)
        {
            try
            {
                long folderSize = GetFolderSize(sourceFolderPath);
                if (!await HasEnoughStorage(folderSize))
                {
                    System.Windows.MessageBox.Show("Недостаточно места в хранилище!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return 0;
                }

                string rootFolderName = Path.GetFileName(sourceFolderPath);
                int rootFolderId = await CreateFolder(rootFolderName, targetFolderId);
                int totalPhotos = await ImportDirectoryRecursive(sourceFolderPath, rootFolderId);

                await UpdateUserStorage();
                return totalPhotos;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private async Task<int> ImportDirectoryRecursive(string sourcePath, int parentFolderId)
        {
            int importedCount = 0;
            var photoFiles = Directory.GetFiles(sourcePath).Where(f => SupportedFormats.Contains(Path.GetExtension(f).ToLower()));

            foreach (var photoPath in photoFiles)
            {
                if (await ImportSinglePhoto(photoPath, parentFolderId))
                {
                    importedCount++;
                    ProgressChanged?.Invoke(this, new ImportProgressEventArgs { CurrentFile = Path.GetFileName(photoPath), ProcessedCount = importedCount });
                }
            }

            foreach (var subDir in Directory.GetDirectories(sourcePath))
            {
                string folderName = Path.GetFileName(subDir);
                int newFolderId = await CreateFolder(folderName, parentFolderId);
                importedCount += await ImportDirectoryRecursive(subDir, newFolderId);
            }

            return importedCount;
        }

        private async Task<bool> ImportSinglePhoto(string filePath, int folderId)
        {
            try
            {
                using var image = await Image.LoadAsync(filePath);
                var fileInfo = new FileInfo(filePath);
                byte[] originalData = await File.ReadAllBytesAsync(filePath);
                byte[] thumbnailData = await CreateThumbnail(image);

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                await connection.ExecuteAsync(
                    @"INSERT INTO Photos (FolderId, UserId, FileName, OriginalName, FileData, ThumbnailData, FileSize, Width, Height) 
                      VALUES (@FolderId, @UserId, @FileName, @OriginalName, @FileData, @ThumbnailData, @FileSize, @Width, @Height)",
                    new
                    {
                        FolderId = folderId,
                        UserId = _currentUserId,
                        FileName = $"{Guid.NewGuid()}{Path.GetExtension(filePath)}",
                        OriginalName = Path.GetFileName(filePath),
                        FileData = originalData,
                        ThumbnailData = thumbnailData,
                        FileSize = fileInfo.Length,
                        Width = image.Width,
                        Height = image.Height
                    });

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<byte[]> CreateThumbnail(Image image)
        {
            using var thumbnail = image.Clone(ctx => ctx.Resize(new ResizeOptions { Size = new Size(300, 300), Mode = ResizeMode.Max }));
            using var ms = new MemoryStream();
            await thumbnail.SaveAsJpegAsync(ms);
            return ms.ToArray();
        }

        private async Task<int> CreateFolder(string folderName, int? parentFolderId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.ExecuteScalarAsync<int>(
                "INSERT INTO Folders (UserId, ParentFolderId, FolderName) VALUES (@UserId, @ParentFolderId, @FolderName); SELECT LAST_INSERT_ID();",
                new { UserId = _currentUserId, ParentFolderId = parentFolderId, FolderName = folderName });
        }

        public async Task<int> ImportDraggedFiles(string[] filePaths, int targetFolderId)
        {
            int importedCount = 0;
            foreach (var path in filePaths)
            {
                if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                    importedCount += await ImportFolderWithStructure(path, targetFolderId);
                else if (SupportedFormats.Contains(Path.GetExtension(path).ToLower()))
                    if (await ImportSinglePhoto(path, targetFolderId)) importedCount++;
            }
            await UpdateUserStorage();
            ImportCompleted?.Invoke(this, EventArgs.Empty);
            return importedCount;
        }

        private async Task<bool> HasEnoughStorage(long requiredSpace)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var user = await connection.QueryFirstOrDefaultAsync<dynamic>("SELECT StorageLimit, StorageUsed FROM Users WHERE UserId = @UserId", new { UserId = _currentUserId });
            return user != null && (user.StorageLimit - user.StorageUsed) >= requiredSpace;
        }

        private async Task UpdateUserStorage()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await connection.ExecuteAsync("UPDATE Users SET StorageUsed = (SELECT COALESCE(SUM(FileSize), 0) FROM Photos WHERE UserId = @UserId) WHERE UserId = @UserId", new { UserId = _currentUserId });
        }

        private long GetFolderSize(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => SupportedFormats.Contains(Path.GetExtension(f).ToLower()))
                .Sum(f => new FileInfo(f).Length);
        }
    }

    public class ImportProgressEventArgs : EventArgs
    {
        public string? CurrentFile { get; set; }
        public int ProcessedCount { get; set; }
    }
}