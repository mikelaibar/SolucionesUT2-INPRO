using System;

int numeroSecreto = 37;
int intentos = 0;
bool acertado = false;

while (intentos < 7)
{
    Console.Write("Introduce un número: ");
    string? entrada = Console.ReadLine();

    if (entrada == null)
    {
        break;
    }

    intentos++;

    if (!int.TryParse(entrada, out int numero))
    {
        Console.WriteLine("Introduce un número entero válido.");
        continue;
    }

    if (numero < numeroSecreto)
    {
        Console.WriteLine("El número secreto es mayor.");
    }
    else if (numero > numeroSecreto)
    {
        Console.WriteLine("El número secreto es menor.");
    }
    else
    {
        Console.WriteLine("¡Has acertado!");
        acertado = true;
        break;
    }
}

Console.WriteLine($"Intentos utilizados: {intentos}");

if (!acertado && intentos == 7)
{
    Console.WriteLine($"Has perdido. El número secreto era {numeroSecreto}.");
}
