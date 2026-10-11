using System;

int capacidad = 20;
int vehiculos = 0;
string opcion;

do
{
    Console.WriteLine("--- APARCAMIENTO ---");
    Console.WriteLine($"Vehículos: {vehiculos} / {capacidad}");
    Console.WriteLine("1. Entrada de vehículos");
    Console.WriteLine("2. Salida de vehículos");
    Console.WriteLine("3. Consultar estado");
    Console.WriteLine("4. Salir");
    Console.Write("Selecciona una opción: ");
    opcion = Console.ReadLine() ?? "4";

    switch (opcion)
    {
        case "1":
        {
            Console.Write("¿Cuántos vehículos quieren entrar? ");
            int cantidad;
            if (!int.TryParse(Console.ReadLine(), out cantidad) || cantidad <= 0)
                Console.WriteLine("Error: la cantidad debe ser mayor que 0.");
            else if (cantidad > capacidad - vehiculos)
            {
                Console.WriteLine("No hay suficientes plazas.");
                Console.WriteLine($"Plazas disponibles: {capacidad - vehiculos}");
            }
            else
            {
                vehiculos += cantidad;
                Console.WriteLine($"Han entrado {cantidad} vehículos.");
                Console.WriteLine($"Vehículos: {vehiculos} / {capacidad}");
                Console.WriteLine($"Plazas libres: {capacidad - vehiculos}");
            }
            break;
        }
        case "2":
        {
            Console.Write("¿Cuántos vehículos quieren salir? ");
            int cantidad;
            if (!int.TryParse(Console.ReadLine(), out cantidad) || cantidad <= 0)
                Console.WriteLine("Error: la cantidad debe ser mayor que 0.");
            else if (cantidad > vehiculos)
                Console.WriteLine($"Error: solamente hay {vehiculos} vehículos en el aparcamiento.");
            else
            {
                vehiculos -= cantidad;
                Console.WriteLine($"Han salido {cantidad} vehículos.");
                Console.WriteLine($"Vehículos: {vehiculos} / {capacidad}");
                Console.WriteLine($"Plazas libres: {capacidad - vehiculos}");
            }
            break;
        }
        case "3":
            Console.WriteLine("--- ESTADO DEL APARCAMIENTO ---");
            Console.WriteLine($"Capacidad: {capacidad}");
            Console.WriteLine($"Vehículos aparcados: {vehiculos}");
            Console.WriteLine($"Plazas disponibles: {capacidad - vehiculos}");
            if (vehiculos == 0)
                Console.WriteLine("El aparcamiento está vacío.");
            else if (vehiculos == capacidad)
                Console.WriteLine("El aparcamiento está completo.");
            break;
        case "4":
            break;
        default:
            Console.WriteLine("Error: opción no válida.");
            break;
    }
} while (opcion != "4");

Console.WriteLine("--- CIERRE DEL APARCAMIENTO ---");
Console.WriteLine($"Vehículos actualmente aparcados: {vehiculos}");
Console.WriteLine($"Plazas disponibles: {capacidad - vehiculos}");
Console.WriteLine("Programa finalizado.");
