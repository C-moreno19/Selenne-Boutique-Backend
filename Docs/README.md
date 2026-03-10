# Selenne Boutique API

## Requisitos
- .NET 8 SDK
- SQL Server LocalDB

## Configuracion rapida

### Paso 1: Crear la Base de Datos
1. Abre SQL Server Management Studio o Azure Data Studio
2. Conecta a: (localdb)\mssqllocaldb
3. Abre el archivo DATABASE.sql (en la raiz del proyecto)
4. Ejecuta todo el script
5. Verifica la BD "SelenneDb" con todas las tablas

### Paso 2: Ejecutar la API
```bash
cd SelenneApi
dotnet restore
dotnet run
```

### Paso 3: Swagger
Abre: http://localhost:5000/swagger

## Usuario Admin por defecto (creado automaticamente por SeedData)
- Email: admin@selenne.com
- Contrasena: Admin1234!

## Estructura de respuesta
```json
{
  "success": true,
  "message": "Operacion exitosa",
  "data": { ... },
  "errors": null
}
```
