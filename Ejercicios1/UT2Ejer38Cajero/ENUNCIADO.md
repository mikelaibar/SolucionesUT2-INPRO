# Ejercicio 1.38

38. Cajero automático
Amplía el concepto anterior creando un menú que modifica el estado del programa mientras se está ejecutando. Para ello combinarás do-while, switch y estructuras condicionales.
El usuario comienza con:
double saldo = 1000;
Crea mediante do-while el siguiente cajero:
--- CAJERO AUTOMÁTICO ---
1. Consultar saldo
2. Retirar dinero
3. Ingresar dinero
4. Salir
Selecciona una opción:
La opción deberá solicitarse al usuario mediante Console.ReadLine(). Utiliza un switch para procesar la opción seleccionada.
El cajero debe aplicar las siguientes reglas:
- Opción 1 - Consultar saldo: muestra el saldo disponible actualmente.
- Opción 2 - Retirar dinero: solicita al usuario la cantidad que desea retirar. La cantidad debe ser mayor que 0 y no puede superar el saldo disponible. Si la operación es válida, resta la cantidad al saldo y muestra el nuevo saldo.
- Opción 3 - Ingresar dinero: solicita al usuario la cantidad que desea ingresar. La cantidad debe ser mayor que 0. Si es correcta, suma el importe al saldo y muestra el nuevo saldo.
- Opción 4 - Salir: finaliza la ejecución del cajero.
- Cualquier otra opción: muestra Opción no
válida y vuelve a mostrar el menú.
Después de consultar, retirar o ingresar dinero, el programa no debe finalizar. El menú debe volver a mostrarse para permitir realizar otra operación.
El programa terminará únicamente cuando el usuario seleccione la opción 4. Antes de finalizar deberá mostrar:
Saldo final: ... €
Gracias por utilizar el cajero.
Piensa en la estructura: el do-while controla cuánto tiempo permanece funcionando el cajero, mientras que el switch decide qué operación se realiza en cada repetición.
Antes de dar el programa por terminado, comprueba como mínimo los siguientes casos:
- Consultar el saldo inicial.
- Realizar una retirada correcta.
- Intentar retirar más dinero del disponible.
- Intentar retirar una cantidad negativa.
- Realizar un ingreso correcto.
- Introducir una opción de menú inexistente.
- Salir del cajero y comprobar el saldo final.
Una cantidad igual o inferior a cero muestra Cantidad no válida. Una retirada positiva superior al saldo muestra Saldo insuficiente. En ambos casos, el saldo se conserva y el menú vuelve a aparecer.
Nombre del proyecto: UT2Ejer38Cajero
