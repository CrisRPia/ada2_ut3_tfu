import { ConfigExternal } from "orval";

export default {
    backend: {
        input: {
            target: "./backend.playlists.schema.json",
        },
        output: {
            baseUrl: "http://localhost:8080/api/playlists",
            target: "backend.playlists.api.ts",
            prettier: true,
            client: "fetch",
        },
    },
} satisfies ConfigExternal;
