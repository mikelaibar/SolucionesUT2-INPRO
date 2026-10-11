using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

double saldo = 1000;
string? opcion;

do
{
    Console.WriteLine("--- CAJERO AUTOMÁTICO ---");
    Console.WriteLine("1. Consultar saldo");
    Console.WriteLine("2. Retirar dinero");
    Console.WriteLine("3. Ingresar dinero");
    Console.WriteLine("4. Salir");
    Console.Write("Selecciona una opción: ");
    opcion = Console.ReadLine();

    if (opcion == null)
    {
        break;
    }

    switch (opcion)
    {
        case "1":
            Console.WriteLine($"Saldo disponible: {saldo:F2} €");
            break;
        case "2":
            Console.Write("Cantidad que deseas retirar: ");
            string? entradaRetirada = Console.ReadLine();

            if (entradaRetirada == null)
            {
                opcion = "4";
                break;
            }

            if (!double.TryParse(entradaRetirada, out double retirada)
                || !double.IsFinite(retirada) || retirada <= 0)
            {
                Console.WriteLine("Cantidad no válida.");
            }
            else if (retirada > saldo)
            {
                Console.WriteLine("Saldo insuficiente.");
            }
            else
            {
                saldo -= retirada;
                Console.WriteLine($"Nuevo saldo: {saldo:F2} €");
            }

            break;
        case "3":
            Console.Write("Cantidad que deseas ingresar: ");
            string? entradaIngreso = Console.ReadLine();

            if (entradaIngreso == null)
            {
                opcion = "4";
                break;
            }

            if (!double.TryParse(entradaIngreso, out double ingreso)
                || !double.IsFinite(ingreso) || ingreso <= 0)
            {
                Console.WriteLine("Cantidad no válida.");
            }
            else
            {
                saldo += ingreso;
                Console.WriteLine($"Nuevo saldo: {saldo:F2} €");
            }

            break;
        case "4":
            break;
        default:
            Console.WriteLine("Opción no válida.");
            break;
    }
}
while (opcion != "4");

Console.WriteLine($"Saldo final: {saldo:F2} €");
Console.WriteLine("Gracias por utilizar el cajero.");
