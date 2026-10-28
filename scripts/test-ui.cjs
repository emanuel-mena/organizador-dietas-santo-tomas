// Isolated PostgreSQL cluster + two application hosts. Never reads user-secrets.
const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const net = require('node:net');
const crypto = require('node:crypto');
const { spawnSync, spawn } = require('node:child_process');

function findPostgres() {
    if (process.env.POSTGRES_BIN) return process.env.POSTGRES_BIN;
    if (process.platform === 'win32') {
        const root = path.join(process.env.ProgramFiles || 'C:/Program Files', 'PostgreSQL');
        if (fs.existsSync(root)) {
            for (const version of fs.readdirSync(root).sort((a, b) => b.localeCompare(a, undefined, { numeric: true }))) {
                const bin = path.join(root, version, 'bin');
                if (fs.existsSync(path.join(bin, 'initdb.exe'))) return bin;
            }
        }
    }
    return ''; // Fall back to PATH on Linux/macOS.
}
function run(command, args, options = {}) {
    const result = spawnSync(command, args, { encoding: 'utf8', windowsHide: true, timeout: 120000, ...options });
    if (result.error || result.status !== 0) throw new Error(result.error?.message || result.stderr || result.stdout || `${command} falló.`);
    return result;
}
async function freePort() {
    const server = net.createServer();
    await new Promise((resolve, reject) => { server.once('error', reject); server.listen(0, '127.0.0.1', resolve); });
    const port = server.address().port;
    await new Promise(resolve => server.close(resolve));
    return port;
}
async function main() {
    const bin = findPostgres();
    const tool = name => bin ? path.join(bin, name + (process.platform === 'win32' ? '.exe' : '')) : name;
    const temporary = fs.mkdtempSync(path.join(os.tmpdir(), 'santo-tomas-ui-'));
    const data = path.join(temporary, 'pgdata');
    let started = false;
    let child;
    const stop = () => {
        if (started) {
            run(tool('pg_ctl'), ['-D', data, '-m', 'fast', '-w', 'stop']);
            started = false;
        }
    };
    const onSignal = () => { child?.kill(); stop(); process.exitCode = 130; };
    process.once('SIGINT', onSignal);
    process.once('SIGTERM', onSignal);
    try {
        const ports = new Set();
        while (ports.size < 3) ports.add(await freePort());
        const [pgPort, devPort, prodPort] = [...ports];
        run(tool('initdb'), ['-D', data, '-U', 'postgres', '--auth=trust', '--encoding=UTF8', '--locale=C']);
        // Paths with spaces are quoted for pg_ctl's internal command construction.
        run(tool('pg_ctl'), ['-D', data, '-l', path.join(temporary, 'postgres.log'), '-o', `-p ${pgPort} -h 127.0.0.1`, '-w', 'start'], { stdio: 'ignore' });
        started = true;
        console.log('PostgreSQL temporal iniciado. Compilando la aplicación…');
        const build = run('dotnet', ['build', '--nologo'], { stdio: 'inherit' });
        if (build.status !== 0) throw new Error('Falló la compilación.');
        run('dotnet', ['publish', '-c', 'Release', '--nologo'], { stdio: 'inherit' });
        const env = {
            ...process.env,
            UI_TEST_CONNECTION: `Host=127.0.0.1;Port=${pgPort};Database=postgres;Username=postgres`,
            UI_TEST_BASE_URL: `http://127.0.0.1:${devPort}`,
            UI_TEST_PRODUCTION_URL: `http://127.0.0.1:${prodPort}`,
            UI_TEST_ADMIN_EMAIL: 'admin-ui@example.test',
            UI_TEST_PASSWORD: `Ui-Test_${crypto.randomBytes(12).toString('hex')}!`
        };
        child = spawn(process.execPath, [require.resolve('@playwright/test/cli'), 'test', ...process.argv.slice(2)], { stdio: 'inherit', env, windowsHide: true });
        process.exitCode = await new Promise((resolve, reject) => { child.once('error', reject); child.once('exit', code => resolve(code ?? 1)); });
    } finally {
        stop();
        // Delete only the exact directory created by mkdtemp above, after PostgreSQL stops.
        const resolved = path.resolve(temporary);
        if (path.dirname(resolved) === path.resolve(os.tmpdir()) && path.basename(resolved).startsWith('santo-tomas-ui-')) {
            fs.rmSync(resolved, { recursive: true, force: true });
        }
        process.removeListener('SIGINT', onSignal);
        process.removeListener('SIGTERM', onSignal);
    }
}
main().catch(error => { console.error(error.message); process.exitCode = 1; });
