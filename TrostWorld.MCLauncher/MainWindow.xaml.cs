using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.Installers;
using CmlLib.Core.ProcessBuilder;
using Path = System.IO.Path;

namespace TrostWorld.MCLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            nickname.Text = "TrostNik";
        }
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = nickname.Text;
            var version = "1.12.2";
            var forge_version = "14.23.5.2859";
            string mcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".trostworld");
            string ramText = maxram.Text;
            int ramCount = int.Parse(ramText);
            
            
            Directory.CreateDirectory(mcPath);
            var path = new MinecraftPath(mcPath); 
            var launcher = new MinecraftLauncher(path);
 
            var fileProgress = new SyncProgress<InstallerProgressChangedEventArgs>(v =>
                Console.WriteLine($"[{v.EventType}][{v.ProgressedTasks}/{v.TotalTasks}] {v.Name}"));
            
            var byteProgress = new SyncProgress<ByteProgress>(v =>
                Console.WriteLine(v.ToRatio() * 100 + "%"));
            
            var installerOutput = new SyncProgress<string>(v =>
                Console.WriteLine(v));

            var forge = new ForgeInstaller(launcher);
            
            var version_name = await forge.Install(version, forge_version, new ForgeInstallOptions
            {
                FileProgress = fileProgress,
                ByteProgress = byteProgress,
                InstallerOutput = installerOutput,
            });

            //Start Minecraft
            var launchOption = new MLaunchOption
            {
                MaximumRamMb = ramCount,
                Session = MSession.CreateOfflineSession(username),
            };
            
            var process = await launcher.InstallAndBuildProcessAsync(version_name, launchOption);

            var processUtil = new ProcessWrapper(process);
            processUtil.OutputReceived += (s, v) => Console.WriteLine(v);
            processUtil.StartWithEvents();
            await processUtil.WaitForExitTaskAsync();
        }
    }
}
