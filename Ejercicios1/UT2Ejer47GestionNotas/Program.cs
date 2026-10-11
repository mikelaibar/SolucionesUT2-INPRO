using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

Console.Write("¿Cuántos alumnos hay en el grupo? ");
int alumnos = int.Parse(Console.ReadLine() ?? "0");
if (alumnos <= 0)
{
    Console.WriteLine("Error: debe haber al menos un alumno.");
    return;
}

double suma = 0;
double notaMasAlta = 0;
double notaMasBaja = 10;
int aprobados = 0;
int suspensos = 0;

for (int alumno = 1; alumno <= alumnos; alumno++)
{
    double nota;
    while (true)
    {
        Console.Write($"Nota del alumno {alumno}: ");
        string? entrada = Console.ReadLine();
        if (entrada == null)
            return;
        if (double.TryParse(entrada, out nota) && nota >= 0 && nota <= 10)
            break;
        Console.WriteLine("Nota no válida. Introduce una nota entre 0 y 10.");
    }

    if (nota < 5)
    {
        Console.WriteLine("Suspenso.");
        suspensos++;
    }
    else
    {
        aprobados++;
        if (nota < 7)
            Console.WriteLine("Aprobado.");
        else if (nota < 9)
            Console.WriteLine("Notable.");
        else
            Console.WriteLine("Sobresaliente.");
    }

    suma += nota;
    if (nota > notaMasAlta)
        notaMasAlta = nota;
    if (nota < notaMasBaja)
        notaMasBaja = nota;
}

Console.WriteLine("--- RESULTADOS ---");
Console.WriteLine($"Nota media: {suma / alumnos:F2}");
Console.WriteLine($"Aprobados: {aprobados}");
Console.WriteLine($"Suspensos: {suspensos}");
Console.WriteLine($"Nota más alta: {notaMasAlta:F2}");
Console.WriteLine($"Nota más baja: {notaMasBaja:F2}");
