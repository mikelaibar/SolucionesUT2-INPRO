using System;

string nombreAlumno = "Alex Ejemplo";
int edad = 18;
string grupo = "2º SMR";
double notaPrimerExamen = 7.5;
// El signo ? permite que la nota sea null mientras no se conozca.
double? notaSegundoExamen = null;
bool haEntregadoProyecto = true;

Console.WriteLine("--- EXPEDIENTE ---");
Console.WriteLine("Nombre: " + nombreAlumno);
Console.WriteLine("Edad: " + edad);
Console.WriteLine("Grupo: " + grupo);
Console.WriteLine("Nota del primer examen: " + notaPrimerExamen);
// Al concatenar null no aparece un número; explicamos su significado.
Console.WriteLine("Nota del segundo examen: " + notaSegundoExamen + " (null: pendiente de corregir)");
Console.WriteLine("Ha entregado el proyecto: " + haEntregadoProyecto);
