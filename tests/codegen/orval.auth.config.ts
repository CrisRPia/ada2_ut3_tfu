import { ConfigExternal } from "orval";

export default {
    backend: {
        input: {
            target: "./backend.auth.schema.json",
        },
        output: {
            baseUrl: "http://localhost:8080/api/auth",
            target: "backend.auth.api.ts",
            prettier: true,
            client: "fetch",
        },
    },
} satisfies ConfigExternal;
