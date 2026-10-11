# Corrector de los 50 ejercicios de UT2

Proyecto para que cada alumno compruebe su propio `Program.cs` en Visual Studio Code. Admite un archivo local o una URL pública HTTPS, incluidos los enlaces normales de GitHub. Las pruebas son visibles y explican los requisitos que debes revisar.

## Empezar en VS Code

1. Instala el **SDK de .NET 8 o posterior** desde [la página de .NET](https://dotnet.microsoft.com/download). El SDK incluye las herramientas de desarrollo; instalar solo el runtime no basta.
2. Descarga esta carpeta completa y ábrela en VS Code con **Archivo → Abrir carpeta → CorrectorUT2**. Acepta la recomendación de instalar C# Dev Kit si quieres explorar las pruebas en el editor.
3. Abre **Terminal → Ejecutar tarea → Comprobar ejercicio**.
4. Escribe el número del ejercicio, del **1 al 50**, y pega la ruta de tu `Program.cs` o su URL.

El corrector prepara una copia de tu código, ejecuta las pruebas y muestra sus resultados. Para volver a comprobar la misma entrega después de editarla, repite **Comprobar ejercicio**: la copia anterior no se actualiza automáticamente.

También puedes hacerlo desde la terminal de esta carpeta:

```bash
dotnet run --project Corrector.Cli
```

La primera ejecución descarga los paquetes públicos de NuGet. Las comprobaciones de archivos locales no necesitan conexión una vez restaurados los paquetes.

## Elegir un archivo o una URL

Ejemplo con un archivo local, usando comillas si su ruta contiene espacios:

```bash
dotnet run --project Corrector.Cli -- --ejercicio 43 --archivo "C:/Mis ejercicios/Aparcamiento/Program.cs"
```

Ejemplo con GitHub:

```bash
dotnet run --project Corrector.Cli -- --ejercicio 43 --url "https://github.com/USUARIO/REPOSITORIO/blob/main/Aparcamiento/Program.cs"
```

El enlace debe devolver código accesible públicamente. También se admiten enlaces Raw. Para repositorios privados, descarga tu archivo y utiliza la opción local. Una página HTML, un archivo vacío o un archivo que supere 1 MB produce un mensaje de error.

Los programas pueden usar instrucciones superiores o un `Main` convencional. La entrega debe ser autocontenida en un único archivo; este proyecto no añade otros archivos o paquetes del programa del alumno.

## Qué significan los resultados

Cada ejercicio tiene comprobaciones de sus requisitos. Una prueba fallida muestra su nombre y qué debe revisarse; los errores de compilación incluyen la línea y la columna. El resumen indica cuántas comprobaciones se han superado.

- **Ejercicios 1–10:** tipos de datos, valores nullable, variables utilizadas al mostrar la ficha y los requisitos específicos del enunciado. Se permiten los datos elegidos por el alumno donde el enunciado los deja libres.
- **Ejercicios 11–20:** cálculos y condiciones con varios juegos de datos. Se examinan los valores de las variables y su uso en la salida; las etiquetas pueden ser diferentes y los booleanos pueden presentarse como «Sí/No».
- **Ejercicios 21–30:** distintas decisiones, límites y prioridades. Se cambian los datos iniciales en copias temporales y se comprueban resultados numéricos, saldos y mensajes.
- **Ejercicios 31–50:** ejecución con distintas entradas, límites, validaciones, contadores y acumuladores. Se prueban varios clientes, intentos fallidos, partidas y opciones de menú.

Los enunciados están en la carpeta `Ejercicios1` del repositorio. Consulta también [CRITERIOS.md](CRITERIOS.md) para conocer las comprobaciones y sus límites. Superar las pruebas significa superar estos casos; la coherencia del contenido de las fichas y la claridad del código siguen formando parte de la revisión del profesor.

## Nombres de variables diferentes

Se admiten nombres distintos. En los ejercicios 11–30 el corrector debe identificar qué variable contiene cada dato inicial para cambiarlo durante las pruebas. Intenta reconocer nombres equivalentes y los tipos/valores del enunciado. En 11–20 puede recurrir al orden de los datos del enunciado cuando no dispone de otra pista.

Si varias variables son indistinguibles, el corrector pide un **mapa de variables**. No hace falta renombrar tu código: indica qué papel corresponde a cada variable. Por ejemplo, para un cajero que utiliza `claveGuardada` y `claveEscrita`:

```bash
dotnet run --project Corrector.Cli -- --ejercicio 30 --archivo "Program.cs" --mapa pinCorrecto=claveGuardada --mapa pinIntroducido=claveEscrita
```

Puedes repetir `--mapa` para los datos que lo necesiten. La parte izquierda es el nombre del dato en el enunciado y la derecha es el nombre de tu variable. En 11–30, declara los datos iniciales y los resultados en el cuerpo principal del programa, con inicializadores; no los escondas en métodos auxiliares. Las asociaciones concretas se muestran en los resultados cuando están disponibles.

## Decimales

La cultura inicial predeterminada es española (`es-ES`); se puede elegir `en-US`:

```bash
dotnet run --project Corrector.Cli -- --ejercicio 47 --archivo "Program.cs" --cultura en-US
```

Las entradas de los casos interactivos usan números enteros cuando resulta suficiente, para evitar diferencias entre coma y punto. Las comprobaciones de salida aceptan ambos separadores decimales. Si tu código establece expresamente una cultura, se respeta.

## Preparar y explorar las pruebas

La tarea **Preparar entrega sin ejecutar pruebas**, o `--solo-preparar`, guarda la copia y su configuración. Después puedes usar **Pruebas de la entrega preparada**, el explorador de pruebas de C# Dev Kit o:

```bash
dotnet test Corrector.Tests --filter Categoria=Entrega --logger "console;verbosity=detailed"
```

`entrega.json` conserva el ejercicio, la ruta de la copia, la cultura y el mapa. `entregas/Program.cs` es la copia que se comprueba. Ambos están ignorados por Git: cada alumno trabaja con su propia entrega.

El corrector ejecuta código con los permisos del usuario. El proceso separado limita el tiempo y la salida, pero no es un entorno de seguridad aislado; utiliza archivos propios o de confianza.

## Para el profesor

Puedes distribuir únicamente la carpeta **CorrectorUT2**, sin las soluciones del resto del repositorio. Las pruebas no leen ni comparan las soluciones de referencia para corregir una entrega.

La organización es:

- `Corrector.Cli`: elección de ejercicio, carga de código y lanzamiento de xUnit.
- `Corrector.Core`: compilación, ejecución y criterios de los 50 ejercicios.
- `Corrector.Tests`: pruebas xUnit de la entrega y del funcionamiento del corrector.
- `.vscode`: tareas listas para VS Code.

Para probar el propio corrector, sin una entrega preparada:

```bash
dotnet test Corrector.Tests --filter "FullyQualifiedName!~SubmissionTests"
```

Para verificar las **50 soluciones de referencia**, conservando ambas carpetas del repositorio, utiliza:

```bash
# Bash (Linux/macOS)
CORRECTOR_REFERENCIAS=1 dotnet test Corrector.Tests
```

```powershell
# PowerShell (Windows)
$env:CORRECTOR_REFERENCIAS = "1"
dotnet test Corrector.Tests
Remove-Item Env:CORRECTOR_REFERENCIAS
```

La CLI siempre comprueba la entrega seleccionada, aunque se haya dejado esa variable de mantenimiento activada. Las versiones de los paquetes están fijadas; para verificar su restauración puedes utilizar `dotnet restore --locked-mode`.

## Validación realizada

Se han ejecutado **116 pruebas xUnit sin fallos ni omisiones**, incluidas las 50 soluciones de referencia (427 comprobaciones de ejercicio). Las pruebas de mantenimiento incluyen soluciones alternativas, resultados escritos a mano, errores de compilación, tiempos máximos y carga de archivos. También se ha comprobado la CLI completa con un archivo local, una URL de GitHub y una entrega incorrecta que se rechaza con explicación.
