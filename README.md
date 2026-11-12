
# FinalPPE — Trophy Base

Aplicación web para la gestión de trofeos de videojuegos, desarrollada con **ASP.NET Core y Vue 3**.

El sistema permite consultar, registrar, editar y eliminar trofeos mediante una API REST. Incorpora autenticación con JWT, persistencia de datos con Entity Framework Core y SQLite, y documentación interactiva de endpoints mediante OpenAPI y Scalar.

## Tecnologías utilizadas

### Backend
- **ASP.NET Core (.NET 10)** — API REST.
- **Entity Framework Core 10** — Acceso y persistencia de datos.
- **SQLite** — Base de datos relacional.
- **JWT Bearer** — Autenticación y autorización.
- **ASP.NET Core Identity PasswordHasher** — Protección de contraseñas.
- **OpenAPI y Scalar** — Documentación interactiva de la API.

### Frontend
- **Vue 3** — Interfaz de usuario.
- **Vue Router** — Navegación entre vistas.
- **Axios** — Comunicación HTTP con el backend.
- **Vite** — Herramienta de desarrollo y compilación.

## Funcionalidades

### Gestión de trofeos

- Consultar el listado de trofeos disponibles.
- Consultar la información de un trofeo por su identificador.
- Registrar nuevos trofeos.
- Editar la información de los trofeos existentes.
- Eliminar trofeos.
- Buscar trofeos por nombre.
- Paginar los resultados.
- Gestionar el estado de obtención de cada trofeo.

### Autenticación y seguridad

- Registro de usuarios mediante correo electrónico y contraseña.
- Almacenamiento seguro de contraseñas mediante hashing.
- Validación de correos electrónicos únicos.
- Inicio de sesión con generación de tokens JWT.
- Protección de operaciones de escritura mediante autorización.
- Persistencia de la sesión en el cliente.
- Cierre de sesión y manejo de tokens inválidos.

### Documentación y pruebas

- Especificación OpenAPI generada automáticamente.
- Interfaz Scalar para explorar y probar los endpoints.
- Pruebas HTTP automatizadas del backend.
- Pruebas de integración del frontend mediante Playwright.

## Estructura del proyecto

```text
FinalPPE/
├── Controllers/          # Controladores de la API
├── Services/             # Lógica de negocio
├── DTOs/                 # Objetos de transferencia de datos
├── Models/               # Entidades del sistema
├── Data/                 # Contexto de Entity Framework
├── Migrations/           # Migraciones de la base de datos
├── frontend/             # Aplicación Vue 3
├── tests/                # Pruebas automatizadas del backend
├── Program.cs            # Configuración principal de la API
├── appsettings.json      # Configuración de la aplicación
└── FinalPPE.csproj       # Proyecto .NET
```

El backend sigue una arquitectura basada en controladores, servicios y acceso a datos, utilizando inyección de dependencias para separar responsabilidades.

## Requisitos previos

Para ejecutar el proyecto es necesario contar con:

- **.NET SDK 10**
- **Node.js 22.18.0 o superior compatible con Vite**
- **npm**
- **OpenSSL** (para generar la clave JWT en Linux)
- **Python 3** (opcional, para pruebas HTTP)

Comprobar las versiones instaladas:

```bash
dotnet --version
node --version
npm --version
```

## Instalación y ejecución

### 1. Clonar el repositorio

```bash
git clone https://github.com/Miguel-213/FinalPPE.git
cd FinalPPE
```

### 2. Instalar las dependencias del backend

Desde la raíz del proyecto:

```bash
dotnet restore
dotnet tool restore
```

### 3. Configurar la autenticación JWT

La aplicación utiliza una clave secreta para firmar y verificar los tokens de autenticación.

En Linux con Bash:

```bash
dotnet user-secrets set "Jwt:Key" "$(openssl rand -hex 32)"
```

En Fish Shell:

```fish
dotnet user-secrets set "Jwt:Key" (openssl rand -hex 32)
```

En Windows con PowerShell:

```powershell
$bytes = New-Object byte[] 32
$generador = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$generador.GetBytes($bytes)
$generador.Dispose()
$clave = [BitConverter]::ToString($bytes).Replace("-", "")
dotnet user-secrets set "Jwt:Key" $clave
```

