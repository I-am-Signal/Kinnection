#!/bin/bash
# GetEnvVars.sh
# Copies environment variables from the Docker directory to run ASP.NET Core
# app locally. 
#
# Args:
#   -e /path/to/.env
#
# If using without -e argument, the terminal must be navigated to 
# 'Kinnection/Kinnection-Backend/app' before use.
# To execute in Bash, use './GetEnvVars.sh -e /path/to/.env'

# Load environment variables from the .env file
envFile="../../Docker/.env"
while getopts "e:" opt; do
    case $opt in
        e)
            envFile="$OPTARG"
            ;;
    esac
done
            

# Read the .env file line by line and set environment variables
while IFS='=' read -r envName envValue; do
    # Trim whitespace
    envName="$(echo "$envName" | xargs)"
    envValue="$(echo "$envValue" | xargs)"

    # Skip comments only
    if [[ "$envName" == \#* || -z "$envName" ]]; then
        continue
    fi

    # Export even if envValue is empty
    if [[ "$envName" =~ ^[A-Za-z_][A-Za-z0-9_]*$ ]]; then
        export "$envName"="$envValue"
    fi

done < "$envFile"

# Remove the internal host var so the external var is used
unset MYSQL_HOST