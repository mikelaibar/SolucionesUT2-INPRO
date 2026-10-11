using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

int edad = 20;
bool estudiante = true;
bool familiaNumerosa = true;
double precio;

// Comprobamos las tarifas de menor a mayor precio.
if (edad < 6)
{
    precio = 0;
}
else if (edad >= 65)
{
    precio = 6;
}
else if (edad <= 17)
{
    precio = 8;
}
else if (familiaNumerosa)
{
    precio = 9;
}
else if (estudiante)
{
    precio = 10;
}
else
{
    precio = 15;
}

Console.WriteLine("--- ENTRADA ---");
Console.WriteLine($"Precio final: {precio:F2} €");
