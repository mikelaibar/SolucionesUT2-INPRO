using System;

Console.Write("Introduce la altura: ");
int altura = int.Parse(Console.ReadLine() ?? "0");
if (altura < 1)
{
    Console.WriteLine("Error: la altura debe ser mayor que 0.");
    return;
}

// Primero se muestra el triángulo que solicita el enunciado.
for (int fila = 1; fila <= altura; fila++)
{
    for (int columna = 1; columna <= fila; columna++)
        Console.Write("*");
    Console.WriteLine();
}

Console.WriteLine("1. Triángulo creciente");
Console.WriteLine("2. Triángulo decreciente");
Console.WriteLine("3. Cuadrado");
Console.Write("¿Qué figura quieres generar? ");
string opcion = Console.ReadLine() ?? "";

switch (opcion)
{
    case "1":
        for (int fila = 1; fila <= altura; fila++)
        {
            for (int columna = 1; columna <= fila; columna++)
                Console.Write("*");
            Console.WriteLine();
        }
        break;
    case "2":
        for (int fila = altura; fila >= 1; fila--)
        {
            for (int columna = 1; columna <= fila; columna++)
                Console.Write("*");
            Console.WriteLine();
        }
        break;
    case "3":
        for (int fila = 1; fila <= altura; fila++)
        {
            for (int columna = 1; columna <= altura; columna++)
                Console.Write("*");
            Console.WriteLine();
        }
        break;
    default:
        Console.WriteLine("Error: opción no válida.");
        break;
}
