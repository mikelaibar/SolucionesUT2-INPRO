using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

double ahorros = 100;
double ahorroMensual = 50;

for (int mes = 1; mes <= 12; mes++)
{
    ahorros += ahorroMensual;

    // Ampliación: los meses 6 y 12 incluyen una paga extra.
    if (mes % 6 == 0)
    {
        ahorros += 100;
        Console.WriteLine("Paga extra de 100 € recibida.");
    }

    Console.WriteLine($"Mes {mes} - Ahorros: {ahorros:F2} €");
}

Console.WriteLine($"Dinero total acumulado: {ahorros:F2} €");
