clean && dotnet restore && dotnet build src\Remote.Linq src\Remote.Linq.Newtonsoft.Json test\Remote.Linq.Tests && dotnet test test\Remote.Linq.Tests && dotnet pack src\Remote.Linq --output artifacts --configuration Debug --version-suffix 013 && dotnet pack src\Remote.Linq.Newtonsoft.Json --output artifacts --configuration Debug --version-suffix 013