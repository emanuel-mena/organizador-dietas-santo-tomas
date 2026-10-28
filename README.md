# Organizador de dietas Santo Tomás

Aplicación ASP.NET Core MVC (.NET 10), Identity, EF Core y PostgreSQL, con roles **Administrador**, **Médico** y **Paciente**. La interfaz usa Tailwind CSS 4 y un sistema visual propio.

## Inicio local

Requisitos: .NET 10 y PostgreSQL. Para modificar estilos: Node.js 22 o posterior (recomendado 24 LTS) y npm. Los estilos compilados están versionados; ejecutar una copia del proyecto con `dotnet run` no requiere Node.

Configure secretos del proyecto sin incluir credenciales en Git:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=organizador_dietas_santo_tomas;Username=USUARIO;Password=CLAVE"
dotnet user-secrets set "SeedAdmin:Email" "admin@ejemplo.com"
dotnet user-secrets set "SeedAdmin:Password" "CONTRASEÑA_SEGURA"
dotnet run
```

Al iniciar se aplican las migraciones pendientes, se crean los roles y, si no existe un administrador, se crea la cuenta configurada. En producción se pueden usar las variables `ConnectionStrings__DefaultConnection`, `SeedAdmin__Email` y `SeedAdmin__Password`. Para aplicar migraciones manualmente: `dotnet ef database update`.

## Desarrollo y compilación de estilos

```powershell
npm ci
npm run build:css
dotnet run
```

| Comando | Resultado |
| --- | --- |
| `npm run build:css` | Compila y minifica el CSS con Tailwind CLI. |
| `npm run watch:css` | Recompila al modificar vistas, JavaScript o el CSS de origen. Mantener en otra terminal mientras corre `dotnet watch`. |
| `npm run build` | Genera los estilos y ejecuta `dotnet build`. |
| `npm run publish` | Genera los estilos y ejecuta `dotnet publish -c Release`. |
| `npm run test:ui` | Compila estilos y aplicación; ejecuta pruebas de navegador con PostgreSQL temporal. |

`Styles/app.css` es la fuente de verdad: tema, estilos base y clases de componentes. `wwwroot/css/site.css` es generado; **no editarlo manualmente**. Incluir ambos en el commit cuando cambien los estilos. El CLI y sus dependencias se resuelven de forma reproducible mediante `package-lock.json`.

La aplicación sirve CSS, JavaScript e iconos locales. Bootstrap ya no se carga. jQuery y la validación unobtrusive se conservan para los formularios MVC. La publicación contiene el CSS compilado y no necesita npm en el servidor.

### Añadir estilos o variantes

1. Modificar tokens en `@theme` o añadir una clase `ui-*` dentro de `@layer components`.
2. Usar utilidades de Tailwind para composición, espaciado y puntos de cambio; clases `ui-*` para patrones repetidos.
3. Escribir nombres de clases completos en Razor. Evitar concatenaciones como `bg-@color`: el detector necesita ver `bg-hunter-green` literalmente. Usar un `switch` con clases completas para variantes.
4. Las directivas `@source` incluyen explícitamente `Views/**/*.cshtml` y `wwwroot/js/**/*.js`. Registrar otra fuente si se crean componentes en otra ubicación.
5. Ejecutar `npm run build:css`, añadir el ejemplo a la galería y actualizar este catálogo.

Referencia técnica: [Tailwind CLI](https://tailwindcss.com/docs/installation/tailwind-cli), [variables de tema](https://tailwindcss.com/docs/theme), [detección de clases](https://tailwindcss.com/docs/detecting-classes-in-source-files).

## Identidad visual

Diseño claro inspirado en un panel de bienestar: superficies blancas, fondo verde muy suave, tarjetas redondeadas, sombras discretas y tipografía del sistema. Los iconos se dibujan con SVG locales.

| Token CSS / Tailwind | Hex | Uso |
| --- | --- | --- |
| `--evergreen` / `--color-evergreen` | `#132a13` | Texto principal y superficies oscuras. |
| `--hunter-green` / `--color-hunter-green` | `#31572c` | Botones principales y navegación activa. |
| `--fern` / `--color-fern` | `#4f772d` | Foco e indicadores. |
| `--palm-leaf` / `--color-palm-leaf` | `#90a955` | Detalles y superficies con texto oscuro. |
| `--lime-cream` / `--color-lime-cream` | `#ecf39e` | Tarjetas y acciones destacadas con texto oscuro. |

