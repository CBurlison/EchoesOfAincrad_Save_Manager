using EchoesOfAincrad_Save_Manager.Models;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace EchoesOfAincrad_Save_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SATTINGS_FILE = "Settings.json";
        private const string SAVE_FILE = "SaveData.sav";
        private const string BACKUP_SAVE_FILE_FORMAT = "SaveData.sav.{0}";
        private const string PROFILES_FILE = "Profiles.json";

        private string _profileFilePath = string.Empty;
        private Profiles? _profiles;

        private System.Windows.Threading.DispatcherTimer? _timer = null;

        private SaveInfo _saveInfo = new();
        private Settings _settings;

        public MainWindow()
        {
            InitializeComponent();

            _settings = Settings.FromFile(SATTINGS_FILE);

            DataContext = _saveInfo;

            if (!Directory.Exists(_settings.DataDir))
                Directory.CreateDirectory(_settings.DataDir);

            LoadProfiles();
            PopulateProfileList();
            LoadBackups();
            _saveInfo.BackupList = new(_saveInfo.BackupList.OrderByDescending(a => a.BackupTime));
        }

        private void PopulateProfileList()
        {
            foreach (var dir in Directory.GetDirectories(_settings.DataDir))
            {
                ProfileList.Items.Add(dir.Split('\\').Last());
            }

            ProfileList.SelectedItem = GetActiveProfile();
        }

        private void ProfileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProfileList.SelectedItem as string == _profiles?.ActiveProfile)
                return;

            MessageBoxResult result = MessageBox.Show("Change profile?", "Change profile", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
            {
                ProfileList.SelectedItem = _profiles?.ActiveProfile;
                return;
            }

            StopTimer();

            _profiles?.ActiveProfile = ProfileList.SelectedItem as string;
            _saveInfo.BackupList.Clear();

            LoadBackups();
        }

        private void LoadProfiles()
        {
            _profileFilePath = Path.Combine(_settings.DataDir, PROFILES_FILE);

            if (!File.Exists(_profileFilePath))
            {
                CreateDefaultProfiles();
            }
            else
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<Profiles>(File.ReadAllText(_profileFilePath));

                    if (result != null)
                        _profiles = result;
                    else
                        CreateDefaultProfiles();
                }
                catch (Exception)
                {
                    CreateDefaultProfiles();
                }
            }

            foreach (var profile in _profiles.Data)
            {
                var profilePath = Path.Combine(_settings.DataDir, profile);

                if (!Directory.Exists(profilePath))
                    Directory.CreateDirectory(profilePath);
            }
        }

        private void ForceBackupButton_Click(object sender, RoutedEventArgs e) => BackupSave();

        private void StopButton_Click(object sender, RoutedEventArgs e) => StopTimer();

        private void StopTimer()
        {
            if (_timer is null) return;

            _timer.Stop();
            _timer = null;

            TimerLabel.Content = "Stopped";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var backupInterval = _profiles is not null ? _settings.BackupInterval : Settings.DEFAULT_BACKUP_INTERVAL;

            _timer = new();
            _timer.Interval = new(0, 0, backupInterval);
            _timer.Tick += _timer_Elapsed;
            _timer.Start();

            TimerLabel.Content = "Running";
        }

        private void _timer_Elapsed(object? sender, EventArgs e) => BackupSave();

        private void LoadBackups()
        {
            foreach (var file in Directory.GetFiles(GetActiveProfileDir()))
            {
                if (file.Contains(SAVE_FILE))
                {
                    AddSaveToBackup(file);
                }
            }
        }

        private void AddSaveToBackup(string file)
        {
            var lastWrite = File.GetLastWriteTime(file);
            var creation = File.GetCreationTime(file);
            var saveFile = SaveFile.New(lastWrite, creation, file);

            _saveInfo.BackupList.Add(saveFile);
        }

        private void BackupSave()
        {
            var savePath = Path.Combine(_settings.GameSavesPath, SAVE_FILE);
            var path = Path.Combine(GetActiveProfileDir(), BACKUP_SAVE_FILE_FORMAT);
            var maxSaves = _profiles is not null ? _settings.MaxSaves : Settings.DEFAULT_MAX_SAVES;

            for (int i = 1; i <= maxSaves; i++)
            {
                var newFile = string.Format(path, i.ToString());

                if (File.Exists(newFile))
                    continue;

                File.Copy(savePath, newFile);

                AddSaveToBackup(newFile);
                return;
            }

            var one = string.Format(path, "1");
            File.Delete(one);
            SaveFile? oneSav = null;

            foreach (var save in _saveInfo.BackupList)
            {
                if (!save.File.Equals(SAVE_FILE))
                    continue;

                oneSav = save;
                break;
            }

            if (oneSav is not null)
                _saveInfo.BackupList.Remove(oneSav);

            for (int i = 2; i <= maxSaves; i++)
            {
                var oldFile = string.Format(path, i.ToString());
                var newFile = string.Format(path, (i-1).ToString());

                File.Move(oldFile, newFile);

                foreach (var save in _saveInfo.BackupList)
                    if (save.File.Equals(oldFile))
                    {
                        save.File = newFile;
                        break;
                    }
            }

            var maxFile = string.Format(path, maxSaves.ToString());
            File.Copy(savePath, maxFile);

            AddSaveToBackup(maxFile);
        }

        private void CreateDefaultProfiles()
        {
            _profiles = new();
            File.WriteAllText(_profileFilePath, JsonConvert.SerializeObject(_profiles, Formatting.Indented));
            Directory.CreateDirectory(GetActiveProfileDir());
        }

        private string GetActiveProfile() => _profiles is not null ? _profiles.ActiveProfile : Profiles.DEFAULT_PROFILE;

        private string GetActiveProfileDir()
        {
            var dir = Path.Join(_settings.DataDir, GetActiveProfile());

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }
    }
}