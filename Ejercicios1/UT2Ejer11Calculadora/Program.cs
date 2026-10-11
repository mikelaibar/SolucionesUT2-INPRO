using System;

int numero1 = 17;
int numero2 = 5;

int suma = numero1 + numero2;
int resta = numero1 - numero2;
int multiplicacion = numero1 * numero2;
// Al dividir dos int, la parte decimal se descarta.
int division = numero1 / numero2;
int resto = numero1 % numero2;

Console.WriteLine("--- CALCULADORA ---");
Console.WriteLine($"Suma: {suma}");
Console.WriteLine($"Resta: {resta}");
Console.WriteLine($"Multiplicación: {multiplicacion}");
Console.WriteLine($"División: {division}");
Console.WriteLine($"Resto: {resto}");
