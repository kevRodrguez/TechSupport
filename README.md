# TechSupport

API de incidentes y sitio MVC en la misma solución. El sitio no usa Entity Framework: cada pantalla llama a la API con `HttpClient` y el JWT de la sesión.

## Cómo ejecutarlo

Hacen falta dos terminales, en este orden:

```bash
dotnet run --project TechSupport.API
dotnet run --project TechSupport.Web
```

- API: http://localhost:5080
- Sitio: http://localhost:5081

Al arrancar, la API crea `techsupport.db` con `EnsureCreated` si todavía no existe. Ese archivo y la carpeta `Logs/` no se suben a Git.

## Usuarios de prueba

| Usuario | Contraseña | Rol |
| --- | --- | --- |
| admin | Admin123! | Administrador |
| tecnico | Tecnico123! | Tecnico |
| usuario | Usuario123! | Usuario |

Las contraseñas se guardan con hash BCrypt.

## Dónde va la clave JWT

En esta entrega la clave, el emisor (`TechSupport.API`), la audiencia (`TechSupport.Web`) y la expiración (60 minutos) están en `TechSupport.API/appsettings.Development.json`, como permite la guía para la clase.

En un proyecto real esa clave no va en el repositorio. En desarrollo se usa `dotnet user-secrets`. En producción se usa una variable de entorno. Si el archivo se filtra, cualquiera puede firmar tokens.

## Decisiones

Consultar un incidente que no corresponde responde **404** con `Incidente no encontrado.`, el mismo cuerpo que un id que no existe. Un 403 diría que el id sí existe. El listado no usa 404: responde 200 solo con lo que el rol puede ver. Un técnico que elimina, o un usuario que asigna, recibe **403** por `[Authorize]`.

El administrador **sí puede eliminar un incidente Cerrado**. La matriz de la guía no limita el borrado por estado. Al hacerlo se pierden el título, la descripción, el técnico, las fechas y la traza de cómo se resolvió.

El detalle está en [docs/analisis.md](docs/analisis.md). Las nueve pruebas de la guía están en [TechSupport.API/TechSupport.http](TechSupport.API/TechSupport.http).
