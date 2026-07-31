using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EchoesOfAincrad_Save_Manager.Models
{
    public class SaveInfo
    {
        public ObservableCollection<SaveFile> BackupList { get; set; } = new ObservableCollection<SaveFile>();
    }

    public class SaveFile
    {
        public DateTime SaveTime { get; set; } = DateTime.MinValue;
        public DateTime BackupTime { get; set; } = DateTime.MinValue;
        public string File { get; set; } = string.Empty;

        public static SaveFile New(DateTime saveTime, DateTime backupTime, string file)
        {
            SaveFile info = new SaveFile();
            info.SaveTime = saveTime;
            info.BackupTime = backupTime;
            info.File = file;

            return info;
        }
    }
}
