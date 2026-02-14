dotnet clean
dotnet build --configuration Release
dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/HtmlReport" -reporttypes:Html
Start-Process "./TestResults/HtmlReport/index.html"
