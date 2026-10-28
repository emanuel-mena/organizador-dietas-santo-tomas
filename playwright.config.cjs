const { defineConfig } = require('@playwright/test');
const path = require('node:path');

if (!process.env.UI_TEST_CONNECTION) {
    throw new Error('Ejecute npm run test:ui para usar una base de datos PostgreSQL temporal.');
}
const baseURL = process.env.UI_TEST_BASE_URL;
const productionURL = process.env.UI_TEST_PRODUCTION_URL;
const environment = {
    ConnectionStrings__DefaultConnection: process.env.UI_TEST_CONNECTION,
    SeedAdmin__Email: process.env.UI_TEST_ADMIN_EMAIL,
    SeedAdmin__Password: process.env.UI_TEST_PASSWORD,
    Logging__LogLevel__Default: 'Warning'
};
module.exports = defineConfig({
    testDir: './tests/ui',
    fullyParallel: false,
    workers: 1,
    timeout: 60000,
    expect: { timeout: 10000 },
    reporter: [['list']],
    use: { baseURL, channel: 'chromium', viewport: { width: 1440, height: 1000 }, trace: 'retain-on-failure', screenshot: 'only-on-failure' },
    webServer: [
        { command: 'dotnet bin/Debug/net10.0/organizador-dietas-santo-tomas.dll', url: baseURL, reuseExistingServer: false, timeout: 90000,
            env: { ...environment, ASPNETCORE_ENVIRONMENT: 'Development', ASPNETCORE_URLS: baseURL } },
        { command: 'dotnet organizador-dietas-santo-tomas.dll', cwd: path.join(__dirname, 'bin/Release/net10.0/publish'), url: productionURL, reuseExistingServer: false, timeout: 90000,
            env: { ...environment, ASPNETCORE_ENVIRONMENT: 'Production', ASPNETCORE_URLS: productionURL } }
    ]
});
