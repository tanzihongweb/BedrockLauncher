﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BedrockLauncher.Classes;
using BedrockLauncher.UpdateProcessor.Enums;
using BedrockLauncher.UpdateProcessor.Extensions;
using BedrockLauncher.ViewModels;
using Newtonsoft.Json;
using PostSharp.Patterns.Model;

namespace BedrockLauncher.Classes
{

    [NotifyPropertyChanged(ExcludeExplicitProperties = Constants.Debugging.ExcludeExplicitProperties)]
    public class MCVersion
    {
        public MCVersion(string uuid, string name, VersionType type, string architecture)
        {
            this.UUID = uuid.ToLower();
            this.Name = name;
            this.Type = type;
            this.Architecture = architecture;
        }

        public string UUID { get; set; }
        public string Name { get; set; }
        public string Architecture { get; set; }
        public bool IsBeta
        {
            get
            {
                return Type == VersionType.Beta;
            }
        }
        public bool IsRelease
        {
            get
            {
                return Type == VersionType.Release;
            }
        }
        public bool IsPreview
        {
            get
            {
                return Type == VersionType.Preview;
            }
        }

        public VersionType Type { get; set; }
        public bool IsInstalled
        {
            get
            {
                Depends.On(GameDirectory, RequireSizeRecalculation);
                return File.Exists(ManifestPath);
            }
        }
        public string GameDirectory
        {
            get
            {
                Depends.On(UUID);
                return Path.GetFullPath(MainViewModel.Default.FilePaths.CurrentLocation + "\\versions\\Minecraft-" + UUID);
            }
        }
        public string DisplayName
        {
            get
            {
                Depends.On(Type, Name, Architecture);

                string _TypeSuffix = string.Empty;
                string _ArchSuffix = string.Empty;


                if (!VersionDbExtensions.DoesVerionArchMatch(Constants.CurrentArchitecture, Architecture))
                    _ArchSuffix = $" [{Architecture}]";

                switch (Type)
                {
                    case VersionType.Beta:
                        _TypeSuffix = string.Format(" ({0})", "Beta"); //TODO: Localize String
                        break;
                    case VersionType.Preview:
                        _TypeSuffix = string.Format(" ({0})", "Preview"); //TODO: Localize String
                        break;
                }

                return Name + _TypeSuffix + _ArchSuffix;
            }
        }
        public string InstallationSize
        {
            get
            {
                Depends.On(RequireSizeRecalculation);
                if (Constants.Debugging.CalculateVersionSizes) Task.Run(GetInstallSize);
                else RequireSizeRecalculation = false;
                return StoredInstallationSize;
            }
        }
        public string IconPath
        {
            get
            {
                Depends.On(IsBeta, IsPreview, IsRelease);
                if (IsBeta) return Constants.BETA_VERSION_ICONPATH;
                else if (IsPreview) return Constants.PREVIEW_VERSION_ICONPATH;
                else if (IsRelease) return Constants.RELEASE_VERSION_ICONPATH;
                else return Constants.UNKNOWN_VERSION_ICONPATH;
            }
        }
        public string ManifestPath
        {
            get
            {
                Depends.On(GameDirectory);
                return Path.Combine(GameDirectory, "AppxManifest.xml");
            }
        }

        #region Size Calcualtion

        [JsonIgnore] private string StoredInstallationSize { get; set; } = "...";
        [JsonIgnore] private bool RequireSizeRecalculation { get; set; } = false;
        [JsonIgnore] private static bool Internal_SizeCalcInProgress { get; set; } = false;
        private async Task GetInstallSize()
        {
            while (Internal_SizeCalcInProgress) await Task.Delay(500);

            await Task.Run(() =>
            {
                if (!RequireSizeRecalculation) return;

                if (IsInstalled)
                {
                    Internal_SizeCalcInProgress = true;
                    var dirSize = GetDirectorySize(Path.GetFullPath(GameDirectory));
                    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                    int order = 0;
                    double len = dirSize;
                    while (len >= 1024 && order < sizes.Length - 1)
                    {
                        order++;
                        len = len / 1024;
                    }
                    StoredInstallationSize = String.Format("{0:0.##} {1}", len, sizes[order]);
                    RequireSizeRecalculation = false;
                    Internal_SizeCalcInProgress = false;
                }
                else
                {
                    StoredInstallationSize = "...";
                    RequireSizeRecalculation = false;
                }
            });

            ulong GetDirectorySize(string dir)
            {
                dynamic fso = Activator.CreateInstance(System.Type.GetTypeFromProgID("Scripting.FileSystemObject"));
                dynamic fldr = fso.GetFolder(dir);
                return (ulong)fldr.size;
            }
        }

        #endregion

        #region Methods

        public string GetPackageNameFromMainifest()
        {
            try
            {
                string manifestXml = File.ReadAllText(ManifestPath);
                XDocument XMLDoc = XDocument.Parse(manifestXml);
                var Descendants = XMLDoc.Descendants();
                XElement Identity = Descendants.Where(x => x.Name.LocalName == "Identity").FirstOrDefault();
                string Name = Identity.Attribute("Name").Value;
                string Version = Identity.Attribute("Version").Value;
                string ProcessorArchitecture = Identity.Attribute("ProcessorArchitecture").Value;
                return String.Join("_", Name, Version, ProcessorArchitecture);
            }
            catch
            {
                return "???";
            }
        }
        public void OpenDirectory()
        {
            string Directory = Path.GetFullPath(GameDirectory);
            if (!System.IO.Directory.Exists(Directory)) System.IO.Directory.CreateDirectory(Directory);
            Process.Start("explorer.exe", Directory);
        }
        public void UpdateFolderSize()
        {
            RequireSizeRecalculation = true;
        }
        public int Compare(MCVersion y)
        {
            try
            {
                var a = Version.Parse(this.Name);
                var b = Version.Parse(y.Name);
                return b.CompareTo(a);
            }
            catch
            {
                return y.Name.CompareTo(this.Name);
            }

        }

        #endregion
    }
}
