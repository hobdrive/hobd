@echo off

rem set Z="C:\Program Files\7-Zip\7z.exe"
set Z=C:\programs\arc\7z.exe

set VERSION=0.3

FOR /F "tokens=*" %%i in ('DATE /T') do SET DATE=%%i 
FOR /F "tokens=*" %%i in ('TIME /T') do SET TIME=%%i 
SET TS=%DATE%%TIME%

echo using System; > src/HOBDBuild.cs
echo namespace hobd >> src/HOBDBuild.cs
echo { >> src/HOBDBuild.cs
echo sealed class HOBDBuild >> src/HOBDBuild.cs
echo { >> src/HOBDBuild.cs
echo     public const string Version   = "%VERSION%"; >> src/HOBDBuild.cs
echo     public const string TimeStamp = "%TS%"; >> src/HOBDBuild.cs
echo } >> src/HOBDBuild.cs
echo } >> src/HOBDBuild.cs

 goto skip
c:\WINDOWS\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:Platform=AnyCPU /p:Configuration=Release hobd.csproj
c:\WINDOWS\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:Platform=WINCE /p:Configuration=Release hobd.csproj
:skip

mkdir distr
del /F /Q distr\win32
del /F /Q distr\wince
mkdir distr\win32
mkdir distr\win32\sensors
mkdir distr\wince
mkdir distr\wince\sensors

copy bin\Release\hobd.exe distr\win32
copy bin\Release\Fleux.dll distr\win32
copy lib\win32\InTheHand.Net.Personal.dll distr\win32
xcopy /E /C /Y config\*.* distr\win32
copy src\elm\gen\*.ecuxml distr\win32\sensors
copy README.MD distr\win32
del distr\win32\log.txt
del distr\win32\config.xml

copy bin\Release_ce\hobd.exe distr\wince
copy bin\Release_ce\Fleux.dll distr\wince
copy lib\wince\InTheHand.Net.Personal.dll distr\wince
xcopy /E /C /Y config\*.* distr\wince
copy src\elm\gen\*.ecuxml distr\wince\sensors
copy README.MD distr\wince
del distr\wince\log.txt
del distr\wince\config.xml

del distr\hobd-%VERSION%.win32.zip
del distr\hobd-%VERSION%.wince.zip

%Z% a distr\hobd-%VERSION%.win32.zip .\distr\win32\*
%Z% a distr\hobd-%VERSION%.wince.zip .\distr\wince\*
