const { test, expect } = require('@playwright/test');
const AxeBuilder = require('@axe-core/playwright').default;
const password = process.env.UI_TEST_PASSWORD;

async function accessible(page) {
    const results = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa', 'wcag22aa']).analyze();
    expect(results.violations, JSON.stringify(results.violations.map(v => ({ id: v.id, nodes: v.nodes.map(n => n.target) })), null, 2)).toEqual([]);
}
async function noOverflow(page) {
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true);
}
async function login(page, email) {
    await page.goto('/Account/Login');
    await page.getByLabel('Correo electrónico', { exact: true }).fill(email);
    await page.getByLabel('Contraseña', { exact: true }).fill(password);
    await page.getByRole('button', { name: 'Entrar' }).click();
    await expect(page).not.toHaveURL(/Account\/Login/);
}
async function register(page, name, email, role) {
    await page.goto('/Account/Register');
    await page.getByLabel('Nombre completo').fill(name);
    await page.getByLabel('Correo electrónico', { exact: true }).fill(email);
    await page.getByLabel('Registrarme como', { exact: true }).selectOption(role);
    await page.getByLabel('Contraseña', { exact: true }).fill(password);
    await page.getByLabel('Confirmar contraseña').fill(password);
    await page.getByRole('button', { name: 'Crear cuenta', exact: true }).click();
    await expect(page).not.toHaveURL(/Account\/Register/);
}
async function logout(page) {
    const toggle = page.locator('[data-nav-toggle]');
    if (await toggle.isVisible() && await toggle.getAttribute('aria-expanded') === 'false') await toggle.click();
    await page.getByRole('button', { name: 'Cerrar sesión', exact: true }).click();
    await expect(page).toHaveURL(/Account\/Login/);
}

test('pantallas públicas y galería: accesibilidad, resoluciones y teclado', async ({ page }) => {
    for (const route of ['/', '/Account/Login', '/Account/Register', '/Account/AccessDenied', '/Home/Error', '/DesignSystem']) {
        await page.goto(route);
        await expect(page.locator('h1')).toBeVisible();
        await accessible(page);
        for (const width of [320, 375, 768, 1024, 1440]) {
            await page.setViewportSize({ width, height: 1000 });
            await noOverflow(page);
        }
        if (route === '/') await page.screenshot({ path: 'test-results/home-desktop.png', fullPage: true });
    }
    await page.goto('/DesignSystem');
    await page.keyboard.press('Tab');
    await expect(page.getByRole('link', { name: 'Saltar al contenido' })).toBeFocused();
    await page.keyboard.press('Enter');
    await expect(page.locator('main')).toBeFocused();
    await page.locator('main').evaluate(element => element.blur());
    await page.emulateMedia({ reducedMotion: 'reduce' });
    expect(await page.locator('[aria-busy]').evaluate(el => getComputedStyle(el, '::before').animationDuration)).toBe('1e-05s');
    await page.screenshot({ path: 'test-results/gallery-desktop.png', fullPage: true });
    await page.setViewportSize({ width: 375, height: 900 });
    await accessible(page);
    await page.screenshot({ path: 'test-results/gallery-mobile.png', fullPage: true });
    await page.locator('html').evaluate(el => el.style.fontSize = '200%');
    await noOverflow(page);
});

test('galería: búsqueda sin acentos y vista previa local con fechas válidas', async ({ page }) => {
    await page.goto('/DesignSystem');
    await page.getByLabel('Buscar en los perfiles de demostración').fill('maria');
    await expect(page.locator('[data-search-count]')).toHaveText('1 de 2 resultados');
    await page.getByLabel('Buscar en los perfiles de demostración').fill('ninguna persona');
    await expect(page.locator('[data-search-empty]')).toBeVisible();
    await page.getByRole('button', { name: 'Limpiar', exact: true }).click();
    await expect(page.locator('[data-search-count]')).toHaveText('2 de 2 resultados');
    await page.getByLabel('Paciente', { exact: true }).selectOption({ label: 'Andrés Mora' });
    await page.getByLabel('Dieta', { exact: true }).selectOption({ label: 'Variedad semanal' });
    await page.getByLabel('Inicio', { exact: true }).fill('2026-12-01');
    await page.getByLabel('Fin', { exact: true }).fill('2026-12-20');
    await page.getByRole('button', { name: 'Actualizar vista previa' }).click();
    await expect(page.locator('[data-preview-patient]')).toHaveText('Andrés Mora');
    await expect(page.locator('[data-preview-start]')).toHaveText('01/12/2026');
    await expect(page.locator('[data-preview-status]')).toContainText('no se guardan');
    await page.getByLabel('Fin', { exact: true }).fill('2026-11-20');
    await page.getByRole('button', { name: 'Actualizar vista previa' }).click();
    await expect(page.locator('[data-preview-end]')).toHaveText('20/12/2026');
});

