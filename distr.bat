rem goto skip
c:\WINDOWS\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:Platform=AnyCPU /p:Configuration=Release hobd.csproj
c:\WINDOWS\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:Platform=WINCE /p:Configuration=Release hobd.csproj

:skip

set VERSION=0.2
set Z="C:\Program Files\7-Zip\7z.exe"

mkdir distr
mkdir distr\win32
mkdir distr\wince

copy bin\Release\hobd.exe distr\win32
copy bin\Release\Fleux.dll distr\win32
copy lib\win32\InTheHand.Net.Personal.dll distr\win32
copy config\default* distr\win32
copy config\*.lang distr\win32
copy src\elm\gen\*.ecuxml distr\win32
copy README.MD distr\win32
del distr\win32\log.txt
del distr\win32\config.xml

copy bin\Release_ce\hobd.exe distr\wince
copy bin\Release_ce\Fleux.dll distr\wince
copy lib\wince\InTheHand.Net.Personal.dll distr\wince
copy config\default* distr\wince
copy src\elm\gen\*.ecuxml distr\wince
copy config\*.lang distr\wince
copy README.MD distr\wince
del distr\wince\log.txt
del distr\wince\config.xml


%Z% a distr\hobd-%VERSION%.win32.zip .\distr\win32\*
%Z% a distr\hobd-%VERSION%.wince.zip .\distr\wince\*
