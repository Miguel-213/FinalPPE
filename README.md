# FinalPPE — Trophy Base

Proyecto académico de Programación para la Plataforma Empresarial. Reemplaza la API NestJS del Taller 1 por ASP.NET Core y conserva el cliente Vue original, sus rutas HTTP y los nombres JSON.

## Tecnologías y organización

- ASP.NET Core y .NET 10.
- Entity Framework Core 10, SQLite y migraciones.
- JWT Bearer y `PasswordHasher<Usuario>` de ASP.NET Core Identity.
- Vue 3, Vue Router, Axios y Vite.
- OpenAPI con `Microsoft.AspNetCore.OpenApi` e interfaz Scalar.

```text
Controllers/    Peticiones HTTP y códigos de respuesta
Services/       Lógica de negocio y contratos de los servicios
DTOs/           Datos de entrada
Models/         Entidades Trofeo y Usuario
Data/           AppDbContext
Migrations/     Historial del esquema de SQLite
frontend/       Cliente Vue recuperado del Taller 1
tests/          Prueba HTTP del backend
Program.cs      Inyección de dependencias, JWT, CORS y middleware
```

No se necesita ejecutar ni instalar el backend NestJS. Los servicios existentes de registro y generación de JWT se conservan.

## Requisitos

- **SDK .NET 10**, no solamente el runtime ni SDK .NET 9.
- Node.js `^22.18.0` o `>=24.12.0`, y npm.
- OpenSSL para generar una clave en Linux; alternativa PowerShell indicada abajo.
- Python 3 para las pruebas HTTP opcionales.

Comprueba las versiones:

```bash
dotnet --version
node --version
npm --version
```

## 1. Levantar la API

Desde la raíz del repositorio:

```bash
dotnet restore
dotnet tool restore
```

Configura una clave aleatoria local. **No escribas la clave en el código ni en archivos que subas a Git.**

Linux / Bash:

```bash
dotnet user-secrets set "Jwt:Key" "$(openssl rand -hex 32)"
```

Alternativa en PowerShell:

```powershell
$bytes = New-Object byte[] 32
$generador = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$generador.GetBytes($bytes)
$generador.Dispose()
$clave = [BitConverter]::ToString($bytes).Replace("-", "")
dotnet user-secrets set "Jwt:Key" $clave
```

Aplica las migraciones y ejecuta el perfil HTTP de desarrollo:

```bash
dotnet tool run dotnet-ef database update
dotnet run --launch-profile http
```

- API: `http://localhost:5013`.
- SQLite: `finalppe.db`, creado por las migraciones.
- OpenAPI: `http://localhost:5013/openapi/v1.json`.
- Scalar: `http://localhost:5013/scalar/v1`.

El perfil `http` establece `ASPNETCORE_ENVIRONMENT=Development`, carga user-secrets y permite HTTP local. Fuera de desarrollo se conserva la redirección a HTTPS; suministra los secretos mediante configuración del entorno. No se aplican migraciones automáticamente al iniciar.

Para futuros cambios del modelo, desde la raíz:

```bash
dotnet tool run dotnet-ef migrations add NombreMigracion
dotnet tool run dotnet-ef database update
```

Las migraciones se versionan; los archivos de base de datos no.

## 2. Apuntar Vue a ASP.NET Core

En otra terminal, desde la raíz:

```bash
cp frontend/.env.example frontend/.env
npm ci --prefix frontend
npm run dev --prefix frontend
```

En PowerShell puedes reemplazar `cp` por `Copy-Item`.

Abre `http://localhost:5173`. La configuración local contiene:

```dotenv
VITE_API_URL=http://localhost:5013
```

`frontend/src/services/api.js` es la única instancia HTTP: registro, login y CRUD utilizan esta URL. Si cambias el puerto de la API, modifica solamente `VITE_API_URL` y **reinicia Vite**. Para un build de producción, establece la variable antes de compilar:

```bash
npm run build --prefix frontend
```

Vite usa el puerto 5173 con `strictPort`; si está ocupado, debes liberarlo en vez de cambiar silenciosamente a otro puerto. CORS permite exactamente `http://localhost:5173`, los encabezados JSON y Authorization, y los métodos del CRUD. `127.0.0.1` u otro puerto son orígenes diferentes.

## Configuración

| Variable / opción | Uso |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | `Data Source=finalppe.db`, en appsettings.json |
| `Jwt:Issuer` | `FinalPPE`, emisor del token |
| `Jwt:Audience` | `FinalPPE`, API destinataria |
| `Jwt:Key` | Secreto local; mínimo 32 bytes UTF-8, usar una clave aleatoria |
| `ASPNETCORE_ENVIRONMENT` | `Development` para user-secrets, HTTP local y documentación |
| `VITE_API_URL` | URL pública de la API que utiliza Vue; nunca colocar secretos aquí |

