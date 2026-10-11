using System;

for (int vuelta = 1; vuelta <= 10; vuelta++)
{
    Console.WriteLine($"Vuelta {vuelta}");

    if (vuelta % 3 == 0)
    {
        Console.WriteLine("Punto de hidratación.");
    }

    if (vuelta == 5)
    {
        Console.WriteLine("Mitad del entrenamiento aproximadamente.");
    }
}

Console.WriteLine("Entrenamiento completado.");
