# Ejercicio 1.43

43. Gestión de un aparcamiento
Crea un programa interactivo para gestionar las entradas y salidas de vehículos de un aparcamiento. El programa deberá mostrar un menú y permitir al usuario realizar operaciones hasta que decida finalizar.
El aparcamiento tiene una capacidad máxima de 20 vehículos y comienza completamente vacío:
int capacidad = 20;
int vehiculos = 0;
El programa deberá mostrar repetidamente el siguiente menú:
--- APARCAMIENTO ---
Vehículos: 0 / 20
1. Entrada de vehículos
2. Salida de vehículos
3. Consultar estado
4. Salir
Selecciona una opción:
El usuario introducirá una opción mediante Console.ReadLine(). Dependiendo de la opción seleccionada, el programa realizará una acción diferente.
Opción 1 - Entrada de vehículos
Pregunta al usuario cuántos vehículos quieren entrar:
¿Cuántos vehículos quieren entrar?
Por ejemplo:
¿Cuántos vehículos quieren entrar? 4
Han entrado 4 vehículos.
Vehículos: 4 / 20
Plazas libres: 16
Antes de realizar la entrada deberás comprobar:
- La cantidad introducida debe ser mayor que 0.
- Debe haber suficientes plazas libres para todos los vehículos.
- El número de vehículos nunca puede superar la capacidad máxima del aparcamiento.
Si no existen suficientes plazas, la entrada no se realizará:
Vehículos: 18 / 20
¿Cuántos vehículos quieren entrar? 4
No hay suficientes plazas.
Plazas disponibles: 2
Opción 2 - Salida de vehículos
Pregunta al usuario cuántos vehículos quieren abandonar el aparcamiento:
¿Cuántos vehículos quieren salir?
Por ejemplo:
Vehículos: 12 / 20
¿Cuántos vehículos quieren salir? 3
Han salido 3 vehículos.
Vehículos: 9 / 20
Plazas libres: 11
No podrán salir más vehículos de los que se encuentran actualmente aparcados.
Por ejemplo:
Vehículos: 2 / 20
¿Cuántos vehículos quieren salir? 5
Error: solamente hay 2 vehículos en el aparcamiento.
Una cantidad igual o inferior a 0 también deberá considerarse incorrecta.
Opción 3 - Consultar estado
Esta opción no modifica ninguna variable. Únicamente muestra la situación actual:
--- ESTADO DEL APARCAMIENTO ---
Capacidad: 20
Vehículos aparcados: 9
Plazas disponibles: 11
Además:
- Si no hay ningún vehículo, muestra El aparcamiento está vacío.
- Si se han ocupado las 20 plazas, muestra El aparcamiento está completo.
Opción 4 - Salir
Cuando el usuario seleccione esta opción, el programa dejará de mostrar el menú y finalizará mostrando:
--- CIERRE DEL APARCAMIENTO ---
Vehículos actualmente aparcados: ...
Plazas disponibles: ...
Programa finalizado.
Si el usuario introduce una opción diferente de 1, 2, 3 o 4, muestra:
Error: opción no válida.
Después, vuelve a mostrar el menú normalmente.
Piensa antes de programar: necesitas una estructura que permita repetir el menú hasta seleccionar la opción de salida y otra que permita decidir qué hacer con cada opción. También necesitarás condicionales para comprobar si una entrada o una salida puede realizarse.
Antes de terminar, prueba al menos la siguiente secuencia:
1 → Entran 10 vehículos
1 → Entran 7 vehículos
1 → Intentan entrar 5 vehículos
3 → Consultar estado
2 → Salen 4 vehículos
2 → Intentan salir 20 vehículos
1 → Entran 3 vehículos
3 → Consultar estado
4 → Salir
Comprueba que el número de vehículos nunca puede ser inferior a 0 ni superior a 20.
Nombre del proyecto: UT2Ejer43Aparcamiento