En variables del entorno de ASP.NET Core se utiliza doble guion bajo: `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience` y `ConnectionStrings__DefaultConnection`. User-secrets es para desarrollo, no un almacén cifrado de producción. `.env`, SQLite, `node_modules`, `bin`, `obj` y resultados de pruebas están excluidos de Git; `.env.example` y los lockfiles sí se incluyen.

## Contrato HTTP

Todos los nombres JSON usan camelCase. Las escrituras autenticadas reciben `Authorization: Bearer <access_token>`.

| Método y ruta | Acceso | Respuesta exitosa |
| --- | --- | --- |
| `POST /auth/register` | Público | 201: `{ "message": "Usuario registrado", "id": 1, "email": "..." }` |
| `POST /auth/login` | Público | 201: `{ "access_token": "..." }` |
| `GET /trofeos` | Público | 200: array de trofeos |
| `GET /trofeos/{id}` | Público | 200: trofeo |
| `POST /trofeos` | JWT | 201: trofeo creado, encabezado Location |
| `PATCH /trofeos/{id}` | JWT | 200: trofeo actualizado |
| `DELETE /trofeos/{id}` | JWT | 200: trofeo eliminado |

El login devuelve **201**, como NestJS en el Taller 1; se mantiene el nombre `access_token`. Email duplicado devuelve 409. Credenciales incorrectas o escrituras sin token válido devuelven 401. Un ID inexistente devuelve 404 en el backend ASP.NET existente. No se devuelve la contraseña ni su hash.

Registro y login reciben:

```json
{
  "email": "usuario@correo.com",
  "password": "UnaContrasenaParaLaDemo"
}
```

Crear un trofeo recibe:

```json
{
  "nombre": "Diamantes",
  "juego": "Minecraft",
  "dificultad": "Media",
  "requisito": "Encontrar diamantes",
  "estado": false,
  "imagen": "http://localhost:5173/favicon.ico"
}
```

La respuesta agrega `id`. PATCH acepta únicamente los campos que se desean modificar; por ejemplo `{ "estado": true }`. `estado` es booleano, no texto. `imagen` contiene una URL, no un archivo subido.

### Búsqueda y paginación

La API conserva el listado completo como array cuando no se especifica `page`, y permite:

```text
GET /trofeos?q=minecraft
GET /trofeos?page=2
GET /trofeos?q=minecraft&page=1&pageSize=4
```

- `q`: búsqueda por nombre sin distinguir mayúsculas.
- `page`: página comenzando en 1; si falta o no es positiva se devuelve el listado sin paginar.
- `pageSize`: 4 por defecto; si es menor que 1 se utiliza 4.
- Registros ordenados por ID; respuesta siempre como array, sin envelope ni total agregado.

En el ZIP original, NestJS no implementaba parámetros de búsqueda/paginación: **Vue descargaba el array completo y filtraba/paginaba localmente de cuatro en cuatro**. Se mantiene ese comportamiento para no rediseñar el cliente ni romper su contrato. Los parámetros adicionales del servidor se comprueban por HTTP y en Scalar.

## Cómo funciona JWT y la sesión

1. Registro: `AuthController` entrega el DTO a `AuthService`; `PasswordHasher` almacena un hash en SQLite y el índice único evita emails duplicados.
2. Login: el servicio verifica el hash y firma un JWT HMAC SHA-256 con `sub` y `email`, válido durante dos horas.
3. `AddAuthentication().AddJwtBearer()` configura cómo leer y verificar el Bearer recibido: firma, issuer, audience y expiración. Solo se acepta HS256 y no se añade tolerancia de reloj a la caducidad.
4. `UseAuthentication()` identifica al usuario antes de `UseAuthorization()`. `[Authorize]` exige autenticación en POST, PATCH y DELETE; GET, registro y login continúan públicos.
5. Vue guarda `access_token` bajo la clave `token` de `localStorage`; Axios agrega el encabezado Bearer.
6. Cerrar sesión elimina el token y oculta controles de escritura. Un 401 en una operación protegida también borra el estado reactivo y redirige al login; un login incorrecto conserva su mensaje de error.

La autenticación identifica al usuario; la autorización decide si puede acceder al endpoint. Este proyecto no implementa roles ni propiedad de trofeos: cualquier usuario autenticado puede modificar los trofeos. Logout es local, **no revoca** un JWT ya emitido; no hay refresh token. En producción utiliza HTTPS y recuerda que localStorage requiere prevenir XSS.

## Probar con Scalar (bonus)

OpenAPI y Scalar se exponen solamente en desarrollo.