La clave debe mantenerse privada y no debe incluirse en el repositorio.

### 4. Configurar la base de datos

El proyecto utiliza SQLite y Entity Framework Core.

Aplicar las migraciones existentes:

```bash
dotnet tool run dotnet-ef database update
```

Esto crea o actualiza la base de datos `finalppe.db`.

### 5. Ejecutar el backend

```bash
dotnet run --launch-profile http
```

La API estará disponible en:

**http://localhost:5013**

Servicios adicionales en el entorno de desarrollo:

| Servicio | URL |
|---|---|
| API REST | http://localhost:5013 |
| OpenAPI | http://localhost:5013/openapi/v1.json |
| Scalar | http://localhost:5013/scalar/v1 |

### 6. Instalar y ejecutar el frontend

En otra terminal, desde la raíz del proyecto:

```bash
cp frontend/.env.example frontend/.env
npm ci --prefix frontend
npm run dev --prefix frontend
```

En Windows con PowerShell, utilizar `Copy-Item` en lugar de `cp` si es necesario.

La interfaz estará disponible en:

**http://localhost:5173**

El archivo `frontend/.env` debe contener la dirección de la API:

```dotenv
VITE_API_URL=http://localhost:5013
```

Si se modifica esta variable, es necesario reiniciar Vite.

## Configuración

Las principales opciones de configuración son:

| Variable | Descripción |
|---|---|
| `ConnectionStrings:DefaultConnection` | Cadena de conexión de SQLite. |
| `Jwt:Issuer` | Emisor de los tokens JWT. |
| `Jwt:Audience` | Destinatario de los tokens JWT. |
| `Jwt:Key` | Clave secreta utilizada para firmar tokens. |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución de ASP.NET Core. |
| `VITE_API_URL` | Dirección del backend utilizada por Vue. |

Valores predeterminados:

```text
ConnectionStrings:DefaultConnection = Data Source=finalppe.db
Jwt:Issuer = FinalPPE
Jwt:Audience = FinalPPE
```

En ASP.NET Core, las variables de entorno pueden configurarse utilizando doble guion bajo:

```text
Jwt__Key
Jwt__Issuer
Jwt__Audience
ConnectionStrings__DefaultConnection
```

Los secretos deben configurarse mediante User Secrets durante el desarrollo o mediante variables de entorno en un despliegue.

## API REST

La API utiliza JSON para intercambiar información y mantiene la convención `camelCase` en sus propiedades.

### Autenticación

| Método | Endpoint | Descripción | Acceso |
|---|---|---|---|
| POST | `/auth/register` | Registrar un usuario. | Público |
| POST | `/auth/login` | Iniciar sesión y generar JWT. | Público |

**Ejemplo de registro**

```http
POST /auth/register
Content-Type: application/json
```

```json
{
  "email": "usuario@correo.com",
  "password": "MiContrasenaSegura123"
}
```

**Respuesta exitosa (201 Created):**

```json
{
  "message": "Usuario registrado",
  "id": 1,
  "email": "usuario@correo.com"
}
```

**Ejemplo de inicio de sesión**

```http
POST /auth/login
Content-Type: application/json
```

```json
{
  "email": "usuario@correo.com",
  "password": "MiContrasenaSegura123"
}
```

**Respuesta exitosa (201 Created):**

```json
{
  "access_token": "TOKEN_JWT"
}
```

El token devuelto permite acceder a las operaciones protegidas.

### Gestión de trofeos

| Método | Endpoint | Descripción | Acceso |
|---|---|---|---|
| GET | `/trofeos` | Obtener todos los trofeos. | Público |
| GET | `/trofeos/{id}` | Consultar un trofeo. | Público |
| POST | `/trofeos` | Crear un trofeo. | JWT |
| PATCH | `/trofeos/{id}` | Actualizar un trofeo. | JWT |
| DELETE | `/trofeos/{id}` | Eliminar un trofeo. | JWT |

**Ejemplo de creación de trofeo**

