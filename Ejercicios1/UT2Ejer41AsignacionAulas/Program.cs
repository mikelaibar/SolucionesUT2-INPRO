using System;

int aula1 = 0;
int aula2 = 0;
int aula3 = 0;
int aula4 = 0;

while (true)
{
    Console.Write("Introduce la primera letra del apellido (0 o ENTER para terminar): ");
    string entrada = Console.ReadLine() ?? "";

    if (entrada == "" || entrada == "0")
        break;

    entrada = entrada.ToUpperInvariant();
    if (entrada.Length != 1 || entrada[0] < 'A' || entrada[0] > 'Z')
    {
        Console.WriteLine("Error: el carácter introducido no es válido.");
        continue;
    }

    char letra = entrada[0];
    int aula;
    if (letra <= 'F')
    {
        aula = 1;
        aula1++;
    }
    else if (letra <= 'J')
    {
        aula = 2;
        aula2++;
    }
    else if (letra <= 'N')
    {
        aula = 3;
        aula3++;
    }
    else
    {
        aula = 4;
        aula4++;
    }

    Console.WriteLine($"Al estudiante le corresponde el Aula {aula}");
}

Console.WriteLine("--- RESUMEN DE ASIGNACIONES ---");
Console.WriteLine($"Aula 1: {aula1} alumnos");
Console.WriteLine($"Aula 2: {aula2} alumnos");
Console.WriteLine($"Aula 3: {aula3} alumnos");
Console.WriteLine($"Aula 4: {aula4} alumnos");
Console.WriteLine($"Total: {aula1 + aula2 + aula3 + aula4} alumnos");