1. Abre `http://localhost:5013/scalar/v1`.
2. Ejecuta `POST /auth/register` y luego `POST /auth/login` con JSON.
3. Copia solamente el valor de `access_token`.
4. En la autenticación **Bearer**, pega el JWT, sin anteponer `Bearer`; la interfaz agrega el encabezado.
5. Ejecuta POST, PATCH o DELETE de trofeos con ese token.
6. Quita el token y repite una escritura: debe responder 401. GET sigue siendo público.

El documento contiene el esquema HTTP Bearer, marca solo las escrituras con requisitos de seguridad y documenta sus respuestas 401. La configuración de OpenAPI describe la seguridad; quien la aplica realmente es JWT Bearer y `[Authorize]`.

## Pruebas reproducibles

### Backend

Desde la raíz, con Python 3 y SDK .NET 10:

```bash
python tests/api_smoke.py
```

Usa `python3` si así se llama en tu sistema. Puedes indicar otro ejecutable: `python tests/api_smoke.py --dotnet /ruta/a/dotnet`.

La prueba compila, restaura EF, aplica migraciones en una **base temporal**, inicia la API en el puerto 5014 y verifica 50 peticiones HTTP: rutas públicas, registro duplicado, hash, login, JWT vencido/alterado/issuer o audience incorrectos, 401, CRUD, PATCH parcial y booleano false, búsqueda, paginación, CORS y seguridad en OpenAPI. Al terminar cierra su propio proceso y elimina su base temporal; no usa `finalppe.db`.

### Vue en Chromium

Con API y Vite ejecutándose según los pasos anteriores:

```bash
npm exec --prefix frontend -- playwright install chromium
npm run test:e2e --prefix frontend
```

En Linux pueden ser necesarias las dependencias del navegador: `playwright install --with-deps chromium`, siguiendo las instrucciones de Playwright para tu distribución.

Las dos pruebas usan Chromium real. La de Vue comprueba registro, duplicado, login incorrecto/correcto, persistencia al recargar, cinco creaciones, paginación 4 + 1, búsqueda por nombre y sin resultados, edición, cancelación/confirmación de eliminación, logout y token inválido. Verifica Bearer y errores de JavaScript y genera una captura en `frontend/test-results/`. La de Scalar introduce un JWT en su interfaz, crea un trofeo con 201 y repite sin token para comprobar 401. Ambas eliminan únicamente sus propios trofeos; los usuarios de prueba permanecen. Si la API usa otro puerto, proporciona también `VITE_API_URL` al comando de prueba.

Comprobaciones de compilación y dependencias:

```bash
dotnet build
npm run build --prefix frontend
npm audit --prefix frontend
git diff --check
```

Detén la API antes de recompilar en Windows si su DLL está bloqueada. Las pruebas se ejecutaron con SDK .NET 10, Chromium y una base de datos de demostración aislada.

## Verificación de la rúbrica y entrega

- [x] API ASP.NET Core con controllers, servicios inyectados y DTOs.
- [x] Mismo modelo conceptual, EF Core, SQLite y migraciones versionadas.
- [x] Rutas, verbos y JSON del CRUD y autenticación conservados.
- [x] Listado compatible, búsqueda/paginación y CORS para Vue.
- [x] Registro con hash, login con JWT firmado y escrituras con Authorize.
- [x] Cliente Vue original conectado, CRUD y flujo de sesión probados.
- [x] README con instalación, variables y ejecución.
- [x] Bonus: Microsoft.AspNetCore.OpenApi, Scalar y pruebas con Bearer.
- [ ] Guardar los cambios en Git y subirlos al repositorio público.
- [ ] Realizar la demostración en clase y la sustentación individual.

### Commits sugeridos

Haz cada commit después de verificar su bloque; estos comandos no se ejecutaron automáticamente:

```bash
git add Program.cs Controllers/TrofeosController.cs tests/api_smoke.py
git commit -m "Proteger escritura de trofeos con JWT"

git add .gitignore FinalPPE.csproj frontend
git commit -m "Conectar Vue con la API de ASP.NET"

git add Program.cs FinalPPE.csproj Controllers/AuthController.cs Controllers/TrofeosController.cs tests/api_smoke.py README.md
git commit -m "Agregar OpenAPI y documentación del proyecto"
```

Si todos los bloques ya se encuentran juntos sin commits intermedios, es más claro guardar un solo commit:

```bash
git add Program.cs Controllers/AuthController.cs Controllers/TrofeosController.cs FinalPPE.csproj .gitignore frontend tests/api_smoke.py README.md
git diff --cached --stat
git commit -m "Completar autenticación e integración con Vue"
```

Revisa lo que estás incluyendo antes de guardar; no subas claves, tokens ni archivos locales. La publicación en GitHub y la demostración requieren una acción posterior del estudiante.

Para la sustentación, muestra un GET sin sesión, una escritura con 401, el login y la misma escritura con Bearer; explica después la secuencia controlador → servicio → DbContext y la diferencia entre generar un token y validarlo.
