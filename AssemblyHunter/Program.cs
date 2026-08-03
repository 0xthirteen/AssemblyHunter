using System;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Reflection;
using System.Management;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32;

namespace AssemblyHunter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage();
                return;
            }

            if(args[0].ToLower() == "help")
            {
                Usage();
                return;
            }
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var arguments = new Dictionary<string, string>();
            foreach (string argument in args)
            {
                int idx = argument.IndexOf('=');
                if (idx > 0)
                    arguments[argument.Substring(0, idx)] = argument.Substring(idx + 1);
            }

            // TODO: Add write to file
            // TODO: Finish Autoruns

            string binp = string.Empty;
            string servicename = string.Empty;
            bool quiet = false;
            bool allpaths = false;
            bool electron = false;

            if (arguments.ContainsKey("electron") && arguments["electron"].ToString().ToLower() == "true")
            {
                electron = true;
            }
            if (arguments.ContainsKey("allpaths") && arguments["allpaths"].ToString().ToLower() == "true")
            {
                allpaths = true;
            }
            if (arguments.ContainsKey("quiet") && arguments["quiet"].ToString().ToLower() == "true")
            {
                quiet = true;
            }

            if (arguments.ContainsKey("path"))
            {
                string path = arguments["path"].ToString();
                if (File.Exists(path))
                {
                    try
                    {
                        if(electron == true)
                        {
                            if (IsElectronApp(path, out string asar))
                            {
                                ElectronChecks(path, arguments, asar);
                            }
                        }
                        else
                        {
                            AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);
                            AssemblyChecks(path, arguments);
                        }

                    }
                    catch { }

                }
                else
                {
                    if (arguments.ContainsKey("recurse") && arguments["recurse"].ToString().ToLower() == "true")
                    {
                        List<string> files = DirSearch(path, allpaths, quiet);
                        foreach (var f in files)
                        {
                            try
                            {
                                if (electron == true)
                                {
                                    if (IsElectronApp(f, out string asar))
                                    {
                                        ElectronChecks(f, arguments, asar);
                                    }
                                }
                                else
                                {
                                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(f);
                                    AssemblyChecks(f, arguments);
                                }
                            }
                            catch { continue; }
                        }

                    }
                    else
                    {
                        foreach (var f in Directory.GetFiles(path))
                        {
                            try
                            {
                                if (electron == true)
                                {
                                    if (IsElectronApp(f, out string asar))
                                    {
                                        ElectronChecks(f, arguments, asar);
                                    }
                                }
                                else
                                {
                                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(f);
                                    AssemblyChecks(f, arguments);
                                }
                            }
                            catch { continue; }
                        }
                    }
                }
                watch.Stop();
                var ct = watch.ElapsedMilliseconds / 1000.0;
                Console.WriteLine("\n\n[+++] Completed in {0} seconds", ct);
            }
            else if (arguments.ContainsKey("file"))
            {
                string file = arguments["file"].ToString();
                if (!File.Exists(file))
                {
                    Console.WriteLine("[-] File doesn't exist");
                    return;
                }
                else
                {
                    try
                    {
                        if (electron == true)
                        {
                            if (IsElectronApp(file, out string asar))
                            {
                                ElectronChecks(file, arguments, asar);
                            }
                        }
                        else
                        {
                            AssemblyName assemblyName = AssemblyName.GetAssemblyName(file);
                            AssemblyChecks(file, arguments);
                        }
                    }
                    catch { }
                }
            }
            else if (arguments.ContainsKey("collection"))
            {
                string file = arguments["collection"].ToString();
                if (!File.Exists(file))
                {
                    Console.WriteLine("[-] File doesn't exist");
                    return;
                }
                else
                {
                    string[] readlines = File.ReadAllLines(file);
                    foreach (string line in readlines)
                    {
                        try
                        {
                            if (electron == true)
                            {
                                if (IsElectronApp(line, out string asar))
                                {
                                    ElectronChecks(line, arguments, asar);
                                }
                            }
                            else
                            {
                                AssemblyName assemblyName = AssemblyName.GetAssemblyName(line);
                                AssemblyChecks(line, arguments);
                            }
                        }
                        catch { continue; }
                    }
                }
            }
            else if (arguments.ContainsKey("autoruns"))
            {
                Console.WriteLine("TODO");
                return;

                List<string> lmkeys = new List<string>
                {
                    //@"SYSTEM\CurrentControlSet\Control\SafeBoot\AlternateShell",
                    //@"Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options",
                    //@"Software\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Image File Execution Options",
                    //@"SOFTWARE\Classes\Htmlfile\Shell\Open\Command\(Default)",
                    @"System\CurrentControlSet\Control\Terminal Server\Wds\rdpwd\StartupPrograms",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\AppSetup",
                    @"Software\Policies\Microsoft\Windows\System\Scripts\Startup",
                    @"Software\Policies\Microsoft\Windows\System\Scripts\Logon",
                    @"Environment\UserInitMprLogonScript",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Userinit",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\VmApplet",
                    @"Software\Policies\Microsoft\Windows\System\Scripts\Shutdown",
                    @"Software\Policies\Microsoft\Windows\System\Scripts\Logoff",
                    @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup",
                    @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Logon",
                    @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Logoff",
                    @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Shutdown",
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\System\Shell",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Shell",
                    @"SYSTEM\CurrentControlSet\Control\SafeBoot\AlternateShell",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Taskman",
                    @"Software\Microsoft\Windows NT\CurrentVersion\Winlogon\AlternateShells\AvailableShells",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\Install\Software\Microsoft\Windows\CurrentVersion\Runonce",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\Install\Software\Microsoft\Windows\CurrentVersion\RunonceEx",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\Install\Software\Microsoft\Windows\CurrentVersion\Run",
                    @"SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp\InitialProgram",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnceEx",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\RunOnceEx",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\RunOnce",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run",
                    @"SOFTWARE\Microsoft\Active Setup\Installed Components",
                    @"SOFTWARE\Wow6432Node\Microsoft\Active Setup\Installed Components",
                    @"Software\Microsoft\Windows NT\CurrentVersion\Windows\IconServiceLib",
                    @"SOFTWARE\Microsoft\Windows CE Services\AutoStartOnConnect",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows CE Services\AutoStartOnConnect",
                    @"SOFTWARE\Microsoft\Windows CE Services\AutoStartOnDisconnect",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows CE Services\AutoStartOnDisconnect"
                };

                List<string> cukeys = new List<string>
                {
                    @"Software\Policies\Microsoft\Windows\System\Scripts\Logon",
                    @"Environment\UserInitMprLogonScript",
                    @"Software\Policies\Microsoft\Windows\System\Scripts\Logoff",
                    @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup",
                    @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Logon",
                    @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Logoff",
                    @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Shutdown",
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\System\Shell",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Shell",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnceEx",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\RunOnceEx",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\RunOnce",
                    @"Software\Microsoft\Windows NT\CurrentVersion\Windows\Load",
                    @"Software\Microsoft\Windows NT\CurrentVersion\Windows\Run",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\Install\Software\Microsoft\Windows\CurrentVersion\Runonce",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\Install\Software\Microsoft\Windows\CurrentVersion\RunonceEx",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\Install\Software\Microsoft\Windows\CurrentVersion\Run"
                };
                    
                List<string> dlkeys = new List<string>
                {
                    @"Software\Classes\*\ShellEx\ContextMenuHandlers",
                    @"Software\Classes\Directory\ShellEx\ContextMenuHandlers",
                    @"Software\Classes\Directory\Shellex\DragDropHandlers",
                    @"Software\Classes\Directory\Shellex\CopyHookHandlers",
                    @"Software\Classes\Folder\ShellEx\ContextMenuHandlers",
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects",
                    @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects",
                };

                try
                {
                    foreach (string ckey in lmkeys) 
                    {
                        Console.WriteLine("Current: {0}", ckey);
                        RegistryKey key = Registry.LocalMachine.OpenSubKey(ckey);
                        if (key != null)
                        {
                            string[] vnames = key.GetValueNames();
                            foreach (string vs in vnames)
                            {
                                object kobj = key.GetValue(vs);
                                if (kobj != null)
                                {
                                    //Version version = new Version(o as String);
                                    Console.WriteLine("\t{0}", kobj.ToString());
                                }
                            }
                        }
                    }

                    foreach (string xkey in cukeys)
                    {
                        Console.WriteLine("Current: {0}", xkey);
                        RegistryKey key = Registry.LocalMachine.OpenSubKey(xkey);
                        if (key != null)
                        {
                            string[] vnames = key.GetValueNames();
                            foreach (string vs in vnames)
                            {
                                object kobj = key.GetValue(vs);

                                if (kobj != null)
                                {
                                    //Version version = new Version(o as String);
                                    Console.WriteLine("\t{0}", kobj.ToString());
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            else if (arguments.ContainsKey("tasks"))
            {
                ManagementObjectSearcher wmiData = null;
                try
                {
                    // WMI Code taken and modified from Seatbelt
                    wmiData = new ManagementObjectSearcher(@"Root\Microsoft\Windows\TaskScheduler", "SELECT * FROM MSFT_ScheduledTask");
                    ManagementObjectCollection data = wmiData.Get();
                    foreach (ManagementObject result in data)
                    {
                        string taskname = result["TaskName"].ToString();
                        var actions = (ManagementBaseObject[])result["Actions"];
                        foreach (var obj in actions)
                        {
                            var Properties = new Dictionary<string, object>();

                            foreach (var prop in obj.Properties)
                            {
                                if (!prop.Name.Equals("PSComputerName"))
                                {
                                    Properties[prop.Name] = prop.Value;
                                }
                            }
                            try
                            {
                                string targfile = Properties["Execute"].ToString();
                                var fullpath = Environment.ExpandEnvironmentVariables(targfile);
                                fullpath = fullpath.Replace("\"", "");

                                if (electron == true)
                                {
                                    if (IsElectronApp(fullpath, out string asar))
                                    {
                                        ElectronChecks(fullpath, arguments, asar, taskname, "tasks");
                                    }
                                }
                                else
                                {
                                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(fullpath);
                                    AssemblyChecks(fullpath, arguments, "tasks");
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("[X] Exception {0}", ex.Message));
                }
                watch.Stop();
                var ct = watch.ElapsedMilliseconds / 1000.0;
                Console.WriteLine("\n\n[+++] Completed in {0} seconds", ct);
            }
            else if (arguments.ContainsKey("services"))
            {
                if (arguments.ContainsKey("servicename"))
                {
                    servicename = arguments["servicename"];
                }

                ServiceController[] scServices;
                scServices = ServiceController.GetServices();
                if (servicename != string.Empty)
                {
                    ManagementObject wmiService;
                    wmiService = new ManagementObject("Win32_Service.Name='" + servicename + "'");
                    wmiService.Get();
                    binp = (string)wmiService["PathName"];
                    try
                    {

                        if (!binp.StartsWith("\""))
                        {
                            string[] subs = binp.Split(' ');
                            binp = subs[0];
                        }
                        binp = binp.Replace("\"", "");

                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(binp);
                        AssemblyChecks(binp, arguments, servicename, "services");
                    }
                    catch { }
                }
                else
                {
                    foreach (ServiceController scTemp in scServices)
                    {
                        //if (scTemp.Status == ServiceControllerStatus.Running)
                        //{
                            ManagementObject wmiService;
                            wmiService = new ManagementObject("Win32_Service.Name='" + scTemp.ServiceName + "'");
                            wmiService.Get();
                            string sname = scTemp.ServiceName;
                            binp = (string)wmiService["PathName"];

                            try
                            {

                                if (!binp.StartsWith("\""))
                                {
                                    string[] subs = binp.Split(' ');
                                    binp = subs[0];
                                }
                                binp = binp.Replace("\"", "");

                                AssemblyName assemblyName = AssemblyName.GetAssemblyName(binp);
                            AssemblyChecks(binp, arguments, sname, "services");
                            }
                            catch { continue; }
                        //}
                    }
                }
                watch.Stop();
                var ct = watch.ElapsedMilliseconds / 1000.0;
                Console.WriteLine("\n\n[+++] Completed in {0} seconds", ct);
            }
        }

        static void Usage()
        {
            Console.WriteLine("\n  AssemblyHunter.exe");
            Console.WriteLine("");
            Console.WriteLine("  Run Type Options:");
            Console.WriteLine(@"    path             (ex: path=C:\Users)         full path to search");
            Console.WriteLine(@"    file             (ex: file=C:\file.exe)      check if a specific file is an assembly");
            Console.WriteLine(@"    collection       (ex: file=C:\files.txt)     run checks against assemblies listed in a file");
            Console.WriteLine("    services         (ex: services=true)         enumerates all services for assemblies");
            Console.WriteLine("    tasks            (ex: tasks=true)            enumerates all scheduled tasks for assemblies");
            Console.WriteLine("    autoruns         (ex: autoruns=true)         enumerates common autorun locations for assemblies");
            Console.WriteLine("\n  Optional args:");
            Console.WriteLine("    recurse          (ex: recurse=true)          recurse the path given");
            Console.WriteLine("    allpaths         (ex: allpaths=true)         recurses all directores, by default some directores with common Microsoft assemblies are skipped");
            Console.WriteLine("    exeonly          (ex: exeonly=true)          look for exes only");
            Console.WriteLine("    getarch          (ex: getarch=true)          get assembly architecture");
            Console.WriteLine("    servicename      (ex: services=true)         check a specific service (needs services run)");
            Console.WriteLine("    isservice        (ex: isservice=true)        check if an exe is a service executable");
            Console.WriteLine("    getuac           (ex: getuac=true)           gets UAC settings of assembly");
            Console.WriteLine("    getrefs          (ex: getrefs=true)          gets references for target assembly");
            Console.WriteLine("    getasmid         (ex: getasmid=true)         gets internal assembly manifest identity");
            Console.WriteLine("    getappid         (ex: getappid=true)         gets internal application manifest identity");
            Console.WriteLine("    getappmanifest   (ex: getappmanifest=true)   gets internal application manifest");
            Console.WriteLine("    getasmmanifest   (ex: getasmmanifest=true)   gets internal assembly manifest");
            Console.WriteLine("    electron         (ex: electron=true)         look for electron apps instead of assemblies");


            Console.WriteLine("\n*    path, file, collection, services, tasks, or autoruns should indicate the type of search performed, all other options narrow down the search");
            Console.WriteLine(@"Example: AssemblyHunter.exe path=C:\ recurse=true signed=true");
            Console.WriteLine(@"Example: AssemblyHunter.exe services=true signed=true");
            Console.WriteLine(@"Example: AssemblyHunter.exe tasks=true signed=true getarch=true");
            Console.WriteLine("");
        }
        static List<string> DirSearch(string dir, bool searchall, bool quiet)
        {
            List<string> skippath = new List<string>
            {
                @"C:\Windows\SxS",
                @"C:\Windows\CCM",
                @"C:\Windows\WinSxS",
                @"C:\Windows\SysWOW64\WinMetadata",
                @"C:\Windows\SysWOW64\WindowsPowerShell",
                @"C:\Windows\SysWOW64\wbem",
                @"C:\Windows\SysWOW64",
                @"C:\Windows\SystemApps",
                @"C:\Windows\System32\WinMetadata",
                @"C:\Windows\System32\WindowsPowerShell",
                @"C:\Windows\System32\wbem",
                @"C:\Windows\Microsoft.NET\Framework64",
                @"C:\Windows\Microsoft.NET\Framework",
                @"C:\Windows\Microsoft.NET\assembly",
                @"C:\Windows\Installer",
                @"C:\Windows\assembly",
                @"C:\Windows\servicing",
                @"C:\Program Files (x86)\dotnet",
                @"C:\Program Files (x86)\Microsoft Visual Studio 14.0",
                @"C:\Program Files (x86)\IIS",
                @"C:\Program Files (x86)\IIS Express",
                @"C:\Program Files (x86)\Microsoft Office",
                @"C:\Program Files (x86)\Microsoft\Microsoft Search in Bing",
                @"C:\Program Files (x86)\Microsoft Azure Information Protection",
                @"C:\Program Files (x86)\Microsoft Visual Studio",
                @"C:\Program Files (x86)\Microsoft Azure Storage Explorer",
                @"C:\Program Files (x86)\Microsoft Intune Management Extension",
                @"C:\Program Files (x86)\EventManagement",
                @"C:\Program Files (x86)\Windows Kits",
                @"C:\Program Files (x86)\Reference Assemblies",
                @"C:\Program Files (x86)\Microsoft SDKs",
                @"C:\Program Files (x86)\Microsoft Silverlight",
                @"C:\Program Files (x86)\Common Files\Microsoft Shared",
                @"C:\Program Files (x86)\MSBuild",
                @"C:\Program Files (x86)\Workflow Manager Tools",
                @"C:\Program Files\PowerShell",
                @"C:\Program Files\Microsoft Office",
                @"C:\Program Files\WindowsApps",
                @"C:\Program Files\IIS",
                @"C:\Program Files\IIS Express",
                @"C:\Program Files\Microsoft SQL Server",
                @"C:\Program Files\dotnet",
                @"C:\Program Files\Reference Assemblies\Microsoft",
                @"C:\Program Files\Common Files\microsoft shared\VS7DEBUG",
                @"C:\ProgramData\Microsoft\DefaultPackMSI",
                @"C:\ProgramData\Microsoft\VisualStudio",
                @"C:\Users\All Users\Microsoft\VisualStudio",
                @"C:\Users\All Users\Microsoft\DefaultPackMSI"
            };
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                {
                        files.Add(f);                    
                }
                foreach (string d in Directory.GetDirectories(dir))
                {
                    if(searchall == false)
                    {
                        if (!skippath.Contains(d))
                        {
                            files.AddRange(DirSearch(d, searchall, quiet));
                        }
                        else
                        {
                            if(quiet == false)
                            {
                                Console.WriteLine("[-] Skipping directory: {0}", d);
                            }
                            DirSearch(d, searchall, quiet);
                        }
                    }
                    else
                    {
                        files.AddRange(DirSearch(d, searchall, quiet));
                    }
                }
            }
            catch (Exception)
            { }
            return files;
        }

        static List<string> GetDirs(string dir)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    files.Add(d);
                    files.AddRange(GetDirs(d));
                }
            }
            catch (Exception)
            { }
            return files;
        }

        // True if `exePath` is the launcher of an Electron app, identified by either of the two
        // definitive signals next to it: a valid packed resources\app.asar, or an unpacked
        // resources\app\package.json. Sets asarPath when the packed archive is present.
        static bool IsElectronApp(string exePath, out string evidence)
        {
            evidence = null;
            try
            {
                if (Path.GetExtension(exePath).ToLower() != ".exe")
                    return false;

                string dir = Path.GetDirectoryName(exePath);
                if (string.IsNullOrEmpty(dir))
                    return false;

                string ev = null;
                string packed = Path.Combine(dir, "resources", "app.asar");
                if (File.Exists(packed) && IsValidAsar(packed))
                {
                    ev = packed;
                }
                else
                {
                    string unpacked = Path.Combine(dir, "resources", "app", "package.json");
                    if (File.Exists(unpacked))
                    {
                        ev = unpacked;
                    }
                }
                if (ev == null)
                    return false;

                // The directory says "an Electron app lives here"; confirm THIS exe is the launcher
                // and not a co-located helper (Squirrel updater, NSIS uninstaller) by requiring it to
                // statically import the Chromium runtime. Done last so the PE walk only runs on exes
                // already sitting next to a valid app.asar / package.json.
                if (!ImportsChromiumRuntime(exePath))
                    return false;

                evidence = ev;
                return true;
            }
            catch { return false; }
        }

        // Confirm a file is really an ASAR archive: 16 bytes of Pickle framing, then the JSON
        // header, which always begins with {"files":. Guards against name-only false positives.
        static bool IsValidAsar(string path)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                {
                    reader.BaseStream.Seek(16, SeekOrigin.Begin);
                    byte[] buf = new byte[8];
                    int n = reader.Read(buf, 0, buf.Length);
                    return System.Text.Encoding.ASCII.GetString(buf, 0, n).StartsWith("{\"files\"");
                }
            }
            catch { return false; }
        }

        // True if the exe imports a Chromium/Electron runtime DLL. The real launcher pulls in
        // ffmpeg.dll (and friends); Squirrel updaters and NSIS uninstallers that share the app folder
        // do not. These DLLs are usually delay-loaded, so both the standard (dir 1) and delay-load
        // (dir 13) import directories are scanned. Reads only the bytes it needs, so it is cheap even
        // on 200 MB+ launchers.
        static bool ImportsChromiumRuntime(string exePath)
        {
            string[] markers = { "ffmpeg.dll", "libglesv2.dll", "libegl.dll", "d3dcompiler_47.dll" };
            try
            {
                using (FileStream fs = File.OpenRead(exePath))
                {
                    byte[] buf = new byte[24];
                    ReadAt(fs, 0x3C, buf, 4);
                    int peOff = BitConverter.ToInt32(buf, 0);

                    ReadAt(fs, peOff, buf, 24);                       // PE signature (4) + COFF header (20)
                    if (buf[0] != 'P' || buf[1] != 'E')
                        return false;
                    ushort numSections = BitConverter.ToUInt16(buf, 6);
                    ushort optSize = BitConverter.ToUInt16(buf, 20);

                    int optOff = peOff + 24;
                    byte[] opt = new byte[optSize];
                    ReadAt(fs, optOff, opt, optSize);
                    ushort magic = BitConverter.ToUInt16(opt, 0);     // 0x10b = PE32, 0x20b = PE32+
                    int dirStart = (magic == 0x20b) ? 112 : 96;

                    byte[] secBuf = new byte[numSections * 40];
                    ReadAt(fs, optOff + optSize, secBuf, secBuf.Length);
                    int[,] sec = new int[numSections, 3];             // [i] = { VirtualAddress, spannedSize, rawPtr }
                    for (int i = 0; i < numSections; i++)
                    {
                        int s = i * 40;
                        int vsize = BitConverter.ToInt32(secBuf, s + 8);
                        int rsize = BitConverter.ToInt32(secBuf, s + 16);
                        sec[i, 0] = BitConverter.ToInt32(secBuf, s + 12);
                        sec[i, 1] = (vsize > rsize) ? vsize : rsize;
                        sec[i, 2] = BitConverter.ToInt32(secBuf, s + 20);
                    }

                    // { data-directory index, descriptor size, offset of the DLL-name RVA within it }
                    int[,] tables = { { 1, 20, 12 }, { 13, 32, 4 } };
                    byte[] desc = new byte[32];
                    for (int t = 0; t <= tables.GetUpperBound(0); t++)
                    {
                        int dirRva = BitConverter.ToInt32(opt, dirStart + tables[t, 0] * 8);
                        if (dirRva == 0)
                            continue;
                        int off = RvaToOffset(dirRva, sec, numSections);
                        if (off < 0)
                            continue;

                        int descSize = tables[t, 1];
                        int nameField = tables[t, 2];
                        for (int guard = 0; guard < 8192; guard++, off += descSize)
                        {
                            ReadAt(fs, off, desc, descSize);
                            int nameRva = BitConverter.ToInt32(desc, nameField);
                            if (nameRva == 0)
                                break;                                // null terminator descriptor

                            int nameOff = RvaToOffset(nameRva, sec, numSections);
                            if (nameOff < 0)
                                continue;

                            string dll = ReadAsciiZ(fs, nameOff).ToLower();
                            foreach (string m in markers)
                                if (dll == m)
                                    return true;
                        }
                    }
                    return false;
                }
            }
            catch { return false; }
        }

        // Map a PE relative virtual address to a file offset using the section table.
        static int RvaToOffset(int rva, int[,] sec, int n)
        {
            for (int i = 0; i < n; i++)
            {
                int va = sec[i, 0];
                if (rva >= va && rva < va + sec[i, 1])
                    return sec[i, 2] + (rva - va);
            }
            return -1;
        }

        static void ReadAt(FileStream fs, long offset, byte[] buf, int count)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            int read = 0;
            while (read < count)
            {
                int n = fs.Read(buf, read, count - read);
                if (n <= 0)
                    throw new EndOfStreamException();
                read += n;
            }
        }

        static string ReadAsciiZ(FileStream fs, long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int c;
            while ((c = fs.ReadByte()) > 0)
                sb.Append((char)c);
            return sb.ToString();
        }

        static void ElectronChecks(string path, Dictionary<string, string> arguments, string evidence = null, string auxname = null, string runt = null)
        {
            string targetapp = path;
            bool signed = false;
            bool exeonly = false;

            if (arguments.ContainsKey("signed") && arguments["signed"].ToString().ToLower() == "true")
            {
                signed = true;
            }
            if (arguments.ContainsKey("exeonly") && arguments["exeonly"].ToString().ToLower() == "true")
            {
                exeonly = true;
            }

            if (exeonly == true)
            {
                if (!CheckFile(targetapp))
                {
                    return;
                }
            }

            if (signed == true)
            {
                if (!CheckSigned(targetapp))
                {
                    return;
                }
            }


            Console.WriteLine("[+] Found electron app: {0}", targetapp);

            if (evidence != null)
            {
                string label = evidence.ToLower().EndsWith(".asar") ? "app.asar" : "package.json";
                Console.WriteLine("    [+] {0}: {1}", label, evidence);
            }

            if (runt == "services")
            {
                Console.WriteLine("    [+] Service Name: {0}", auxname);
            }

            else if (runt == "tasks")
            {
                Console.WriteLine("    [+] Scheduled Task Name: {0}", auxname);
            }

            if (signed == true)
            {
                X509Certificate basicSigner = X509Certificate.CreateFromSignedFile(targetapp);
                X509Certificate2 cert = new X509Certificate2(basicSigner);
                Console.WriteLine("    [+] Cert Issuer Name: {0}", cert.IssuerName.Name);
                Console.WriteLine("    [+] Cert Subject Name: {0}", cert.SubjectName.Name);
            }

            Console.WriteLine("");
        }
        
        static void AssemblyChecks(string path, Dictionary<string, string> arguments, string auxname = null, string runt = null)
        {
            string targetAssembly = path;
            bool signed = false;
            bool exeonly = false;
            bool getappid = false;
            bool getasmid = false;
            bool getappmanifest = false;
            bool getasmmanifest = false;
            bool getarch = false;
            bool issvc = false;
            bool getuac = false;
            bool getrefs = false;
            bool clickonce = false;

            if (arguments.ContainsKey("signed") && arguments["signed"].ToString().ToLower() == "true")
            {
                signed = true;
            }
            if (arguments.ContainsKey("exeonly") && arguments["exeonly"].ToString().ToLower() == "true")
            {
                exeonly = true;
            }
            if (arguments.ContainsKey("getasmid") && arguments["getasmid"].ToString().ToLower() == "true")
            {
                getasmid = true;
            }
            if (arguments.ContainsKey("getappid") && arguments["getappid"].ToString().ToLower() == "true")
            {
                getappid = true;
            }
            if (arguments.ContainsKey("getappmanifest") && arguments["getappmanifest"].ToString().ToLower() == "true")
            {
                getappmanifest = true;
            }
            if (arguments.ContainsKey("getasmmanifest") && arguments["getasmmanifest"].ToString().ToLower() == "true")
            {
                getasmmanifest = true;
            }
            if (arguments.ContainsKey("getarch") && arguments["getarch"].ToString().ToLower() == "true")
            {
                getarch = true;
            }
            if (arguments.ContainsKey("getuac") && arguments["getuac"].ToString().ToLower() == "true")
            {
                getuac = true;
            }
            if (arguments.ContainsKey("getrefs") && arguments["getrefs"].ToString().ToLower() == "true")
            {
                getrefs = true;
            }
            if (arguments.ContainsKey("isservice") && arguments["isservice"].ToString().ToLower() == "true")
            {
                issvc = true;
            }
            if (arguments.ContainsKey("clickonce") && arguments["clickonce"].ToString().ToLower() == "true")
            {
                clickonce = true;
                exeonly = true;
                signed = true;
                getappid = true;
                getasmid = true;
                getuac = true;
            }

            if(exeonly == true)
            {
                if (!CheckFile(targetAssembly))
                {
                    return;
                }
            }

            if(signed == true)
            {
                if (!CheckSigned(targetAssembly))
                {
                    return;
                }
            }
            
            if(clickonce == true)
            {
                string uacinfo = string.Empty;
                string appidinfo = string.Empty;
                uacinfo = GetUacInfo(targetAssembly);
                if (uacinfo != "asInvoker" && uacinfo != "No UAC settings")
                {
                    return;
                }

                try
                {
                    var getapp = GetPEFileManifest(targetAssembly);
                    XmlDocument appxml = new XmlDocument();
                    appxml.LoadXml(getapp.OuterXml);
                    XmlNodeList applicationidentity = appxml.GetElementsByTagName("assemblyIdentity");
                    var appidentity = applicationidentity[0].OuterXml;
                    if (!appidentity.Contains("processorArchitecture"))
                    {
                        return;
                    }
                }
                catch { }
                
            }

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(targetAssembly);
            Assembly tasm = null;
            ProcessorArchitecture procinfo;
            if (getarch == true || getrefs == true)
            {
                try
                {
                    tasm = Assembly.LoadFrom(targetAssembly);
                }
                catch { }
            }

            Console.WriteLine("[+] Found assembly: {0}", targetAssembly);

            if (runt == "services")
            {
                Console.WriteLine("    [+] Service Name: {0}", auxname);
            }

            else if (runt == "tasks")
            {
                Console.WriteLine("    [+] Scheduled Task Name: {0}", auxname);
            }

            if (getarch == true)
            {
                procinfo = assemblyName.ProcessorArchitecture;
                try
                {

                    PortableExecutableKinds peKind;
                    ImageFileMachine machine;
                    tasm.ManifestModule.GetPEKind(out peKind, out machine);
                    Console.WriteLine("    [+] Assembly Architecture: {0} {1}", procinfo, peKind);
                }
                catch
                {
                    Console.WriteLine("    [+] Assembly Architecture: {0}", procinfo);
                }
            }

            if (signed == true)
            {
                X509Certificate basicSigner = X509Certificate.CreateFromSignedFile(targetAssembly);
                X509Certificate2 cert = new X509Certificate2(basicSigner);
                Console.WriteLine("    [+] Cert Issuer Name: {0}", cert.IssuerName.Name);
                Console.WriteLine("    [+] Cert Subject Name: {0}", cert.SubjectName.Name);
            }

            if (issvc == true)
            {
                bool svccheck = CheckIfService(targetAssembly);
                Console.WriteLine("    [+] Is a service exe: {0}", svccheck);
            }

            if (getuac == true)
            {
                string uacout = string.Empty;
                uacout = GetUacInfo(targetAssembly);
                if(uacout != "No UAC settings")
                {
                    Console.WriteLine("    [+] UAC settings: {0}", uacout);
                }
                else
                {
                    Console.WriteLine("    [-] No UAC settings");
                }
            }

            if (getasmid == true)
            {
                try
                {
                    var defid = GetDefinitionIdentity(targetAssembly);
                    Console.WriteLine("    [+] Assembly Manifest Identity: {0}", defid);
                }
                catch (Exception)
                {
                    Console.WriteLine("    [-] No Assembly Manifest Identity");
                }
            }

            if (getappid == true)
            {
                try
                {
                    var getapp = GetPEFileManifest(targetAssembly);
                    XmlDocument appxml = new XmlDocument();
                    appxml.LoadXml(getapp.OuterXml);
                    XmlNodeList applicationidentity = appxml.GetElementsByTagName("assemblyIdentity");
                    var appidentity = applicationidentity[0].OuterXml;
                    if (appidentity.Contains("xmlns=\"urn:schemas-microsoft-com:asm.v1\""))
                    {
                        appidentity = appidentity.Replace("xmlns=\"urn:schemas-microsoft-com:asm.v1\"", "");
                    }

                    if (applicationidentity[0].ParentNode.Name != "dependentAssembly")
                    {
                        Console.WriteLine("    [+] Application Manifest Identity : {0}", appidentity);
                    }
                    else
                    {
                        Console.WriteLine("    [-] No Application Manifest Identity");
                    }
                }
                catch
                {
                    Console.WriteLine("    [-] No Application Manifest Identity");
                }
            }

            if (getappmanifest == true)
            {
                try
                {
                    var pemanifest = GetPEFileManifest(targetAssembly);
                    XDocument doc = XDocument.Parse(pemanifest.OuterXml);
                    Console.WriteLine("    [+] Internal Application Manifest: {0}", doc);
                }
                catch (Exception)
                {
                    Console.WriteLine("    [-] No Internal Application Manifest");
                }
            }

            if (getasmmanifest == true)
            {
                // Might not be needed/wanted
            }

            if (getrefs == true)
            {
                try
                {
                    Console.WriteLine("    [+] Application References");
                    foreach (AssemblyName an in tasm.GetReferencedAssemblies())
                    {
                        Console.WriteLine("      [+] Name={0}, Version={1}, PublicKey token={2}", an.Name, an.Version, (BitConverter.ToString(an.GetPublicKeyToken())));
                    }
                }
                catch { }
            }
            Console.WriteLine("");
        }

        public static bool CheckSigned(string assemblyName)
        {
            bool signed = false;
            X509Certificate basicSigner = X509Certificate.CreateFromSignedFile(assemblyName);
            X509Certificate2 cert = new X509Certificate2(basicSigner);

            if (cert != null)
            {
                signed = true;
            }
            return signed;
        }

        public static bool CheckFile(string assemblyName)
        {
            bool exe = false;
            string fileExt = Path.GetExtension(assemblyName);
            if (fileExt.ToLower() == ".exe")
            {
                exe = true;
            }
            return exe;
        }

        public static bool CheckManifest(string assemblyName)
        {
            bool hasval = true;
            var pemanifest = GetPEFileManifest(assemblyName);
            try
            {
                XmlDocument xmlinfo = new XmlDocument();
                xmlinfo.LoadXml(pemanifest.OuterXml);
                if (pemanifest.OuterXml.Contains("requestedPrivileges"))
                {

                    hasval = false;
                }
            }
            catch (Exception)
            {
                hasval = false;
            }
            return hasval;
        }

        public static bool CheckIfService(string assemblyName)
        {
            bool isservice = false;

            try
            {
                Assembly tasm = Assembly.LoadFrom(assemblyName);
                Type[] asmtypes = tasm.GetTypes();
                foreach (Type t in asmtypes)
                {
                    try
                    {
                        if (t.BaseType.FullName.Contains("System.ServiceProcess.ServiceBase"))
                        {
                            isservice = true;
                        }
                    }
                    catch { }
                }
                return isservice;
            }
            catch
            {
                return false;
            }

        }
        
        public static string GetUacInfo(string assemblyName)
        {
            string uac = string.Empty;
            try
            {
                
                var uacset = GetPEFileManifest(assemblyName);
                XmlDocument xmlinfo = new XmlDocument();
                xmlinfo.LoadXml(uacset.OuterXml);
                XmlNodeList requestedExecutionLevel = xmlinfo.GetElementsByTagName("requestedExecutionLevel");
                var uacsetting = requestedExecutionLevel[0].Attributes[0].InnerText;
                uac = uacsetting;
                return uac;
            }
            catch
            {
                uac = "No UAC settings";
                return uac;
            }
        }

        public static ProcessorArchitecture CheckArch(string assemblyName)
        {
            AssemblyName asminfo = System.Reflection.AssemblyName.GetAssemblyName(assemblyName);
            ProcessorArchitecture archtype = asminfo.ProcessorArchitecture;
            return archtype;
        }

        public static string GetDefinitionIdentity(string filename)
        {
            string identityinfo;
            Exception err;
            GetDefinitionIdentity(filename, out identityinfo, out err);
            return identityinfo;
        }

        public static bool GetDefinitionIdentity(string filename, out string identityinfo, out Exception err)
        {
            try
            {
                Assembly SystemDeploymentAssembly = Assembly.Load("System.Deployment, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                Type SystemUtilsClass = SystemDeploymentAssembly.GetType("System.Deployment.Application.Win32InterOp.SystemUtils");
                Object SystemUtils = Activator.CreateInstance(SystemUtilsClass);
                var definitionidentity = SystemUtils.GetType().InvokeMember(
                    "GetDefinitionIdentityFromManagedAssembly",
                    BindingFlags.InvokeMethod |
                    BindingFlags.NonPublic |
                    BindingFlags.Static,
                    null,
                    SystemUtils,
                    new Object[] { filename });

                identityinfo = definitionidentity.ToString();
            }
            catch (Exception e)
            {
                err = e;
                identityinfo = null;
                return false;
            }
            err = null;
            return true;
        }

        public static XmlDocument GetPEFileManifest(string filename)
        {
            XmlDocument xmld;
            Exception err;
            GetPEFileManifest(filename, out xmld, out err);

            return xmld;
        }

        public static bool GetPEFileManifest(string filename, out XmlDocument applicationXmlManifest, out Exception error)
        {
            try
            {
                if (System.String.IsNullOrEmpty(filename) == true)
                    throw new System.NullReferenceException("Parameter \"fileName\" cant be null or empty");

                if (System.IO.File.Exists(filename) == false)
                    throw new System.IO.FileNotFoundException
                        ("Parameter \"fileName\" does not point to a existing file");

                Assembly SystemDeploymentAssembly = Assembly.Load("System.Deployment, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                Type SystemUtilsClass = SystemDeploymentAssembly.GetType("System.Deployment.Application.Win32InterOp.SystemUtils");
                Object SystemUtils = Activator.CreateInstance(SystemUtilsClass);
                Byte[] ManifestBytes = SystemUtils.GetType().InvokeMember(
                    "GetManifestFromPEResources",
                    BindingFlags.InvokeMethod |
                    BindingFlags.Public |
                    BindingFlags.Static,
                    null,
                    SystemUtils,
                    new Object[] { filename }) as Byte[];

                string ManifestXmlString = string.Empty;

                using (MemoryStream ManifestBytesMemoryStream =
                                    new MemoryStream(ManifestBytes))
                using (StreamReader ManifestBytesStreamReader =
                        new StreamReader(ManifestBytesMemoryStream, true))
                {
                    ManifestXmlString = ManifestBytesStreamReader.ReadToEnd().Trim();
                }

                XmlDocument ManifestXmlDocument = new XmlDocument();

                ManifestXmlDocument.LoadXml(ManifestXmlString);

                applicationXmlManifest = ManifestXmlDocument;

                error = null;
                return true;
            }
            catch (Exception err)
            {
                error = err;
                applicationXmlManifest = null;
                return false;
            }
        }
    }
}
