# Ejercicio 1.41

41. Asignación de aulas
Crea un programa para asignar estudiantes a diferentes aulas según la primera letra de su apellido. El programa deberá permitir introducir tantos estudiantes como sea necesario y llevar un recuento de las asignaciones.
Solicita repetidamente al usuario la primera letra del apellido:
Introduce la primera letra del apellido (0 o ENTER para terminar):
La asignación de aulas se realiza de la siguiente manera:
- Apellidos desde la A hasta la F → Aula 1.
- Apellidos desde la G hasta la J → Aula 2.
- Apellidos desde la K hasta la N → Aula 3.
- Apellidos desde la O hasta la Z → Aula 4.
Por ejemplo:
Introduce la primera letra del apellido: B
Al estudiante le corresponde el Aula 1
Introduce la primera letra del apellido: M
Al estudiante le corresponde el Aula 3
El programa deberá aceptar letras tanto mayúsculas como minúsculas.
Si el usuario introduce un carácter que no sea una letra comprendida entre A y Z, muestra:
Error: el carácter introducido no es válido.
Una entrada incorrecta no debe contabilizarse como estudiante.
El programa continuará solicitando letras hasta que el usuario introduzca 0 o pulse ENTER sin escribir ningún carácter.
Durante la ejecución deberás llevar cuatro contadores independientes: uno para cada aula. Al finalizar muestra:
--- RESUMEN DE ASIGNACIONES ---
Aula 1: 5 alumnos
Aula 2: 3 alumnos
Aula 3: 4 alumnos
Aula 4: 6 alumnos
Total: 18 alumnos
Antes de convertir la entrada a char, comprueba si el usuario ha pulsado ENTER sin introducir texto. Piensa también cómo puedes trabajar siempre con la letra en mayúsculas para simplificar las comparaciones.
Nombre del proyecto: UT2Ejer41AsignacionAulas
