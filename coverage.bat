.\packages\OpenCover.4.6.166\tools\OpenCover.Console.exe -register:user "-filter:+[Swashbuckle.OData]* -[*Test]*" "-target:.\packages\NUnit.Runners.2.6.4\tools\nunit-console-x86.exe" "-targetargs:/noshadow .\Swashbuckle.OData.Tests\bin\Debug\Swashbuckle.OData.Tests.dll"

.\packages\ReportGenerator.2.3.5.0\tools\ReportGenerator.exe "-reports:results.xml" "-targetdir:.\coverage"

pause