dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.Extensions.Logging
dotnet add package Microsoft.Extensions.Logging.Console
dotnet add package OpenTelemetry.Exporter.Console

# using System.Configuration; // for ConfigurationErrorsException: 
dotnet add package System.Configuration.ConfigurationManager

# for *.prompty file support (no usings needed)
dotnet add package Microsoft.SemanticKernel.Prompty --prerelease

# RAG/web search
dotnet add package Microsoft.SemanticKernel.Plugins.Web --version 1.30.0-alpha

# see what you hath wrought
dotnet list package
