using System;

Console.Write("¿Hasta qué tabla quieres mostrar? ");
int ultimaTabla = int.Parse(Console.ReadLine() ?? "0");
if (ultimaTabla < 1)
{
    Console.WriteLine("Error: debes introducir un número mayor o igual que 1.");
    return;
}

for (int tabla = 1; tabla <= ultimaTabla; tabla++)
{
    Console.WriteLine($"--- TABLA DEL {tabla} ---");
    for (int multiplicador = 1; multiplicador <= 10; multiplicador++)
        Console.WriteLine($"{tabla} x {multiplicador} = {tabla * multiplicador}");
}
