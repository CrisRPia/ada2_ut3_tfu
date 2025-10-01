#!/usr/bin/env bash

for service in "auth" "songs" "playlists"; do
    # Get schema
    curl "http://localhost:8080/api/$service/swagger/v1/swagger.json" \
        > "./backend.$service.schema.json"

    # Generate frontend client
    orval --config "./orval.$service.config.ts"
done
