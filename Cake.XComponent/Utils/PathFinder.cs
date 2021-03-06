﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Cake.Core.Diagnostics;
using Cake.XComponent.Exception;
[assembly: InternalsVisibleTo("Cake.XComponent.Test")]

namespace Cake.XComponent.Utils
{
    internal class PathFinder
    {
        private const string XcStudioProgram = "XCStudio{0}.exe";
        private const string XcSpyProgram = "xcspy{0}.exe";
        private const string XcRuntimeProgram = "xcruntime{0}.exe";
        private const string XcBridgeProgram = "XCWebSocketBridge{0}.exe";
        private const string XcBuildProgram = "xcbuild{0}.exe";
        private const string X86Suffix = "32";
        private const string CakeToolsDirectory = "tools";
        private readonly ICakeLog _cakeLog;
        private static string _workingDirectory;

        internal static string WorkingDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_workingDirectory))
                {
                    _workingDirectory = Directory.GetCurrentDirectory();
                }

                return _workingDirectory;
            }
            set { _workingDirectory = value; }
        }

        internal static string XcStudioPath { get; set; }

        internal static string XcBuildPath { get; set; }

        internal static string XcRuntimePath { get; set; }

        internal static string XcBridgePath { get; set; }

        internal static string XcSpyPath { get; set; }

        public  PathFinder(ICakeLog cakeLog)
        {
            _cakeLog = cakeLog;
        }

        internal string FindXcStudio(Platform platform)
        {
            return FindApplicationPath("XcStudio", XcStudioPath, GetXcStudioProgram(platform));
        }
        
        internal string FindXcStudioDirectory(Platform platform)
        {
            return Path.GetDirectoryName(FindXcStudio(platform));
        }

        internal static string GetXcStudioProgram(Platform platform)
        {
            return string.Format(XcStudioProgram, platform == Platform.X64 ? string.Empty : X86Suffix);
        }

        internal string FindXcBuild(Platform platform)
        {
            return FindApplicationPath("XcBuild", XcBuildPath, GetXcBuildProgram(platform));
        }
        
        internal string FindXcBuildDirectory(Platform platform)
        {
            return Path.GetDirectoryName(FindXcBuild(platform));
        }

        internal static string GetXcBuildProgram(Platform platform)
        {
            return string.Format(XcBuildProgram, platform == Platform.X64 ? string.Empty : X86Suffix);
        }

        internal string FindXcRuntime(Platform platform)
        {
            return FindApplicationPath("XcRuntime", XcRuntimePath, GetXcRuntimeProgram(platform));
        }
        
        internal string FindXcRuntimeDirectory(Platform platform)
        {
            return Path.GetDirectoryName(FindXcRuntime(platform));
        }

        internal static string GetXcRuntimeProgram(Platform platform)
        {
            return string.Format(XcRuntimeProgram, platform == Platform.X64 ? string.Empty : X86Suffix);
        }

        public string FindXcBridge(Platform platform)
        {
            return FindApplicationPath("XcBridge", XcBridgePath, GetXcBridgeProgram(platform));
        }
        
        internal string FindXcBridgeDirectory(Platform platform)
        {
            return Path.GetDirectoryName(FindXcBridge(platform));
        }

        internal static string GetXcBridgeProgram(Platform platform)
        {
            return string.Format(XcBridgeProgram, platform == Platform.X64 ? string.Empty : X86Suffix);
        }

        public string FindXcSpy(Platform platform)
        {
            return FindApplicationPath("XcSpy", XcSpyPath, GetXcSpyProgram(platform));
        }
        
        internal string FindXcSpyDirectory(Platform platform)
        {
            return Path.GetDirectoryName(FindXcSpy(platform));
        }

        internal static string GetXcSpyProgram(Platform platform)
        {
            return string.Format(XcSpyProgram, platform == Platform.X64 ? string.Empty : X86Suffix);
        }

        private string FindApplicationPath(string applicationName, string userPath, string exeToFind)
        {
            if (!string.IsNullOrEmpty(userPath))
            {
                if (!File.Exists(userPath))
                {
                    _cakeLog.Write(Verbosity.Normal, LogLevel.Fatal,
                        $"{applicationName} provided by user can't be fount at {userPath}");
                    return null;
                }

                _cakeLog.Write(Verbosity.Normal, LogLevel.Information,
                    $@"{applicationName} path provided by user: using {applicationName} version '{
                            GetVersion(userPath)
                        }' from {userPath}");
                return userPath;
            }

            var applicationPath = FindExe(exeToFind);
            _cakeLog.Write(Verbosity.Normal, LogLevel.Information,
                $@"{applicationName} auto-detection: using {applicationName} version '{
                        GetVersion(applicationPath)
                    }' from {applicationPath}");

            return applicationPath;
        }

        private string GetVersion(string path)
        {
            string version = "Unknown";

            try
            {
                version = FileVersionInfo.GetVersionInfo(path).ProductVersion;
            }
            catch (System.Exception)
            {
                _cakeLog.Write(Verbosity.Normal, LogLevel.Warning, $"Unable to retrieve version from file : {path}");
            }

            return version;
        }

        private static string FindExe(string exeToFind)
        {
            var toolsDirectory = Path.Combine(WorkingDirectory, CakeToolsDirectory);

            var exeFiles = new DirectoryInfo(toolsDirectory).GetFiles(exeToFind, SearchOption.AllDirectories);
            if (exeFiles.Any())
            {
                return exeFiles.First().FullName;
            }

            throw new XComponentException($"Can't find {exeToFind}, please make sure {exeToFind} exists in the tools directory.");
        }
    }
}
