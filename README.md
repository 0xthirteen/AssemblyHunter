# AssemblyHunter.exe
Find assemblies on a host that can be useful for payloads or post ex.

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
AssemblyHunter.exe path=C:\ recurse=true signed=true
AssemblyHunter.exe path=C:\Users\Admin\Downloads recurse=true clickonce=true
AssemblyHunter.exe services=true signed=true
AssemblyHunter.exe tasks=true signed=true getarch=true
AssemblyHunter file=C:\Users\admin\elevate.exe getarch=true

##### Credit
GetPEFileManifest from Kerem Guemruekcue