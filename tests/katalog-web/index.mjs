// `node --test tests/katalog-web/` runs this directory as one test file: Node resolves the directory
// through package.json "main" to here, and every *.test.mjs beside it is loaded.
// `node --test "tests/katalog-web/*.test.mjs"` runs them as separate files instead.
import { readdirSync } from 'node:fs';

const dir = new URL('./', import.meta.url);
for (const name of readdirSync(dir).filter(n => n.endsWith('.test.mjs')).sort()) {
    await import(new URL(name, dir).href);
}
