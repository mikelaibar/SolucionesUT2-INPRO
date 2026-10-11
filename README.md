# Soluciones UT2 · Introducción a la programación en C#

Soluciones sencillas del libro **Ejercicios 1** de la copia de Moodle facilitada: un ejercicio de introducción y 50 ejercicios numerados, pensados para alumnado de 2.º de SMR.

Cada carpeta de `Ejercicios1/` contiene:

- `Program.cs`: una solución independiente, con variables de nombres descriptivos y comentarios breves.
- `ENUNCIADO.md`: el enunciado del ejercicio, para poder consultarlo junto al código.

Los ejemplos de personas son ficticios. Solo se ha extraído el contenido del libro de actividades.

## Cómo ejecutarlos

Los archivos usan instrucciones de nivel superior de C# (C# 9 o posterior). No se deben reunir todos los `Program.cs` en un único proyecto: cada ejercicio es un programa distinto.

Con el SDK de **.NET 10**, puedes ejecutar directamente un archivo, desde la raíz del repositorio:

```bash
dotnet run --file Ejercicios1/UT2Ejer43Aparcamiento/Program.cs
```

También puedes crear un proyecto de consola con .NET 8 o posterior en otra carpeta, sustituir su `Program.cs` por el del ejercicio y ejecutar `dotnet run`:

```bash
dotnet new console --output /tmp/MiEjercicioUT2
cp Ejercicios1/UT2Ejer43Aparcamiento/Program.cs /tmp/MiEjercicioUT2/Program.cs
dotnet run --project /tmp/MiEjercicioUT2
```

En el entorno de nube preparado, el SDK está en `/workspace/.tools/dotnet`. Actívalo para la sesión:

```bash
export DOTNET_ROOT=/workspace/.tools/dotnet
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_HOME=/workspace/.dotnet-home
export NUGET_PACKAGES=/workspace/.nuget/packages
export XDG_DATA_HOME=/workspace/.local/share
export XDG_CACHE_HOME=/workspace/.cache
```

Los ejercicios interactivos con números decimales usan **punto decimal** (por ejemplo `7.5`). Las validaciones solicitadas se mantienen sencillas; cuando no se exige comprobar el formato de entrada, se presupone que el usuario escribe un número.

## Criterios de las soluciones

- Se respeta la progresión del temario: variables y tipos en 1–10, operadores sin condicionales en 11–20, decisiones en 21–30, bucles en 31–40 e integración en 41–50.
- Se incluye una solución por ejercicio. En los ejercicios abiertos se ha elegido una alternativa fácil de entender.
- Los retos se incorporan cuando son sencillos: paga extra cada seis meses en el ejercicio 32 y límite de 100 puntos de vida en el ejercicio 40.
- En el combate, una opción inexistente o un ataque fuerte en turno impar no consume turno. Intentar usar una poción agotada sí consume turno. Huir no cuenta como turno jugado.
- En el generador de figuras se dibuja primero el triángulo creciente y después se ofrece la elección de figura, siguiendo el orden del enunciado.
- En el campeonato, si hay empate de puntos gana el primer jugador introducido.

## Índice