Las variables cortas originales se exponen como alias; los tokens `--color-*` generan utilidades como `bg-lime-cream` y `text-evergreen`.

Otros tokens: `canvas` (#f2f5ed), `muted` (#52614f), `line` (#d9e1d3), `danger` (#a52330), `danger-soft` (#fff0f1), `warning` (#744500), `warning-soft` (#fff5dc), además de blanco. Los colores semánticos se acompañan siempre de texto.

Combinaciones de referencia (contraste aproximado):
- Blanco / hunter green: **8.30:1**.
- Blanco / fern: **5.24:1**.
- Evergreen / palm leaf: **5.84:1**.
- Evergreen / lime cream: **13.07:1**.
- No usar texto blanco sobre palm leaf: **2.63:1**.

### Adaptación a pantallas

- Desde **1024 px**: navegación lateral de 256 px, con iconos y etiquetas.
- Por debajo de 1024 px: menú desplegable; comunica `aria-expanded` y permite cerrar con Escape desde el menú y devolver foco al botón.
- Sin JavaScript el menú sigue disponible. Las listas conservan todos sus elementos.
- Las tarjetas y formularios pasan a una columna en móvil. Las tablas tienen una región enfocada y desplazable; el desplazamiento horizontal queda dentro de ella.
- El menú conserva las opciones autorizadas para cada rol y marca la ruta actual mediante `aria-current="page"`.

## Catálogo de componentes

Los controles básicos usan HTML nativo y clases CSS. Las composiciones están en `Views/Shared/UI` y reciben modelos de presentación definidos en `Models/DesignSystemViewModels.cs`; no son entidades de EF Core.

### Controles y estructura

| Clase / patrón | Uso |
| --- | --- |
| `ui-btn` + `ui-btn-primary` | Acción principal. |
| `ui-btn-secondary`, `ui-btn-ghost` | Alternativas con borde o discretas. |
| `ui-btn-danger`, `ui-btn-accent` | Acción destructiva o destacada. |
| `ui-input` | Texto, correo, contraseña, búsqueda, número, fecha, select o textarea. |
| `ui-field`, `ui-label`, `ui-hint`, `ui-error` | Agrupación, etiqueta, ayuda y error. |
| `ui-choice` | Etiqueta de casilla o radio, con área táctil amplia. Agrupar con fieldset/legend. |
| `ui-validation` | Resumen MVC con `asp-validation-summary`. |
| `ui-card`, `ui-card-accent`, `ui-card-dark` | Superficies normal, destacada y oscura. |
| `ui-page-header`, `ui-eyebrow`, `ui-description`, `ui-actions` | Encabezados y composición de acciones. |
| `ui-table-region`, `ui-table` | Tabla semántica dentro de región desplazable. |
| `ui-progress` | Elemento nativo progress con etiqueta. |
| `ui-link` | Enlace dentro de texto. |
| `ui-sidebar`, `ui-nav-link`, `ui-topbar`, `ui-main` | Estructura compartida de la aplicación. |

Estados:
- **Normal / hover / foco / pulsación:** definidos por la variante; foco visible con contorno de 3 px.
- **Deshabilitado:** usar `disabled` en controles nativos. Para navegación usar un enlace solo cuando haya un destino real.
- **Carga:** botón con `disabled aria-busy="true"` y texto como “Guardando…”. El indicador respeta movimiento reducido. El código que realice una operación debe activar y restaurar este estado.
- **Error:** `aria-invalid="true"` y `aria-describedby` apuntando a un mensaje; MVC también añade `input-validation-error`. El JavaScript sincroniza el atributo accesible con la validación cliente y enfoca el resumen o primer error al recibir una respuesta inválida.

Ejemplo Razor que conserva el enlace de modelos:

```cshtml
<div class="ui-field">
    <label asp-for="Email" class="ui-label"></label>
    <input asp-for="Email" class="ui-input"
           autocomplete="username" aria-describedby="Email-validation" />
    <span asp-validation-for="Email" id="Email-validation" class="ui-error"></span>
</div>
<button type="submit" class="ui-btn ui-btn-primary">Guardar</button>
```

Usar un ID exterior terminado en `-validation` evita colisionar con los IDs `-error` que genera jQuery Validate. Cada ID debe ser único en su documento. Los formularios conservan las rutas, antiforgery, `asp-for`, `asp-validation-for` y la validación del servidor.

### Parciales tipados

| Parcial | Modelo y parámetros |
| --- | --- |
| `UI/_Avatar` | `AvatarUiModel(Name, Large=false)`: iniciales de los primeros dos nombres; respaldo “?”. Decorativo si acompaña al nombre visible. |
| `UI/_Profile` | `ProfileUiModel(Name, Email, Role, Detail=null)`: avatar y datos visibles; sin enlaces de perfil ficticios. |
| `UI/_Stat` | `StatUiModel(Label, Value, Description, Icon="chart")`: tarjeta de resumen. |
| `UI/_Alert` | `AlertUiModel(Title, Message=null, Tone="success")`: confirmación, advertencia o error. Error usa role alert; otros, status. |
| `UI/_Badge` | `BadgeUiModel(Text, Tone="neutral")`: estado con texto. |
| `UI/_Empty` | `EmptyUiModel(Title, Description, Icon="leaf")`: contenido vacío o búsqueda sin coincidencias. |
| `UI/_Diet` | `DietUiModel(Name, Description, Calories, Carbohydrates, Protein, Fat, Status="Borrador")`: kcal diarias y macronutrientes en gramos. |
| `UI/_Meal` | `MealUiModel(Name, Time, Food, Portion, Calories)`: comida con horario, porción y energía. |
| `UI/_Assignment` | `AssignmentUiModel(PatientName, DietName, Start, End, Status="Pendiente")`: fechas DateOnly con formato dd/MM/yyyy. |
| `UI/_Search` | `SearchUiModel(Id, Label, Placeholder)`: entrada etiquetada, limpieza, contador y estado vacío. El ID debe ser único. |
| `UI/_UserFields` | `CreateUserViewModel`: nombre, correo y contraseña inicial compartidos por administración y médico. |
| `UI/_Icon` | Cadena: home, users, plus, link, search, menu, arrow, logout, check, alert, chart, calendar, grid, shield o leaf (respaldo). SVG decorativo. |

Tonos: alertas `success|warning|danger`; insignias `neutral|success|warning|danger`. Una variante desconocida usa el estilo base. Los modelos de nutrición y asignación reciben datos ya validados por quien los consume.

```cshtml
<partial name="UI/_Profile"
         model='@(new ProfileUiModel("María Solís", "maria@ejemplo.com", "Paciente"))' />
<partial name="UI/_Diet"
         model='@(new DietUiModel("Equilibrio diario", "Ejemplo visual", 2000, 250, 100, 67))' />
```

### Búsquedas y comportamiento

Para reutilizar la búsqueda, envolver `UI/_Search` y la lista en un elemento `data-search-list`. Marcar el contenedor de resultados con `data-search-results` y cada fila o tarjeta con `data-search-item data-search-text="nombre correo"`.

El filtro es local, ignora mayúsculas y acentos, anuncia el contador mediante role status y muestra un estado vacío. Limpiar restaura la lista y devuelve el foco a la entrada. Los datos permanecen sujetos al filtrado por rol que ya realiza el servidor.

La selección de médicos usa casillas con `name="DoctorIds"` y conserva la selección tras errores. El grupo solo se habilita para el rol Paciente; al cambiar de rol, no se envían asociaciones deshabilitadas.

## Galería en desarrollo

Con `ASPNETCORE_ENVIRONMENT=Development`, abrir [galería local](http://localhost:5294/DesignSystem). La cabecera o pie también contiene un enlace.

Incluye paleta, botones, campos, estados, perfiles con búsqueda, resumen, dieta, comida, progreso y asignación con vista previa local. Los datos están identificados como ficticios. La vista previa valida fechas y nunca guarda cambios.

`GET /DesignSystem` y `GET /DesignSystem/Index` devuelven **404 fuera de Development**, incluso para administradores. La galería permite acceso anónimo en desarrollo para facilitar la revisión.

## Accesibilidad y comprobaciones

Objetivo: [WCAG 2.2 AA](https://www.w3.org/WAI/WCAG22/quickref/). Se usan etiquetas, mensajes asociados, agrupación semántica, foco visible, enlace para saltar al contenido, estados expresados con texto, movimiento reducido y acciones con una altura mínima de 44 px. Los formularios y el menú funcionan con teclado.

### Pruebas automatizadas

```powershell
npm ci
npx playwright install chromium
npm run test:ui
npm run publish
```

Las pruebas necesitan los binarios de PostgreSQL (`initdb` y `pg_ctl`). En Windows se detecta la instalación bajo Program Files/PostgreSQL; en otros sistemas se usa PATH. Para una instalación distinta:

```powershell
$env:POSTGRES_BIN = "C:/Program Files/PostgreSQL/18/bin"
npm run test:ui
```

El ejecutor crea un clúster **temporal**, con puertos libres y conexión solo a 127.0.0.1. Arranca las aplicaciones Development y Production con variables propias; no lee los secretos del usuario ni utiliza la base de datos habitual. Crea cuentas de prueba, detiene PostgreSQL y elimina exclusivamente su directorio temporal al finalizar.

La suite verifica:
- Pantallas públicas y de cada rol con axe (reglas WCAG A/AA).
- Anchos 320, 375, 768, 1024 y 1440 px; ausencia de desbordamiento de página; ampliación de texto.
- Teclado, enlace de salto, apertura del menú, Escape y movimiento reducido.
- Registro, acceso, cierre de sesión, creación por médico/administrador, asociaciones y selección de médicos.
- Validación cliente/servidor, búsqueda con acentos, limpieza, resultados vacíos y nombres/correos largos.
- Vista previa de asignación y rechazo de periodos invertidos.
- Galería inaccesible en producción y CSS local servido.

Los artefactos de navegador quedan en `test-results/` (ignorado por Git), incluidas capturas de escritorio y móvil. Axe complementa la revisión manual; no constituye una certificación de accesibilidad. Revisar también orden de foco, lectura con lector de pantalla, zoom del navegador y desplazamiento de tablas antes de entregar cambios.

## Funcionalidad actual y límites

- Una persona se registra como Médico o Paciente y obtiene acceso de inmediato.
- El administrador consulta usuarios y crea cuentas de cualquier rol; puede asociar varios médicos a un paciente.
- Un médico crea pacientes o asocia cuentas existentes por correo exacto. Su listado distingue los creados por él.
- El paciente consulta sus médicos asociados.
- Las contraseñas iniciales se introducen en el formulario correspondiente.

La creación y asignación persistente de dietas queda para una etapa posterior. Esta entrega prepara sus componentes visuales y adapta las pantallas actuales, sin cambiar roles ni agregar migraciones.

Compatibilidad visual: navegadores modernos de escritorio, Android e iOS compatibles con [Tailwind CSS 4](https://tailwindcss.com/docs/compatibility) (Chrome 111+, Safari 16.4+, Firefox 128+ como base).
