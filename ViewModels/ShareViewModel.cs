using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diplom_zxc.Models;
using Diplom_zxc.Services;

namespace Diplom_zxc.ViewModels
{
    public partial class ShareViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _shareCode = string.Empty;

        [ObservableProperty]
        private string _sharedItemName = string.Empty;

        [ObservableProperty]
        private bool _hasExpiry = true;

        [ObservableProperty]
        private bool _isLinkCreated;

        [ObservableProperty]
        private bool _isCreating;

        [ObservableProperty]
        private string _statusText = "Готов";

        private int? _folderId;
        private int? _photoId;

        public ShareViewModel()
        {
        }

        public void InitializeWithFolder(Folder folder)
        {
            _folderId = folder.FolderId;
            SharedItemName = folder.FolderName ?? "Папка";
        }

        public void InitializeWithPhoto(Photo photo)
        {
            _photoId = photo.PhotoId;
            SharedItemName = photo.OriginalName ?? "Фото";
        }

        [RelayCommand]
        private void CreateLink()
        {
            IsCreating = true;
            ShareCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            IsLinkCreated = true;
            IsCreating = false;
            StatusText = "Ссылка создана!";
        }

        [RelayCommand]
        private void CopyLink()
        {
            Clipboard.SetText(ShareCode);
            StatusText = "Скопировано!";
        }

        [RelayCommand]
        private void CloseWindow()
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w is Views.ShareDialog)
                    w.Close();
            }
        }
    }
}