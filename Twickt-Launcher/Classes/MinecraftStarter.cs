﻿// Copyright (c) 2016 Twickt / Ceschia Davide
//Application idea, code and time are given by Davide Ceschia / Twickt
//You may use them according to the GNU GPL v.3 Licence
//GITHUB Project: https://github.com/killpowa/Twickt-Launcher
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Twickt_Launcher.Classes
{
    class MinecraftStarter
    {
        public static Dialogs.ModpackLoading loading;
        public static List<string> downloadingVersion;
        public static async void Minecraft_Start(string modpackname, bool remote)
        {
            loading = new Dialogs.ModpackLoading(true, Pages.SplashScreen.singleton.manager.GetString("starting") + "...");
            MaterialDesignThemes.Wpf.DialogHost.Show(loading, "RootDialog", OpenEvent);
            loading.forgeProgress.Value = 15;
            string gamedir = "";
            string getForge = "";
            string gamedirectory = "";
            bool forge;
            List<String[]> urlsforge = new List<string[]>();
            List<String[]> urlslibraries = new List<string[]>();
            if (remote == true)
            {
                downloadingVersion = await RemoteModpacks.GetMinecraftUrlsAndData(modpackname);
                gamedir = await Classes.RemoteModpacks.GetModpacksDir(modpackname);
                getForge = await Classes.RemoteModpacks.IsModpackForgeNeeded(modpackname);
                if (getForge == "false")
                    forge = false;
                else
                    forge = true;

                gamedirectory = ((forge == true) ? "\"" + config.M_F_P + "\\" + downloadingVersion[1] + "\\instances\\" + gamedir + "\" " : "\"" + config.M_F_P + "\\" + downloadingVersion[1] + "\" ");
                urlsforge = await JSON.GetFiles(modpackname, false, true);
                urlslibraries = await JSON.GetFiles(modpackname, true, false);
            }
            else
            {
                downloadingVersion = await LocalModpacks.GetMinecraftUrlsAndData(modpackname);
                gamedir = modpackname;
                getForge = downloadingVersion[2];
                if (getForge == "false")
                    forge = false;
                else
                    forge = true;

                gamedirectory = "\"" + config.LocalModpacks + modpackname + "\" ";

                urlsforge = await JSON.GetFiles(modpackname, false, true, false);
                urlslibraries = await JSON.GetFiles(modpackname, true, false, false);
            }

            
            string launch =@"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump -Xmx" + Properties.Settings.Default["RAM"] + "G -Xms" + "256" + "M " + ((String.Compare(Properties.Settings.Default["RAM"].ToString(), "3") > 0) ? "-XX:+DisableExplicitGC -XX:+UseConcMarkSweepGC -XX:+UseParNewGC -XX:+UseNUMA -XX:+CMSParallelRemarkEnabled -XX:MaxTenuringThreshold=15 -XX:MaxGCPauseMillis=30 -XX:GCPauseIntervalMillis=150 -XX:+UseAdaptiveGCBoundary -XX:-UseGCOverheadLimit -XX:+UseBiasedLocking -XX:SurvivorRatio=8 -XX:TargetSurvivorRatio=90 -XX:MaxTenuringThreshold=15 -Dfml.ignorePatchDiscrepancies=true -Dfml.ignoreInvalidMinecraftCertificates=true -XX:+UseFastAccessorMethods -XX:+UseCompressedOops -XX:+OptimizeStringConcat -XX:+AggressiveOpts -XX:ReservedCodeCacheSize=2048m -XX:+UseCodeCacheFlushing -XX:SoftRefLRUPolicyMSPerMB=10000 -XX:ParallelGCThreads=10 " : "-XX:+DisableExplicitGC -XX:+UseConcMarkSweepGC -XX:+UseParNewGC -XX:+UseNUMA -XX:+CMSParallelRemarkEnabled -XX:MaxTenuringThreshold=15 -XX:MaxGCPauseMillis=30 -XX:GCPauseIntervalMillis=150 -XX:+UseAdaptiveGCBoundary -XX:-UseGCOverheadLimit -XX:+UseBiasedLocking -XX:SurvivorRatio=8 -XX:TargetSurvivorRatio=90 -XX:MaxTenuringThreshold=15 -Dfml.ignorePatchDiscrepancies=true -Dfml.ignoreInvalidMinecraftCertificates=true -XX:+UseFastAccessorMethods -XX:+UseCompressedOops -XX:+OptimizeStringConcat -XX:+AggressiveOpts -XX:ReservedCodeCacheSize=2048m -XX:+UseCodeCacheFlushing -XX:SoftRefLRUPolicyMSPerMB=2000 -XX:ParallelGCThreads=10 ") +
                           "\"-Djava.library.path=" + config.M_F_P + downloadingVersion[1] + "\\\\natives-win\\\\\" " +

                           @"-cp ";
            if (forge == true)
                launch += "\"" + config.M_F_P + downloadingVersion[1] + @"\\libraries\net\minecraftforge\forge\" + config.forgeversion + @"\\forge-" + config.forgeversion + @".jar" + ";";
            else
                launch += "\"" + config.M_F_P + downloadingVersion[1] + @"\\versions\\" + downloadingVersion[0] + "\\\\" + downloadingVersion[0] + ".jar" + ";";
            if (forge == true)
            {
                foreach (string[] url in urlsforge)
                {
                    if (url[3].Contains("platform"))
                        continue;
                    if (url[3].Contains("https://libraries.minecraft.net"))
                    {
                        string dir = config.M_F_P + downloadingVersion[1] + @"\\libraries\\" + url[3].Replace("https://libraries.minecraft.net", "");
                        string FileName = Path.GetFileName(dir);
                        dir = Path.GetDirectoryName(@dir);
                        if (!Directory.Exists(@dir))
                        {
                            Directory.CreateDirectory(@dir);
                        }
                        launch = launch + ("" + @dir + "\\\\" + FileName + "" + ";");
                    }
                    if (url[3].Contains("http://search.maven.org/remotecontent?filepath="))
                    {
                        string dir = config.M_F_P + downloadingVersion[1] + @"\\libraries\\" + url[3].Replace("http://search.maven.org/remotecontent?filepath=", "");
                        string FileName = Path.GetFileName(dir);
                        dir = Path.GetDirectoryName(@dir);
                        if (!Directory.Exists(@dir))
                        {
                            Directory.CreateDirectory(@dir);
                        }
                        launch = launch + ("" + @dir + "\\\\" + FileName.Replace(".jar.pack.xz", ".jar") + "" + ";");
                    }
                }
            }
            foreach (string[] url in urlslibraries)
            {
                if (url[3].Contains("platform"))
                    continue;
                if (url[3].Contains("https://libraries.minecraft.net"))
                {
                    string dir = config.M_F_P + downloadingVersion[1] + @"\\libraries\\" + url[3].Replace("https://libraries.minecraft.net", "");
                    string FileName = Path.GetFileName(dir);
                    dir = Path.GetDirectoryName(@dir);
                    if (!Directory.Exists(@dir))
                    {
                        Directory.CreateDirectory(@dir);
                    }
                    launch = launch + ("" + @dir + "\\\\" + FileName + "" + ";");
                }
                if (forge == true)
                {
                    if (url[3].Contains("http://search.maven.org/remotecontent?filepath="))
                    {
                        string dir = config.M_F_P + downloadingVersion[1] + @"\\libraries\\" + url[3].Replace("http://search.maven.org/remotecontent?filepath=", "");
                        string FileName = Path.GetFileName(dir);
                        dir = Path.GetDirectoryName(@dir);
                        if (!Directory.Exists(@dir))
                        {
                            Directory.CreateDirectory(@dir);
                        }
                        launch = launch + ("" + @dir + "\\\\" + FileName.Replace(".jar.pack.xz", ".jar") + "" + ";");
                    }
                }
            }
            if (forge == true)
                launch += config.M_F_P + downloadingVersion[1] + @"\\versions\\" + downloadingVersion[0] + "\\\\" + downloadingVersion[0] + ".jar";


            if (forge == false)
            {
                launch = launch + "\" net.minecraft.client.main.Main " +
                               "--username " + SessionData.username + " " +
                               //"--accessToken " + MojangLogin.getAccessToken() + " " +
                               //"--username killpowa " +
                               "--accessToken 0 " +
                               "--version " + downloadingVersion[0] + (forge == true ? "-forge" + config.forgeversion : "") + " " +
                               "--gameDir " + gamedirectory +
                               "--assetsDir " + config.M_F_P + "\\" + downloadingVersion[1] + "\\assets\\ " +
                               "--assetIndex " + downloadingVersion[0] + " " +
                               "--userProperties {} " +
                               //"--uuid " + MojangLogin.getUUID() + " " +
                               "--uuid 0 ";
            }
            else
            {
                launch = launch + "\" " + config.mainclass + " " +
                    config.arguments.Replace(
                        "${auth_player_name}", SessionData.username
                        ).Replace(
                        "${version_name}", downloadingVersion[0] + "-forge" + config.forgeversion + " "
                        ).Replace(
                        "${game_directory}", gamedirectory
                        ).Replace(
                        "${assets_root}", config.M_F_P + "\\" + downloadingVersion[1] + "\\assets\\"
                        ).Replace(
                        "${assets_index_name}", downloadingVersion[0]
                        ).Replace(
                        "${auth_uuid}", "0"
                        ).Replace(
                        "${auth_access_token}", "0"
                        ).Replace(
                        "${user_properties}", "{}"
                        ).Replace(
                        "${user_type}", "legacy"
                        );
            }

            Process process = new Process();
            
            try
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = ComputerInfoDetect.GetJavaInstallationPath() + "//bin//java.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.Arguments = launch;
                Windows.DebugOutputConsole.singleton.Write(launch);
                loading.forgeProgress.Value = 25;

                process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    if (e.Data != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Windows.DebugOutputConsole.singleton.Write(e.Data.ToString());
                            if (e.Data.ToString().Contains("Loading tweak class name"))
                            {
                                loading.forgeProgress.Value = 50;
                            }
                            if(e.Data.ToString().Contains("Setting user:"))
                            {
                                loading.forgeProgress.Value = 75;
                            }
                            if (e.Data.ToString().Contains("LWJGL Version:"))
                            {
                                loading.forgeProgress.Value = 100;
                                Window1.singleton.Hide();
                            }

                        }));

                    }
                });
                process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    if (e.Data != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Windows.DebugOutputConsole.singleton.Write(e.Data.ToString());
                        }));
                    }
                });

                await Task.Run(() => process.Start());
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());
                Window1.singleton.Show();
            }
            catch(Exception e)
            {
                Windows.DebugOutputConsole.singleton.Write(Pages.SplashScreen.singleton.manager.GetString("fatalError") + " "  + e);
            }
        }

        private static async void OpenEvent(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
        {
            while(loading.forgeProgress.Value != 100)
            {
                await Task.Delay(2000);
            }
            try
            {
                eventArgs.Session.Close();
            }
            catch { }

        }
    }
}
