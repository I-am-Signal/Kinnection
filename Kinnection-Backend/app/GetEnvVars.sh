#!/bin/bash
# GetEnvVars.sh
# Copies environment variables from the Docker directory to run ASP.NET Core
# app locally.
#
# NOTE: Must navigate to 'Kinnection/Kinnection-Backend/app' before use
# To execute in Bash, use './GetEnvVars.sh'

# Load environment variables from the .env file
envFile="../../Docker/.env"

# Read the .env file line by line and set environment variables
while IFS='=' read -r envName envValue; do
    # Skip empty lines or lines starting with a comment
    if [[ -n "$envName" && ! "$envName" =~ ^# ]]; then
        export "$envName"="$envValue"
    fi
done < "$envFile"

# Remove the internal host var so the external var is used
unset MYSQL_HOST