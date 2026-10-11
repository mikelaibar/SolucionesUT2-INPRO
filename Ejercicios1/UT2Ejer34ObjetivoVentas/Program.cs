using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

double ventas = 1000;
double objetivo = 5000;
double ventasDiarias = 650;
int dia = 0;

// No conocemos los días necesarios: repetimos hasta alcanzar el objetivo.
while (ventas < objetivo)
{
    dia++;
    ventas += ventasDiarias;
    Console.WriteLine($"Día {dia} - Ventas acumuladas: {ventas:F2} €");
}

Console.WriteLine("Objetivo alcanzado");
Console.WriteLine($"Días necesarios: {dia}");
