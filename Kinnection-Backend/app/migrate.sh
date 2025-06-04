#!/bin/bash

set -e  # Exit on error

# Read in the environment variables
HOST="${MYSQL_HOST:-localhost}"
PORT="${MYSQL_PORT:-3306}"
USER="${MYSQL_USER:-root}"
PASSWORD="${MYSQL_PASSWORD:-}"

ATTEMPTS="${RETRY_ATTEMPTS}"
RETRY_IN="${RETRY_IN}"

success="false"

# Attempt to connect to the database
for ((i=1; i<ATTEMPTS+1; i++)); do
    echo "Pinging MySQL at $HOST:$PORT (Attempt $i of $ATTEMPTS)..."
    if mysqladmin ping -h"$HOST" -P"$PORT" -u"$USER" -p"$PASSWORD" --silent; then
        success="true";
        break
    fi
    echo "mysqld not up yet. Retrying in $RETRY_IN seconds..."
    sleep $RETRY_IN
done

# Final check
if [ "$success" = "false" ]; then
    echo "Could not connect to MySQL after $ATTEMPTS attempts."
    exit 1
fi

# Create migration
migration_name=$(shuf -i 10000000-99999999 -n 1)
echo "Creating migration: $migration_name"
dotnet ef migrations add "$migration_name"

exit 0

./migrate.sh: line 20: success: command not found