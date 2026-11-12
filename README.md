FinalPPE — Trophy Base

Proyecto académico de Programación para la Plataforma Empresarial. Consiste en la migración de la API del Taller 1, desarrollada originalmente en NestJS, a ASP.NET Core, manteniendo el frontend en Vue 3 y las rutas HTTP existentes.

Tecnologías utilizadas
ASP.NET Core y .NET 10.
Entity Framework Core 10.
SQLite.
JWT Bearer para autenticación.
PasswordHasher de ASP.NET Core Identity.
Vue 3, Vue Router, Axios y Vite.
OpenAPI y Scalar.
Estructura del proyecto
text
Controllers/    Controladores de la API
Services/       Lógica de negocio
DTOs/           Objetos de transferencia de datos
Models/         Modelos de Usuario y Trofeo
Data/           Configuración de Entity Framework
Migrations/     Migraciones de la base de datos
frontend/       Aplicación Vue
tests/          Pruebas del backend
Program.cs      Configuración principal de la aplicación

Requisitos
SDK de .NET 10.
Node.js ^22.18.0 o >=24.12.0.
npm.
OpenSSL para generar la clave JWT en Linux.
Python 3 para ejecutar las pruebas del backend.

Para comprobar las versiones instaladas:

bash
dotnet --version
node --version
npm --version

1. Ejecutar el backend

Desde la carpeta principal del proyecto, ejecutar:

bash
dotnet restore
dotnet tool restore


Configurar la clave JWT utilizando user-secrets.

En Linux:

bash
dotnet user-secrets set "Jwt:Key" "$(openssl rand -hex 32)"


En PowerShell:

powershell
$bytes = New-Object byte[] 32
$generador = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$generador.GetBytes($bytes)
$generador.Dispose()
$clave = [BitConverter]::ToString($bytes).Replace("-", "")
dotnet user-secrets set "Jwt:Key" $clave


Ejecutar las migraciones y levantar la API:

bash
dotnet tool run dotnet-ef database update
dotnet run --launch-profile http


La API queda disponible en:

API: http://localhost:5013
OpenAPI: http://localhost:5013/openapi/v1.json
Scalar: http://localhost:5013/scalar/v1

La base de datos SQLite se crea con el nombre finalppe.db.

Para crear nuevas migraciones:

bash
dotnet tool run dotnet-ef migrations add NombreMigracion
dotnet tool run dotnet-ef database update

2. Ejecutar el frontend

Abrir otra terminal y ejecutar desde la carpeta principal:

bash
cp frontend/.env.example frontend/.env
npm ci --prefix frontend
npm run dev --prefix frontend


En PowerShell:

powershell
Copy-Item frontend/.env.example frontend/.env


La configuración del archivo .env es:

dotenv
VITE_API_URL=http://localhost:5013


El frontend estará disponible en:

http://localhost:5173

La URL de la API se configura desde frontend/src/services/api.js mediante la variable VITE_API_URL.

Para generar la versión de producción:

bash
npm run build --prefix frontend

3. Configuración

Las principales opciones de configuración son:

Variable	Descripción
ConnectionStrings:DefaultConnection	Conexión a la base de datos SQLite
Jwt:Issuer	Emisor del token
Jwt:Audience	Destinatario del token
Jwt:Key	Clave utilizada para firmar los JWT
ASPNETCORE_ENVIRONMENT	Entorno de ejecución
VITE_API_URL	Dirección de la API para Vue

La conexión SQLite utiliza por defecto:

text
Data Source=finalppe.db


Las variables de entorno de ASP.NET Core utilizan doble guion bajo. Por ejemplo:

text
Jwt__Key
Jwt__Issuer
Jwt__Audience
ConnectionStrings__DefaultConnection


Los archivos .env, las bases de datos locales y las carpetas de compilación están excluidos del repositorio.

4. Rutas de la API

La API mantiene las rutas y los nombres JSON utilizados en el proyecto original.

Método	Ruta	Acceso
POST	/auth/register	Público
POST	/auth/login	Público
GET	/trofeos	Público
GET	/trofeos/{id}	Público
POST	/trofeos	JWT
PATCH	/trofeos/{id}	JWT
DELETE	/trofeos/{id}	JWT
Registro

Ruta:

http
POST /auth/register


JSON:

json
{
  "email": "usuario@correo.com",
  "password": "UnaContrasenaParaLaDemo"
}


Respuesta exitosa:

json
{
  "message": "Usuario registrado",
  "id": 1,
  "email": "usuario@correo.com"
}


El registro devuelve código 201. Si el correo ya existe, devuelve 409.

Login

Ruta:

http
POST /auth/login


JSON:

json
{
  "email": "usuario@correo.com",
  "password": "UnaContrasenaParaLaDemo"
}


Respuesta:

