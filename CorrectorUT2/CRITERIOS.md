# Criterios de comprobación

El número seleccionado es el del ejercicio 1.1 a 1.50 del libro. Las pruebas comprueban requisitos observables; no comparan el archivo con una solución modelo. Los criterios se pueden consultar y adaptar en los tres archivos `*ExerciseVerifier.cs` de `Corrector.Core`.

## Variables, tipos y operadores

En las fichas se permiten los ejemplos elegidos por el alumno. Se comprueban los tipos y el uso de las variables en instrucciones de salida que realmente se ejecutan. En los ejercicios de operadores se prueban datos alternativos y se buscan resultados almacenados en variables; imprimir directamente el resultado no basta cuando el enunciado pide guardarlo.

| Ejercicio | Comprobaciones principales |
| --- | --- |
| 1 | Texto, edad entera, altura decimal, inicial y carnet; mostrar las variables. |
| 2 | Nombre, jugadores, precio, letra de clasificación, multijugador y valoración decimal. |
| 3 | Nombre y código como textos, precio, unidades, disponibilidad y sección. |
| 4 | Localidad, temperatura y viento decimales, humedad, lluvia y dirección. |
| 5 | Expediente completo y segunda nota nullable iniciada a null. |
| 6 | Datos del vehículo, propietarios y consumo nullable desconocidos. |
| 7 | Tarjeta como texto, edad, temperatura, fiebre, consulta nullable e inicial. |
| 8 | Conservar las ocho variables del código y corregir sus tipos y valores. |
| 9 | Datos de seguimiento, paquete no entregado y valoración nullable sin asignar. |
| 10 | Al menos diez variables mostradas, los tipos obligatorios y un nullable. |
| 11 | Suma, resta, producto, división entera y resto con distintos operandos. |
| 12 | Reparto entero y monedas sobrantes, incluido reparto exacto. |
| 13 | Actualización correcta de puntos, vidas y monedas. |
| 14 | Subtotales y total con distintos precios y cantidades. |
| 15 | Media decimal y aprobado desde cinco. |
| 16 | Edad, altura, entrada y acompañamiento, incluidos sus límites. |
| 17 | Compras, socio/premium, cupón, productos y promociones. |
| 18 | Edad, autorización, trámites, marca y acceso al grupo avanzado. |
| 19 | Edad, intentos, cuenta, doble factor y administrador. |
| 20 | Estadísticas decimales y clasificación según los seis requisitos booleanos, sin if. |

En estos ejercicios la coherencia de una ficha elegida libremente —por ejemplo, que el texto represente de verdad el nombre de un producto— requiere también la revisión del profesor. El análisis de las operaciones trabaja con variables locales del programa principal; no interpreta cualquier arquitectura de clases o métodos auxiliares.

## Condicionales

Se modifican únicamente inicializadores de los datos identificados. Los mensajes admiten variaciones de mayúsculas, acentos, puntuación y algunas expresiones equivalentes. Las cantidades admiten punto/coma y distintos formatos decimales. Si no se puede identificar un dato, se informa y se permite asociarlo con `--mapa`; no se da la entrega por válida sin probarlo.

| Ejercicio | Comprobaciones principales |
| --- | --- |
| 21 | Promociones independientes y límites de importe/productos. |
| 22 | Compra válida, saldo exacto, saldo insuficiente y actualización del saldo. |
| 23 | Usuario, contraseña y estado de cuenta. |
| 24 | Todos los rangos de temperatura y sus fronteras. |
| 25 | Prioridad del proyecto/asistencia y límites de las calificaciones. |
| 26 | Cada plato con su precio, salida y opción incorrecta. |
| 27 | Cuatro operaciones, división decimal y operador inválido. |
| 28 | Tarifas por edad y elección de la más barata cuando coinciden ventajas. |
| 29 | Matriz de roles, acciones, propiedad y autenticación. |
| 30 | Seguridad del cajero, consulta, retirada, ingreso, cantidades y saldo conservado al rechazar. |

La ampliación de división entre cero del ejercicio 27 no es obligatoria. Los dos PIN del ejercicio 30 y los dos booleanos inicialmente true del 28 pueden necesitar un mapa cuando sus nombres no explican su significado.

## Bucles e integración

Las pruebas alimentan `Console.ReadLine()` y comprueban resultados y resúmenes. Se permiten cambios razonables de etiquetas; para facilitar la corrección conviene conservar palabras descriptivas como «total», «saldo», «media», «ocupadas» o «recaudación» y mostrar cada dato claramente.

| Ejercicio | Comprobaciones principales |
| --- | --- |
| 31 | Un único for, vueltas 1–10, hidratación 3/6/9 y aviso en la vuelta 5. |
| 32 | Doce meses y ahorro acumulado; se acepta total 700 o ampliación de 900. |
| 33 | Pares excluyendo múltiplos de tres, continue, cantidad 17 y suma 434. |
| 34 | While, ventas acumuladas y objetivo alcanzado en siete días. |
| 35 | Contraseña, cinco intentos, intentos restantes y bloqueo. |
| 36 | Pistas mayor/menor, siete intentos, victoria y derrota. |
| 37 | Opciones del menú, repetición y finalización. |
| 38 | Saldo inicial, consulta, retiradas, ingresos y rechazos sin alterar el saldo. |
| 39 | Precios válidos, negativos descartados, cero como fin y aviso de producto caro. |
| 40 | Ataques normal/fuerte, turnos pares, pociones, huida, victoria y derrota. |
| 41 | Rangos de letras, mayúsculas/minúsculas, entradas inválidas y cuatro contadores. |
| 42 | Subtotales, acumulación y todos los tramos de descuento. |
| 43 | Entradas y salidas válidas, límites 0–20, consultas y secuencia del enunciado. |
| 44 | Descuento desde cinco entradas, stock, agotamiento y recaudación. |
| 45 | Tablas completas, multiplicaciones y rechazo de límites menores que uno. |
| 46 | Tres figuras con distintas alturas; se admite el triángulo inicial adicional. |
| 47 | Repetir notas inválidas del mismo alumno, clasificación, media, máximos y mínimos. |
| 48 | Recorrer habitaciones, repetir S/N inválido, ocupación y porcentaje. |
| 49 | Puntos no negativos, totales y medias por jugador, partidas destacadas y ganador. |
| 50 | Carta, unidades, errores, descuentos, varios clientes, reinicio y recaudación. |

El límite de 100 puntos de vida del combate es una ampliación opcional. Las pruebas evitan que esa diferencia determine el aprobado. Intentar un ataque fuerte en turno impar no consume turno; una opción inexistente no ataca. Intentar usar una poción agotada consume turno, según la interpretación documentada de la solución. En el campeonato no se impone una política de desempate que el enunciado no especifica.

Los casos interactivos tienen un tiempo máximo de cinco segundos y un límite de salida de 256 KiB por canal. Un bucle que no termina o una entrega que no compila produce un fallo explicado. Se aceptan programas que utilizan `Console.ReadLine`; no se simulan interfaces gráficas ni pulsaciones individuales de `Console.ReadKey`.

Un resultado positivo verifica estos casos y requisitos, no demuestra matemáticamente que cualquier posible entrada funcione ni juzga el estilo del código.
