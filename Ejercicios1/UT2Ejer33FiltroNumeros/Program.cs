using System;

int cantidad = 0;
int suma = 0;

for (int numero = 1; numero <= 50; numero++)
{
    if (numero % 3 == 0)
    {
        continue;
    }

    if (numero % 2 == 0)
    {
        Console.WriteLine(numero);
        cantidad++;
        suma += numero;
    }
}

Console.WriteLine($"Cantidad de números mostrados: {cantidad}");
Console.WriteLine($"Suma total: {suma}");
