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
        private static string _settingsFile = "Settings.json";
        private static string _saveFile = "SaveData.sav";
        private static string _backupSaveFileFormat = "SaveData.sav.{0}";

        private readonly string _profilesFile = "Profiles.json";

        private string _profileFilePath = string.Empty;
        private Profiles? _profiles;

        private System.Windows.Threading.DispatcherTimer? _timer = null;

        private SaveInfo _saveInfo = new();
        private Settings _settings;

        public MainWindow()
        {
            InitializeComponent();

            _settings = Settings.FromFile(_settingsFile);

            DataContext = _saveInfo;

            if (!Directory.Exists(_settings.DataDir))
                Directory.CreateDirectory(_settings.DataDir);

            LoadProfiles();
            PopulateProfileList();
            LoadBackups();
            AssignButtons();
        }

        private void PopulateProfileList()
        {
            foreach (var dir in Directory.GetDirectories(_settings.DataDir))
            {
                ProfileList.Items.Add(dir.Split('\\').Last());
            }

            ProfileList.SelectedItem = GetActiveProfile();
        }

        private void AssignButtons()
        {
            StartButton.Click += StartButton_Click;
            StopButton.Click += StopButton_Click;
            ForceBackupButton.Click += ForceBackupButton_Click;

            ProfileList.SelectionChanged += ProfileList_SelectionChanged;
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
            _profileFilePath = Path.Combine(_settings.DataDir, _profilesFile);

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
            var backupInterval = _profiles is not null ? _settings.BackupInterval : Settings.DefaultBackupInterval;

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
                if (file.Contains(_saveFile))
                    _saveInfo.BackupList.Add(SaveFile.New(File.GetLastWriteTime(file).ToString(), file));
            }
        }

        private void BackupSave()
        {
            var savePath = Path.Combine(_settings.GameSavesPath, _saveFile);
            var path = Path.Combine(GetActiveProfileDir(), _backupSaveFileFormat);
            var maxSaves = _profiles is not null ? _settings.MaxSaves : Settings.DefaultMaxSaves;

            for (int i = 1; i <= maxSaves; i++)
            {
                var newFile = string.Format(path, i.ToString());

                if (File.Exists(newFile))
                    continue;

                File.Copy(savePath, newFile);
                _saveInfo.BackupList.Add(SaveFile.New(File.GetLastWriteTime(newFile).ToString(), newFile));
                return;
            }

            var one = string.Format(path, "1");
            File.Delete(one);
            SaveFile? oneSav = null;

            foreach (var save in _saveInfo.BackupList)
            {
                if (!save.File.Equals(_saveFile))
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
            _saveInfo.BackupList.Add(SaveFile.New(File.GetLastWriteTime(maxFile).ToString(), maxFile));
        }

        private void CreateDefaultProfiles()
        {
            _profiles = new();
            File.WriteAllText(_profileFilePath, JsonConvert.SerializeObject(_profiles, Formatting.Indented));
            Directory.CreateDirectory(GetActiveProfileDir());
        }

        private string GetActiveProfile() => _profiles is not null ? _profiles.ActiveProfile : Profiles.DefaultProfile;

        private string GetActiveProfileDir()
        {
            var dir = Path.Join(_settings.DataDir, GetActiveProfile());

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }
    }
}