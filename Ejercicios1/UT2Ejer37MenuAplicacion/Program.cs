using System;

string? opcion;

do
{
    Console.WriteLine("--- MENÚ ---");
    Console.WriteLine("1. Mostrar saludo");
    Console.WriteLine("2. Mostrar curso académico");
    Console.WriteLine("3. Mostrar nombre del módulo");
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
            Console.WriteLine("¡Hola! Bienvenido a la aplicación.");
            break;
        case "2":
            Console.WriteLine("Curso 2026/2027.");
            break;
        case "3":
            Console.WriteLine("Iniciación a la Programación.");
            break;
        case "4":
            Console.WriteLine("Cerrando programa...");
            break;
        default:
            Console.WriteLine("Opción incorrecta.");
            break;
    }
}
while (opcion != "4");
