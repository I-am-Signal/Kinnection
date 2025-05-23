# Creates a new 'environment.ts' file with template env vars filled in


param( [string]$e )
$envFile = "..\..\Docker\.env"
if ($e) { $envFile = $e }

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

$templateFile = "./environment.template.ts"
$productionFile = "./environment.ts"
$developmentFile = "./environment.development.ts"

# Read the template and expand environment variables
$expanded = [regex]::Replace((Get-Content $templateFile -Raw), '\$\{?(\w+)\}?', {
    param($match)
    $varName = $match.Groups[1].Value
    $value = (Get-Item "Env:$varName").Value
    return $value
})

# Output to desired files
$expanded | Set-Content $productionFile
$expanded | Set-Content $developmentFile
