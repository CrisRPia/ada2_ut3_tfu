import { ConfigExternal } from "orval";

export default {
    backend: {
        input: {
            target: "./backend.songs.schema.json",
        },
        output: {
            baseUrl: "http://localhost:8080/api/songs",
            target: "backend.songs.api.ts",
            prettier: true,
            client: "fetch",
        },
    },
} satisfies ConfigExternal;