```http
POST /trofeos
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "nombre": "Diamantes",
  "juego": "Minecraft",
  "dificultad": "Media",
  "requisito": "Encontrar diamantes",
  "estado": false,
  "imagen": "https://ejemplo.com/imagen.png"
}
```

El servidor asigna automáticamente un identificador al nuevo registro.

**Ejemplo de actualización**

```http
PATCH /trofeos/1
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "estado": true
}
```

El método PATCH permite actualizar únicamente los campos proporcionados, sin necesidad de enviar el objeto completo.

### Búsqueda y paginación

El endpoint de consulta admite parámetros opcionales.

**Buscar por nombre:**

```http
GET /trofeos?q=minecraft
```

**Consultar una página:**

```http
GET /trofeos?page=2
```

**Combinar búsqueda y paginación:**

```http
GET /trofeos?q=minecraft&page=1&pageSize=4
```

Parámetros disponibles:

| Parámetro | Descripción |
|---|---|
| `q` | Búsqueda por nombre, sin distinguir mayúsculas y minúsculas. |
| `page` | Número de página, comenzando desde 1. |
| `pageSize` | Cantidad de resultados por página. |

El tamaño predeterminado de página es de cuatro registros.

Cuando no se especifica `page`, la API devuelve el listado completo. Los resultados se ordenan por identificador y se entregan como un arreglo JSON.

El frontend también implementa filtrado y paginación local para trabajar con el listado completo.

## Autenticación y autorización

La aplicación implementa autenticación mediante **JSON Web Tokens (JWT)**.

### Funcionamiento

1. El usuario se registra proporcionando su correo electrónico y contraseña.
2. El backend genera un hash de la contraseña mediante `PasswordHasher<Usuario>`.
3. Los datos del usuario se almacenan en SQLite.
4. Al iniciar sesión, se verifican las credenciales.
5. Si son válidas, el servidor genera un token JWT firmado con HMAC SHA-256.
6. El frontend almacena el token y lo utiliza en las solicitudes protegidas.
7. El middleware de ASP.NET Core verifica la firma, el emisor, el destinatario y la expiración del token.

Los tokens tienen una duración de **dos horas**.

### Endpoints protegidos

Las operaciones de creación, edición y eliminación requieren autenticación.

```http
Authorization: Bearer TOKEN_JWT
```

La protección se implementa mediante el atributo `[Authorize]` en los controladores correspondientes.

Las consultas GET permanecen públicas.

### Manejo de sesiones

El frontend almacena el token en `localStorage` bajo la clave `token`.

Axios se encarga de incluirlo en las solicitudes autenticadas.

Al cerrar sesión:
- Se elimina el token almacenado.
- Se actualiza el estado de autenticación.
- Se ocultan las opciones que requieren una sesión activa.

Si el servidor devuelve un error 401 en una operación protegida, la aplicación elimina el estado de sesión y redirige al inicio de sesión.

El cierre de sesión es local y no revoca tokens emitidos anteriormente. El proyecto no implementa refresh tokens ni roles de usuario.

## Base de datos

El sistema utiliza **SQLite** como motor de persistencia y **Entity Framework Core** como ORM.

Las entidades principales son:

### Usuario

Representa a los usuarios registrados en la aplicación.

Contiene la información necesaria para la identificación y autenticación, incluyendo correo electrónico y hash de contraseña.

### Trofeo

Representa los trofeos disponibles en el sistema.

Campos principales:

| Campo | Descripción |
|---|---|
| `id` | Identificador único. |
| `nombre` | Nombre del trofeo. |
| `juego` | Videojuego al que pertenece. |
| `dificultad` | Nivel de dificultad. |
| `requisito` | Condición necesaria para obtenerlo. |
| `estado` | Indica si fue obtenido. |
| `imagen` | URL de la imagen del trofeo. |

### Migraciones

Los cambios en el esquema de la base de datos se administran mediante migraciones de Entity Framework Core.

Crear una migración:

```bash
dotnet tool run dotnet-ef migrations add NombreMigracion
```

Aplicar migraciones:

```bash
dotnet tool run dotnet-ef database update
```

