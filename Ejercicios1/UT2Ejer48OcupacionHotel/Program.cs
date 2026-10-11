using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

Console.Write("Número de plantas del hotel: ");
int plantas = int.Parse(Console.ReadLine() ?? "0");
Console.Write("Habitaciones por planta: ");
int habitacionesPorPlanta = int.Parse(Console.ReadLine() ?? "0");
if (plantas <= 0 || habitacionesPorPlanta <= 0)
{
    Console.WriteLine("Error: el número de plantas y de habitaciones debe ser mayor que 0.");
    return;
}

int habitacionesTotales = 0;
int ocupadas = 0;
int libres = 0;

for (int planta = 1; planta <= plantas; planta++)
{
    for (int habitacion = 1; habitacion <= habitacionesPorPlanta; habitacion++)
    {
        Console.WriteLine($"Planta {planta} - Habitación {habitacion}");
        string respuesta;
        do
        {
            Console.Write("¿Está ocupada? (S/N): ");
            string? entrada = Console.ReadLine();
            if (entrada == null)
                return;
            respuesta = entrada.ToUpperInvariant();
            if (respuesta != "S" && respuesta != "N")
                Console.WriteLine("Error: introduce S o N.");
        } while (respuesta != "S" && respuesta != "N");

        habitacionesTotales++;
        if (respuesta == "S")
            ocupadas++;
        else
            libres++;
    }
}

double porcentaje = ocupadas * 100.0 / habitacionesTotales;
Console.WriteLine("--- OCUPACIÓN DEL HOTEL ---");
Console.WriteLine($"Habitaciones totales: {habitacionesTotales}");
Console.WriteLine($"Ocupadas: {ocupadas}");
Console.WriteLine($"Libres: {libres}");
Console.WriteLine($"Porcentaje de ocupación: {porcentaje:F2} %");
