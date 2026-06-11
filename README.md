# Sistema de Gestión de Pistas Urbanas - Trujillo Vial

Aplicación web ASP.NET Core MVC para registrar y visualizar reportes del estado de pistas del Centro Histórico de Trujillo delimitado por el anillo de la Av. España, Perú. Esta versión usa como base el diseño del proyecto de Open Code y agrega el mapa interactivo con Leaflet/OpenStreetMap, evidencia fotográfica y registros visibles en mapa.

## Tecnologías

- C# con ASP.NET Core MVC (.NET 8)
- HTML, CSS, JavaScript y Bootstrap 5
- Leaflet + OpenStreetMap para el mapa
- OSRM público como apoyo para validar puntos sobre pistas y dibujar rutas sobre calles
- System.Text.Json para persistencia en archivo
- Docker para despliegue en Render

## Estructuras de datos implementadas

La lógica principal se apoya en estructuras manuales con nodos:

| Estructura | Uso en el sistema |
|---|---|
| ListaSimple<T> | Guarda los tramos registrados |
| ListaDoble<T> | Guarda el historial de cambios por tramo |
| Cola<T> | Conserva el orden de llegada de los reportes registrados |
| Pila<T> | Guarda acciones y permite deshacer el último registro |
| ÁrbolBinarioBusqueda<T> | Inserta, busca, ordena por ID y apoya filtros por condición |

> Nota para exposición: la lógica principal del proyecto trabaja con estructuras propias basadas en nodos. Las vistas recorren nodos y no dependen de colecciones genéricas como estructura principal.

## Funcionalidades principales

- Registrar tramos de pistas desde un mapa centrado en el Centro Histórico de Trujillo delimitado por el anillo de la Av. España.
- Seleccionar punto de inicio y punto final del tramo.
- Dibujar el tramo con color según estado: bueno, regular, malo, en reparación o intransitable.
- Guardar coordenadas y ruta del tramo.
- Subir evidencia fotográfica JPG, JPEG, PNG o WEBP.
- Ver miniatura de evidencia en registros y foto completa en detalle.
- Ver todos los registros en un mapa general.
- Botón **Ver en mapa** para enfocar un tramo específico.
- Vista de detalle con mapa individual, coordenadas, evidencia e historial.
- Filtros por estado de la pista.
- Búsqueda por código del reporte.
- Reportes ordenados por código.
- Búsqueda por estado de la pista.
- Desactivar registros para que ya no aparezcan en las consultas activas.
- Deshacer último registro con pila.
- Persistencia en `App_Data/tramos.json`.

## Cómo ejecutar localmente

Requisitos:

- .NET SDK 8.0
- Visual Studio 2022 o Visual Studio Code

Comandos:

```bash
cd GestionPistasWeb
dotnet restore
dotnet build
dotnet run
```

Luego abre la URL mostrada en consola, normalmente:

```txt
http://localhost:5000
```

También puedes abrir el archivo `GestionPistasWeb.csproj` directamente en Visual Studio 2022.

## Cómo usar el sistema

1. Entra a **Registrar**.
2. En el mapa, haz clic en el punto de inicio del tramo.
3. Haz clic en el punto final del tramo.
4. Completa sector del centro, referencia, tipo de vía, estado, descripción y reportante.
5. Sube una imagen si tienes evidencia.
6. Presiona **Registrar Tramo**.
7. En **Tramos**, revisa el mapa general y usa **Ver en mapa**.
8. En **Detalle**, revisa el mapa individual, foto y coordenadas.

## GitHub

Antes de subir:

```bash
git init
git add .
git commit -m "Proyecto Trujillo Vial con mapa y estructuras"
git branch -M main
git remote add origin https://github.com/tu-usuario/tu-repositorio.git
git push -u origin main
```

El `.gitignore` evita subir `bin`, `obj`, `.vs`, archivos de usuario, datos JSON generados y evidencias subidas.

## Render

El proyecto incluye:

- `Dockerfile`
- `.dockerignore`
- `render.yaml`

Pasos:

1. Sube el proyecto a GitHub.
2. En Render, crea un **New Web Service**.
3. Conecta tu repositorio.
4. Selecciona despliegue con Docker.
5. Render construirá el proyecto usando el `Dockerfile`.

## Limitación importante en Render Free

En Render Free, los archivos guardados dentro del contenedor pueden perderse cuando el servicio se reinicia o se redeploya. Eso incluye:

- `App_Data/tramos.json`
- imágenes subidas a `wwwroot/uploads`

Para persistencia real se necesitaría base de datos o almacenamiento externo. Para proyecto universitario/demo, la versión actual funciona bien localmente y también despliega en Render.

## Proyecto educativo

Sistema preparado para exposición de estructuras de datos y gestión vial básica del Centro Histórico de Trujillo delimitado por el anillo de la Av. España.


## Alcance actual del proyecto

El sistema trabaja como piloto únicamente dentro del Centro Histórico de Trujillo delimitado por el anillo de la Av. España, tomando como referencia el anillo de la Av. España. Por eso no se manejan zonas externas ni registros fuera de esa área.
