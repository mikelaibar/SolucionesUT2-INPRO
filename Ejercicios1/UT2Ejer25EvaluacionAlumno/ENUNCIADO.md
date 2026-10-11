# Ejercicio 1.25

25. Evaluación de un alumno
No siempre basta con observar un único dato. En este ejercicio tendrás que combinar la nota, la asistencia y la entrega de un proyecto.
Dispones de:
double nota = 7.5;
int asistencia = 85;
bool proyectoEntregado = true;
Aplica las siguientes reglas:
- Si el proyecto no está entregado, el resultado será Proyecto pendiente.
- Si la asistencia es inferior al 80 %, el resultado será No cumple la asistencia mínima.
- Si cumple los requisitos anteriores pero la nota es inferior a 5, mostrará Suspenso.
- Con una nota desde 5 hasta menos de 7: Aprobado.
- Desde 7 hasta menos de 9: Notable.
- Desde 9 hasta 10: Sobresaliente.
El programa debe mostrar un único resultado final.
Antes de programar, piensa qué condiciones deben comprobarse primero. El orden de los if puede cambiar el resultado.
Nombre del proyecto: UT2Ejer25EvaluacionAlumno
