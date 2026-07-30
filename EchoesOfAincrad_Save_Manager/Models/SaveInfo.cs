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
        public string Time { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;

        public static SaveFile New(string time,  string file)
        {
            SaveFile info = new SaveFile();
            info.Time = time;
            info.File = file;

            return info;
        }
    }
}
