set OpenCoverVersion=4.6.519
set ReportGeneratorVersion=2.5.8
set NUnitRunnersVersion=3.6.1

.\packages\OpenCover.%OpenCoverVersion%\tools\OpenCover.Console.exe -register:user "-filter:+[Swashbuckle.OData]* -[Swashbuckle.OData]System.*" "-target:.\packages\NUnit.ConsoleRunner.%NUnitRunnersVersion%\tools\nunit3-console.exe" "-targetargs:/noshadow .\Swashbuckle.OData.Tests\bin\Debug\Swashbuckle.OData.Tests.dll"

.\packages\ReportGenerator.%ReportGeneratorVersion%\tools\ReportGenerator.exe "-reports:results.xml" "-targetdir:.\coverage"

pause