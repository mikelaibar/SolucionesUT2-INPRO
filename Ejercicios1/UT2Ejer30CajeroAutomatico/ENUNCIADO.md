# Ejercicio 1.30

30. Cajero automático
En este último ejercicio tendrás que diseñar la lógica completa del programa. No se indica qué estructura condicional debes utilizar en cada momento.
Un cajero automático dispone de la siguiente información:
bool tarjetaActiva = true;
int pinCorrecto = 1234;
int pinIntroducido = 1234;
int intentosFallidos = 1;
double saldo = 850.00;
double cantidad = 200.00;
int operacion = 2;
Las operaciones disponibles son:
1 - Consultar saldo
2 - Retirar dinero
3 - Ingresar dinero
4 - Salir
El programa debe aplicar las siguientes reglas:
- Si la tarjeta no está activa, muestra Tarjeta bloqueada y no debe realizar ninguna operación.
- Si existen 3 o más intentos fallidos, muestra Demasiados intentos. Tarjeta bloqueada.
- Si el PIN introducido es incorrecto, muestra PIN incorrecto.
- Solo si las comprobaciones anteriores son correctas se podrá procesar la operación seleccionada.
Para las operaciones:
- Consultar saldo: muestra el saldo actual.
- Retirar dinero: comprueba primero que cantidad sea mayor que 0 y que exista saldo suficiente. Si todo es correcto, resta la cantidad y muestra el nuevo saldo.
- Ingresar dinero: comprueba que la cantidad sea mayor que 0, suma el importe y muestra el nuevo saldo.
- Salir: muestra Gracias por utilizar el cajero.
- Cualquier otra opción deberá mostrar Operación no válida.
Antes de programar: piensa qué comprobaciones tienen prioridad, cuáles son excluyentes y en qué momento resulta más adecuado utilizar if, if-else, if-else if-else o switch.
Prueba como mínimo estos casos:
- Una consulta de saldo correcta.
- Una retirada correcta.
- Una retirada superior al saldo disponible.
- Un PIN incorrecto.
- Una tarjeta bloqueada.
- Una opción de menú inexistente.
Nombre del proyecto: UT2Ejer30CajeroAutomatico
