xcopy /Y /E config bin\Debug\

c:\WINDOWS\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:Platform=AnyCPU hobd.csproj
copy lib\win32\InTheHand.Net.Personal.dll bin\Debug\
c:\WINDOWS\Microsoft.NET\Framework\v3.5\MSBuild.exe /p:Platform=WINCE hobd.csproj