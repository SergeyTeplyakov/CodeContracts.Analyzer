@echo Off
set config=%1
if "%config%" == "" (
   set config=Debug
)
 
set version=0.1.0

if not "%PackageVersion%" == "" (
   set version=%PackageVersion%
)
 
set nuget=".\src\packages\NuGet.CommandLine.2.8.2\tools\nuget"

if "%nuget%" == "" (
	set nuget=nuget
)
 
set PATH=C:\Program Files (x86)\MSBuild\14.0\Bin;%PATH%

msbuild src\CodeContractor.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false /t:Rebuild

COPY src\CodeContractor.Vsix\bin\%config%\*.vsix BUILD