Las migraciones se mantienen versionadas para permitir la reproducción del esquema en diferentes entornos.

## Documentación interactiva con Scalar

El proyecto integra OpenAPI y Scalar para facilitar la consulta y ejecución de solicitudes HTTP.

Con la API ejecutándose en modo desarrollo, acceder a:

**http://localhost:5013/scalar/v1**

Scalar permite:

- Consultar los endpoints disponibles.
- Visualizar los esquemas de solicitudes y respuestas.
- Ejecutar solicitudes desde el navegador.
- Probar el registro e inicio de sesión.
- Configurar autenticación Bearer.
- Ejecutar operaciones protegidas con JWT.
- Inspeccionar códigos de estado y respuestas HTTP.

La documentación OpenAPI está disponible en:

```text
http://localhost:5013/openapi/v1.json
```

Por seguridad, OpenAPI y Scalar solamente están habilitados en el entorno de desarrollo.

## Pruebas

El proyecto incorpora pruebas automatizadas para verificar el funcionamiento del backend y la integración con el frontend.

### Pruebas del backend

Ejecutar desde la raíz:

```bash
python tests/api_smoke.py
```

En sistemas donde Python se ejecuta como `python3`:

```bash
python3 tests/api_smoke.py
```

El conjunto de pruebas realiza 50 solicitudes HTTP y verifica:

- Registro de usuarios.
- Validación de correos duplicados.
- Protección de contraseñas.
- Inicio de sesión.
- Generación y validación de JWT.
- Rechazo de tokens inválidos o expirados.
- Restricciones de acceso a endpoints.
- Operaciones CRUD.
- Actualizaciones parciales.
- Búsqueda y paginación.
- Configuración de CORS.
- Seguridad declarada en OpenAPI.

Las pruebas utilizan una base de datos temporal independiente de la base principal.

### Pruebas del frontend

Con el backend y Vite ejecutándose:

```bash
npm exec --prefix frontend -- playwright install chromium
npm run test:e2e --prefix frontend
```

Las pruebas utilizan Playwright y Chromium para validar:

- Registro e inicio de sesión.
- Persistencia de la sesión.
- Creación y edición de trofeos.
- Búsqueda y paginación.
- Eliminación de registros.
- Cierre de sesión.
- Manejo de autenticación inválida.
- Integración de Scalar con JWT.

Los resultados y capturas generados se almacenan en `frontend/test-results/`.

### Verificación de compilación

Compilar el backend:

```bash
dotnet build
```

Compilar el frontend:

```bash
npm run build --prefix frontend
```

Revisar las dependencias del frontend:

```bash
npm audit --prefix frontend
```

## Consideraciones de seguridad

La aplicación incorpora diferentes mecanismos de seguridad:

- Las contraseñas se almacenan como hashes, nunca en texto plano.
- Los correos electrónicos deben ser únicos.
- Los tokens JWT están firmados y tienen expiración.
- Las operaciones de escritura requieren autenticación.
- Las credenciales y claves secretas no deben incluirse en Git.
- CORS limita los orígenes permitidos en el entorno de desarrollo.
- Los endpoints de documentación interactiva están restringidos al entorno de desarrollo.

Para un despliegue en producción se recomienda utilizar HTTPS, configurar los secretos mediante variables de entorno y aplicar medidas de prevención contra ataques XSS, especialmente por el uso de `localStorage`.

## Arquitectura

La aplicación separa las responsabilidades entre el cliente, la API y la capa de persistencia.

```text
              FRONTEND
               Vue 3
                 |
                 | HTTP / JSON
                 v
              BACKEND
            ASP.NET Core
                 |
                 v
            Controllers
                 |
                 v
              Services
                 |
                 v
           Entity Framework
                 |
                 v
               SQLite
```

Los controladores reciben las solicitudes HTTP y delegan la lógica de negocio a los servicios.

Los servicios utilizan Entity Framework Core para consultar o modificar la información almacenada en SQLite.

El frontend se comunica con la API mediante Axios, manteniendo separadas la presentación y la lógica del servidor.

## Repositorio

Código fuente disponible en:

**https://github.com/Miguel-213/FinalPPE**
