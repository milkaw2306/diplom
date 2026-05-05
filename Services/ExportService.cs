using MySql.Data.MySqlClient;
using Dapper;

namespace Diplom_zxc.Services
{
    public class ExportService
    {
        private readonly string _connectionString;

        public ExportService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> ExportPhotos(List<int> photoIds, string exportPath)
        {
            try
            {
                if (!Directory.Exists(exportPath))
                    Directory.CreateDirectory(exportPath);

                string datedFolder = Path.Combine(exportPath, $"PhotoExport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
                Directory.CreateDirectory(datedFolder);

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                foreach (int photoId in photoIds)
                {
                    var photo = await connection.QueryFirstOrDefaultAsync<Models.Photo>(
                        "SELECT * FROM Photos WHERE PhotoId = @PhotoId", new { PhotoId = photoId });

                    if (photo?.FileData != null && photo.OriginalName != null)
                    {
                        string exportFileName = Path.Combine(datedFolder, photo.OriginalName);
                        await File.WriteAllBytesAsync(exportFileName, photo.FileData);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExportFolder(int folderId, string exportPath)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var photoIds = await connection.QueryAsync<int>(
                "SELECT PhotoId FROM Photos WHERE FolderId = @FolderId", new { FolderId = folderId });

            return await ExportPhotos(photoIds.ToList(), exportPath);
        }
    }
}