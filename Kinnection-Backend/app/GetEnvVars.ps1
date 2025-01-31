# GetEnvVars.ps1
# Copies environment variables from the Docker directory to run ASP.NET Core
# app locally. 
#
# NOTE: Must navigate to 'Kinnection/Kinnection-Backend/app' before use
# To execute in Powershell, use './GetEnvVars.ps1'

# Load environment variables from the .env file
$envFile = "..\..\Docker\.env"

# Read the .env file line by line and set environment variables
Get-Content $envFile | ForEach-Object {
    if ($_ -match "^\s*([A-Za-z0-9_]+)=(.*)\s*$") {
        $envName = $matches[1]
        $envValue = $matches[2]
        [System.Environment]::SetEnvironmentVariable($envName, $envValue, [System.EnvironmentVariableTarget]::Process)
    }
}

# Remove the internal host var so the external var is used
$Env:MYSQL_HOST = ""