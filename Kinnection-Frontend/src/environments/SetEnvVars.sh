#!/bin/bash
# Creates a new 'environment.ts' file with template env vars filled in

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
        echo "${envName}: ${envValue}"
        export "$envName"="$envValue"
    fi

done < "$envFile"

# Remove the internal host var so the external var is used
unset MYSQL_HOST

TEMPLATE_FILE="./environment.template.ts"
PRODUCTION_FILE="./environment.ts"
DEVELOPMENT_FILE="./environment.development.ts"

# Replace environment variables in the template file
envsubst < "$TEMPLATE_FILE" > "$PRODUCTION_FILE"
envsubst < "$TEMPLATE_FILE" > "$DEVELOPMENT_FILE"