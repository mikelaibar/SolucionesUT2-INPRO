using System;

double nota = 7.5;
int asistencia = 85;
bool proyectoEntregado = true;

// El proyecto y la asistencia tienen prioridad sobre la nota.
if (!proyectoEntregado)
{
    Console.WriteLine("Proyecto pendiente");
}
else if (asistencia < 80)
{
    Console.WriteLine("No cumple la asistencia mínima");
}
else if (nota < 5)
{
    Console.WriteLine("Suspenso");
}
else if (nota < 7)
{
    Console.WriteLine("Aprobado");
}
else if (nota < 9)
{
    Console.WriteLine("Notable");
}
else if (nota <= 10)
{
    Console.WriteLine("Sobresaliente");
}
else
{
    Console.WriteLine("Nota fuera de rango");
}
