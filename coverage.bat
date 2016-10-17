set OpenCoverVersion=4.6.519
set ReportGeneratorVersion=2.4.3.0
set NUnitRunnersVersion=2.6.4

.\packages\OpenCover.%OpenCoverVersion%\tools\OpenCover.Console.exe -register:user "-filter:+[Swashbuckle.OData]* -[Swashbuckle.OData]System.*" "-target:.\packages\NUnit.Runners.%NUnitRunnersVersion%\tools\nunit-console-x86.exe" "-targetargs:/noshadow .\Swashbuckle.OData.Tests\bin\Debug\Swashbuckle.OData.Tests.dll"

.\packages\ReportGenerator.%ReportGeneratorVersion%\tools\ReportGenerator.exe "-reports:results.xml" "-targetdir:.\coverage"

pause