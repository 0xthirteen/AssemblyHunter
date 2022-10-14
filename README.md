# AssemblyHunter
Tool released in combination with the [Less SmartScreen More Caffeine: ClickOnce (Ab)Use for Trusted Code Execution](https://media.defcon.org/DEF%20CON%2030/DEF%20CON%2030%20video%20and%20slides/DEF%20CON%2030%20-%20Nick%20Powers%2C%20Steven%20Flores%20-%20Less%20SmartScreen%20More%20Caffeine%20-%20ClickOnce%20AbUse%20for%20Trusted%20Code%20Execution.mp4)  conference presentation by [zyn3rgy](https://twitter.com/zyn3rgy) and myself.


Find assemblies on hosts that can be useful for payloads or post ex. No pre-built assemblies will be provided, open project, select release and build. Build for .Net Framework 4.0+ (some assemblies are not identified correctly less than 4.0)

##  Core Options:
*    path         (ex: path=C:\Users)           full path to search
*    file         (ex: file=C:\file.exe)        check if a specific file is an assembly
*    collection   (ex: collection=C:\files.txt) check a list of assemblies from a file
*    services     (ex: services=true)           check all services binpaths for any assemblies
*    tasks        (ex: tasks=true)              check if any exec action tasks are assemblies
*    autoruns     (ex: autoruns=true)           enumerates common autorun locations for assemblies

## Optional
*    recurse          (ex: recurse=true)          recurse the path given
*    allpaths         (ex: allpaths=true)         recurses all directores, by default some directores with common Microsoft assemblies are skipped
*    exeonly          (ex: exeonly=true)          return exes only
*    getarch          (ex: getarch=true)          get assembly architecture
*    servicename      (ex: services=true)         check a specific service (needs services run)
*    isservice        (ex: iservice=true)         checks if assembly is a service executable
*    getuac           (ex: getuac=true)           gets UAC settings of assembly
*    getrefs          (ex: getrefs=true)          gets references used by assembly
*    getasmid         (ex: getasmid=true)         gets internal assembly manifest identity");
*    getappid         (ex: getappid=true)         gets internal application manifest identity");
*    getappmanifest   (ex: getappmanifest=true)   gets internal application manifest");
*    getasmmanifest   (ex: getasmmanifest=true)   gets internal assembly manifest");
*    clickonce        (ex: clickonce=true)        returns assemblies that can be deployed via clickonce
*    electron         (ex: electron=true)         finds electron apps instead of assemblies

path, file, collection, services, tasks, or autoruns should indicate the type of search performed, all other options narrow down the search

#### Examples: 
##### AssemblyHunter.exe path=C:\ recurse=true signed=true
##### AssemblyHunter.exe path=C:\Users\Admin\Downloads recurse=true clickonce=true
##### AssemblyHunter.exe services=true signed=true
##### AssemblyHunter.exe tasks=true signed=true getarch=true
##### AssemblyHunter file=C:\Users\admin\elevate.exe getarch=true

#### Credit
GetPEFileManifest from Kerem Guemruekcue