test('validación de formularios: cliente y servidor accesibles', async ({ page }) => {
    await page.goto('/Account/Register');
    await page.getByRole('button', { name: 'Crear cuenta', exact: true }).click();
    await expect(page.locator('#FullName')).toHaveAttribute('aria-invalid', 'true');
    await expect(page.locator('#FullName-validation')).toContainText('Ingrese su nombre');
    await accessible(page);
    await page.goto('/Account/Login');
    await page.getByLabel('Correo electrónico', { exact: true }).fill('missing@example.test');
    await page.getByLabel('Contraseña', { exact: true }).fill(password);
    await page.getByRole('button', { name: 'Entrar' }).click();
    await expect(page.getByRole('alert')).toContainText('Correo o contraseña incorrectos');
    await expect(page.getByRole('alert')).toBeFocused();
    await accessible(page);
});

test('roles, creación de cuentas, asociaciones, filtros y menú móvil', async ({ browser }) => {
    test.setTimeout(120000);
    const options = { baseURL: process.env.UI_TEST_BASE_URL, viewport: { width: 1440, height: 1000 } };
    const doctorContext = await browser.newContext(options);
    const doctor = await doctorContext.newPage();
    await register(doctor, 'Andrés Médico', 'doctor-ui@example.test', 'Medico');
    await expect(doctor).toHaveURL(/Doctor/);
    for (const route of ['/Doctor/Index', '/Doctor/Patients', '/Doctor/CreatePatient', '/Doctor/AssociatePatient']) {
        await doctor.goto(route); await accessible(doctor);
    }
    await doctor.goto('/Doctor/CreatePatient');
    await doctor.getByLabel('Nombre completo').fill('María Paciente Creada');
    await doctor.getByLabel('Correo electrónico', { exact: true }).fill('created-ui@example.test');
    await doctor.getByLabel('Contraseña inicial').fill(password);
    await doctor.getByRole('button', { name: 'Crear paciente', exact: true }).click();
    await expect(doctor.locator('[data-search-item]')).toHaveCount(1);
    await expect(doctor.locator('main')).toContainText('Creado por ti');

    const patientContext = await browser.newContext(options);
    const patient = await patientContext.newPage();
    await register(patient, 'Lucía Paciente Asociada', 'patient-ui@example.test', 'Paciente');
    await expect(patient).toHaveURL(/Patient/);
    await accessible(patient);
    await doctor.goto('/Doctor/AssociatePatient');
    await doctor.getByLabel('Correo electrónico del paciente').fill('patient-ui@example.test');
    await doctor.getByRole('button', { name: 'Asociar paciente', exact: true }).click();
    await expect(doctor.locator('[data-search-item]')).toHaveCount(2);
    await doctor.getByLabel('Buscar pacientes').fill('maria');
    await expect(doctor.locator('[data-search-count]')).toHaveText('1 de 2 resultados');
    await doctor.getByRole('button', { name: 'Limpiar', exact: true }).click();
    await accessible(doctor);
    await patient.reload();
    await expect(patient.locator('main')).toContainText('Andrés Médico');
    await accessible(patient);

    const adminContext = await browser.newContext(options);
    const admin = await adminContext.newPage();
    await login(admin, process.env.UI_TEST_ADMIN_EMAIL);
    for (const route of ['/Admin/Index', '/Admin/Users', '/Admin/CreateUser']) {
        await admin.goto(route); await accessible(admin);
    }
    await admin.getByLabel('Rol', { exact: true }).selectOption('Paciente');
    await expect(admin.locator('[data-doctors-field]')).toBeVisible();
    await admin.getByRole('checkbox', { name: /Andrés Médico/ }).check();
    await admin.getByLabel('Nombre completo').fill('Paciente con nombre muy largo para comprobar la adaptación de perfiles y tablas en pantallas pequeñas');
    await admin.getByLabel('Correo electrónico', { exact: true }).fill('a-very-long-email-address-to-check-mobile-layout@example.test');
    await admin.getByLabel('Contraseña inicial').fill(password);
    await admin.getByRole('button', { name: 'Crear usuario', exact: true }).click();
    await expect(admin).toHaveURL(/Admin\/Users/);
    await admin.getByLabel('Buscar usuarios').fill('very-long');
    await expect(admin.locator('[data-search-count]')).toHaveText('1 de 5 resultados');
    await admin.getByLabel('Buscar usuarios').fill('no-coincide');
    await expect(admin.locator('[data-search-empty]')).toBeVisible();
    await admin.getByRole('button', { name: 'Limpiar', exact: true }).click();

    for (const page of [doctor, patient, admin]) {
        for (const width of [320, 375, 768, 1024, 1440]) {
            await page.setViewportSize({ width, height: 1000 });
            await noOverflow(page);
            if (width < 768 && await page.locator('.ui-table-region').count()) {
                const region = page.locator('.ui-table-region');
                expect(await region.evaluate(el => el.scrollWidth > el.clientWidth)).toBe(true);
                await region.focus();
                await page.keyboard.press('ArrowRight');
                await expect.poll(() => region.evaluate(el => el.scrollLeft)).toBeGreaterThan(0);
            }
            if (width < 640 && await page.locator('[data-search-input]').count()) {
                const field = await page.locator('[data-search-input]').boundingBox();
                const clear = await page.locator('[data-search-clear]').boundingBox();
                expect(clear.y).toBeGreaterThanOrEqual(field.y + field.height);
            }
            const toggle = page.locator('[data-nav-toggle]');
            if (width < 1024) {
                await expect(toggle).toBeVisible();
                await toggle.click();
                await expect(toggle).toHaveAttribute('aria-expanded', 'true');
                await page.getByRole('navigation', { name: 'Navegación principal' }).getByRole('link').first().focus();
                await page.keyboard.press('Escape');
                await expect(toggle).toBeFocused();
                await expect(toggle).toHaveAttribute('aria-expanded', 'false');
            } else await expect(toggle).toBeHidden();
        }
        await accessible(page);
    }
    await admin.screenshot({ path: 'test-results/admin-desktop.png', fullPage: true });
    await admin.setViewportSize({ width: 375, height: 900 });
    await admin.screenshot({ path: 'test-results/admin-mobile.png', fullPage: true });
    await doctor.goto('/Doctor/Patients');
    await expect(doctor.locator('[data-search-item]')).toHaveCount(3);
    // Switching away from Patient must not submit previously checked DoctorIds.
    await admin.goto('/Admin/CreateUser');
    await admin.getByLabel('Rol', { exact: true }).selectOption('Paciente');
    await admin.getByRole('checkbox').check();
    await admin.getByLabel('Rol', { exact: true }).selectOption('Medico');
    await expect(admin.locator('[data-doctors-field]')).toBeHidden();
    expect(await admin.locator('form.ui-form').evaluate(form => new FormData(form).getAll('DoctorIds'))).toEqual([]);
    await logout(doctor); await login(doctor, 'doctor-ui@example.test');
    await logout(patient); await login(patient, 'patient-ui@example.test');
    await logout(admin);
    await doctorContext.close(); await patientContext.close(); await adminContext.close();
});

test('producción: galería cerrada y estilos locales disponibles', async ({ request }) => {
    const production = process.env.UI_TEST_PRODUCTION_URL;
    for (const path of ['/DesignSystem', '/DesignSystem/Index']) expect((await request.get(production + path)).status()).toBe(404);
    const home = await request.get(production);
    expect(await home.text()).not.toContain('Galería de componentes');
    const css = await request.get(production + '/css/site.css');
    expect(css.status()).toBe(200);
    expect(await css.text()).toContain('--color-evergreen');
});