json
{
  "access_token": "..."
}


El login devuelve código 201 y genera un JWT válido durante dos horas.

Consultar trofeos

Obtener todos los trofeos:

http
GET /trofeos


Consultar un trofeo por ID:

http
GET /trofeos/1

Crear trofeo

Ruta:

http
POST /trofeos


Requiere autenticación JWT.

json
{
  "nombre": "Diamantes",
  "juego": "Minecraft",
  "dificultad": "Media",
  "requisito": "Encontrar diamantes",
  "estado": false,
  "imagen": "http://localhost:5173/favicon.ico"
}

Actualizar trofeo

Ruta:

http
PATCH /trofeos/1


Permite modificar únicamente los campos necesarios.

json
{
  "estado": true
}

Eliminar trofeo

Ruta:

http
DELETE /trofeos/1


También requiere autenticación JWT.

Las respuestas utilizan los siguientes códigos:

200: operación realizada correctamente.
201: registro o creación realizada.
401: falta de autenticación o credenciales incorrectas.
404: registro no encontrado.
409: correo electrónico duplicado.
5. Búsqueda y paginación

La API permite consultar los trofeos utilizando los parámetros q, page y pageSize.

Ejemplos:

http
GET /trofeos?q=minecraft
GET /trofeos?page=2
GET /trofeos?q=minecraft&page=1&pageSize=4

q: busca por nombre del trofeo sin distinguir mayúsculas y minúsculas.
page: indica el número de página.
pageSize: cantidad de registros por página. Su valor predeterminado es 4.

Los resultados se ordenan por ID y se devuelven como un array.

El frontend original realiza la búsqueda y paginación localmente, mostrando cuatro trofeos por página. Se mantiene este funcionamiento para conservar la estructura original de Vue.

6. Autenticación JWT

La autenticación funciona de la siguiente manera:

El usuario se registra mediante /auth/register.
La contraseña se almacena utilizando PasswordHasher.
Al iniciar sesión, se comprueban las credenciales.
Si son correctas, se genera un JWT firmado con HMAC SHA-256.
El token contiene el identificador y el correo del usuario.
Vue guarda el token en localStorage.
Axios envía el token mediante el encabezado Authorization: Bearer.
ASP.NET Core valida el token antes de permitir las operaciones protegidas.

Las operaciones POST, PATCH y DELETE requieren autenticación. Las consultas GET y las rutas de autenticación son públicas.

La duración del token es de dos horas. El proyecto no utiliza roles ni refresh tokens, y el cierre de sesión elimina el token almacenado en el frontend.

7. Documentación con Scalar

La documentación de la API está disponible en:

http://localhost:5013/scalar/v1

Desde Scalar se pueden ejecutar las diferentes rutas:

Registrar un usuario.
Iniciar sesión.
Copiar el valor de access_token.
Configurar la autenticación Bearer.
Ejecutar las operaciones del CRUD.

Las operaciones de escritura requieren un token válido. Sin autenticación, la API devuelve 401.

OpenAPI y Scalar están habilitados en el entorno de desarrollo.

8. Pruebas del backend

Las pruebas HTTP se ejecutan con Python 3:

bash
python tests/api_smoke.py


En algunos sistemas se utiliza:

bash
python3 tests/api_smoke.py


El archivo de pruebas verifica:

Registro de usuarios.
Correos duplicados.
Inicio de sesión.
Validación de JWT.
Operaciones CRUD.
Actualizaciones parciales.
Búsqueda y paginación.
Configuración CORS.
Documentación OpenAPI.

Las pruebas utilizan una base de datos temporal y ejecutan la API en el puerto 5014.

9. Pruebas del frontend

Con el backend y Vue en funcionamiento, instalar Chromium para Playwright:

bash
npm exec --prefix frontend -- playwright install chromium


Ejecutar las pruebas:

bash
npm run test:e2e --prefix frontend


Las pruebas verifican el registro, inicio de sesión, persistencia de sesión, creación de trofeos, búsqueda, paginación, edición, eliminación y cierre de sesión.

También se comprueba el funcionamiento de la autenticación y las respuestas de la API.

10. Compilación

Para comprobar que los proyectos compilan correctamente:

Backend:

bash
dotnet build


Frontend:

bash
npm run build --prefix frontend


Revisión de dependencias:

bash
npm audit --prefix frontend

11. Estado del proyecto
Migración de NestJS a ASP.NET Core.
Configuración de Entity Framework Core y SQLite.
Migraciones de la base de datos.
Registro e inicio de sesión.
Autenticación mediante JWT.
Protección de las rutas de escritura.
Integración con el frontend Vue.
Operaciones CRUD de trofeos.
Búsqueda y paginación.
Configuración de CORS.
Documentación con OpenAPI y Scalar.
Pruebas del backend y frontend.
