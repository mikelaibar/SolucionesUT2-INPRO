using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

int entradasDisponibles = 50;
double recaudacion = 0;

while (entradasDisponibles > 0)
{
    Console.WriteLine($"Entradas disponibles: {entradasDisponibles}");
    Console.Write("¿Cuántas entradas desea comprar? (0 para cerrar): ");
    string entrada = Console.ReadLine() ?? "0";
    int cantidad;
    if (!int.TryParse(entrada, out cantidad) || cantidad < 0)
    {
        Console.WriteLine("Error: la cantidad debe ser un número entero no negativo.");
        continue;
    }
    if (cantidad == 0)
        break;
    if (cantidad > entradasDisponibles)
    {
        Console.WriteLine("No hay suficientes entradas disponibles.");
        continue;
    }

    double importe = cantidad * 12.0;
    if (cantidad >= 5)
        importe *= 0.90;

    entradasDisponibles -= cantidad;
    recaudacion += importe;
    Console.WriteLine($"Importe de la compra: {importe:F2} €");
    if (entradasDisponibles == 0)
        Console.WriteLine("ENTRADAS AGOTADAS");
}

Console.WriteLine("--- CIERRE DE VENTAS ---");
Console.WriteLine($"Entradas vendidas: {50 - entradasDisponibles}");
Console.WriteLine($"Entradas restantes: {entradasDisponibles}");
Console.WriteLine($"Recaudación total: {recaudacion:F2} €");