| Ejercicio | Carpeta |
| --- | --- |
| Ejercicio introducción | [HolaMundo](Ejercicios1/HolaMundo/Program.cs) |
| Ejercicio 1.1 | [UT2Ejer1FichaPersonal](Ejercicios1/UT2Ejer1FichaPersonal/Program.cs) |
| Ejercicio 1.2 | [UT2Ejer2Videojuego](Ejercicios1/UT2Ejer2Videojuego/Program.cs) |
| Ejercicio 1.3 | [UT2Ejer3Producto](Ejercicios1/UT2Ejer3Producto/Program.cs) |
| Ejercicio 1.4 | [UT2Ejer4EstacionMeteorologica](Ejercicios1/UT2Ejer4EstacionMeteorologica/Program.cs) |
| Ejercicio 1.5 | [UT2Ejer5Expediente](Ejercicios1/UT2Ejer5Expediente/Program.cs) |
| Ejercicio 1.6 | [UT2Ejer6Vehiculo](Ejercicios1/UT2Ejer6Vehiculo/Program.cs) |
| Ejercicio 1.7 | [UT2Ejer7CitaMedica](Ejercicios1/UT2Ejer7CitaMedica/Program.cs) |
| Ejercicio 1.8 | [UT2Ejer8RescataPrograma](Ejercicios1/UT2Ejer8RescataPrograma/Program.cs) |
| Ejercicio 1.9 | [UT2Ejer9SeguimientoPaquete](Ejercicios1/UT2Ejer9SeguimientoPaquete/Program.cs) |
| Ejercicio 1.10 | [UT2Ejer10MiSistema](Ejercicios1/UT2Ejer10MiSistema/Program.cs) |
| Ejercicio 1.11 | [UT2Ejer11Calculadora](Ejercicios1/UT2Ejer11Calculadora/Program.cs) |
| Ejercicio 1.12 | [UT2Ejer12RepartoMonedas](Ejercicios1/UT2Ejer12RepartoMonedas/Program.cs) |
| Ejercicio 1.13 | [UT2Ejer13Partida](Ejercicios1/UT2Ejer13Partida/Program.cs) |
| Ejercicio 1.14 | [UT2Ejer14TicketCompra](Ejercicios1/UT2Ejer14TicketCompra/Program.cs) |
| Ejercicio 1.15 | [UT2Ejer15CalculadoraNotas](Ejercicios1/UT2Ejer15CalculadoraNotas/Program.cs) |
| Ejercicio 1.16 | [UT2Ejer16Atraccion](Ejercicios1/UT2Ejer16Atraccion/Program.cs) |
| Ejercicio 1.17 | [UT2Ejer17TiendaOnline](Ejercicios1/UT2Ejer17TiendaOnline/Program.cs) |
| Ejercicio 1.18 | [UT2Ejer18InscripcionCarrera](Ejercicios1/UT2Ejer18InscripcionCarrera/Program.cs) |
| Ejercicio 1.19 | [UT2Ejer19SistemaAcceso](Ejercicios1/UT2Ejer19SistemaAcceso/Program.cs) |
| Ejercicio 1.20 | [UT2Ejer20Campeonato](Ejercicios1/UT2Ejer20Campeonato/Program.cs) |
| Ejercicio 1.21 | [UT2Ejer21Promociones](Ejercicios1/UT2Ejer21Promociones/Program.cs) |
| Ejercicio 1.22 | [UT2Ejer22CompraVideojuego](Ejercicios1/UT2Ejer22CompraVideojuego/Program.cs) |
| Ejercicio 1.23 | [UT2Ejer23ControlAcceso](Ejercicios1/UT2Ejer23ControlAcceso/Program.cs) |
| Ejercicio 1.24 | [UT2Ejer24Temperatura](Ejercicios1/UT2Ejer24Temperatura/Program.cs) |
| Ejercicio 1.25 | [UT2Ejer25EvaluacionAlumno](Ejercicios1/UT2Ejer25EvaluacionAlumno/Program.cs) |
| Ejercicio 1.26 | [UT2Ejer26MenuRestaurante](Ejercicios1/UT2Ejer26MenuRestaurante/Program.cs) |
| Ejercicio 1.27 | [UT2Ejer27CalculadoraSwitch](Ejercicios1/UT2Ejer27CalculadoraSwitch/Program.cs) |
| Ejercicio 1.28 | [UT2Ejer28PrecioEntrada](Ejercicios1/UT2Ejer28PrecioEntrada/Program.cs) |
| Ejercicio 1.29 | [UT2Ejer29Permisos](Ejercicios1/UT2Ejer29Permisos/Program.cs) |
| Ejercicio 1.30 | [UT2Ejer30CajeroAutomatico](Ejercicios1/UT2Ejer30CajeroAutomatico/Program.cs) |
| Ejercicio 1.31 | [UT2Ejer31Entrenamiento](Ejercicios1/UT2Ejer31Entrenamiento/Program.cs) |
| Ejercicio 1.32 | [UT2Ejer32PlanAhorro](Ejercicios1/UT2Ejer32PlanAhorro/Program.cs) |
| Ejercicio 1.33 | [UT2Ejer33FiltroNumeros](Ejercicios1/UT2Ejer33FiltroNumeros/Program.cs) |
| Ejercicio 1.34 | [UT2Ejer34ObjetivoVentas](Ejercicios1/UT2Ejer34ObjetivoVentas/Program.cs) |
| Ejercicio 1.35 | [UT2Ejer35AccesoPassword](Ejercicios1/UT2Ejer35AccesoPassword/Program.cs) |
| Ejercicio 1.36 | [UT2Ejer36AdivinaNumero](Ejercicios1/UT2Ejer36AdivinaNumero/Program.cs) |
| Ejercicio 1.37 | [UT2Ejer37MenuAplicacion](Ejercicios1/UT2Ejer37MenuAplicacion/Program.cs) |
| Ejercicio 1.38 | [UT2Ejer38Cajero](Ejercicios1/UT2Ejer38Cajero/Program.cs) |
| Ejercicio 1.39 | [UT2Ejer39CajaRegistradora](Ejercicios1/UT2Ejer39CajaRegistradora/Program.cs) |
| Ejercicio 1.40 | [UT2Ejer40TorneoCombate](Ejercicios1/UT2Ejer40TorneoCombate/Program.cs) |
| Ejercicio 1.41 | [UT2Ejer41AsignacionAulas](Ejercicios1/UT2Ejer41AsignacionAulas/Program.cs) |
| Ejercicio 1.42 | [UT2Ejer42FacturaTienda](Ejercicios1/UT2Ejer42FacturaTienda/Program.cs) |
| Ejercicio 1.43 | [UT2Ejer43Aparcamiento](Ejercicios1/UT2Ejer43Aparcamiento/Program.cs) |
| Ejercicio 1.44 | [UT2Ejer44VentaEntradas](Ejercicios1/UT2Ejer44VentaEntradas/Program.cs) |
| Ejercicio 1.45 | [UT2Ejer45TablasMultiplicar](Ejercicios1/UT2Ejer45TablasMultiplicar/Program.cs) |
| Ejercicio 1.46 | [UT2Ejer46GeneradorFiguras](Ejercicios1/UT2Ejer46GeneradorFiguras/Program.cs) |
| Ejercicio 1.47 | [UT2Ejer47GestionNotas](Ejercicios1/UT2Ejer47GestionNotas/Program.cs) |
| Ejercicio 1.48 | [UT2Ejer48OcupacionHotel](Ejercicios1/UT2Ejer48OcupacionHotel/Program.cs) |
| Ejercicio 1.49 | [UT2Ejer49Campeonato](Ejercicios1/UT2Ejer49Campeonato/Program.cs) |
| Ejercicio 1.50 | [UT2Ejer50Restaurante](Ejercicios1/UT2Ejer50Restaurante/Program.cs) |
