using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

double total = 0;
int productos = 0;

while (true)
{
    Console.Write("Introduce el precio del producto (0 para terminar): ");
    string? entrada = Console.ReadLine();

    if (entrada == null)
    {
        break;
    }

    if (!double.TryParse(entrada, out double precio) || !double.IsFinite(precio))
    {
        Console.WriteLine("Precio no válido.");
        continue;
    }

    if (precio < 0)
    {
        Console.WriteLine("Precio no válido.");
        continue;
    }

    if (precio == 0)
    {
        break;
    }

    total += precio;
    productos++;
    Console.WriteLine($"Producto {productos} registrado.");
    Console.WriteLine($"Total acumulado: {total:F2} €");

    if (precio > 100)
    {
        Console.WriteLine("Producto de precio elevado.");
    }
}

Console.WriteLine("--- RESUMEN DE COMPRA ---");
Console.WriteLine($"Productos registrados: {productos}");
Console.WriteLine($"Total de la compra: {total:F2} €");
