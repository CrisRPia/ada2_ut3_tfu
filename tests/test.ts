import * as auth from "./codegen/backend.auth.api.ts"
import * as songs from "./codegen/backend.songs.api.ts"
import * as playlists from "./codegen/backend.playlists.api.ts"
import { assert, deepLog, delay } from "./helpers.ts";

async function main() {
    assert(deepLog(await auth.getPing()).status === 200, {});
    assert(deepLog(await songs.getPing()).status === 200, {});
    assert(deepLog(await playlists.getPing()).status === 200, {});
}

main();
