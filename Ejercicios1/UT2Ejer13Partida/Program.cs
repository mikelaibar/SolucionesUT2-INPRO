using System;

int puntos = 100;
int vidas = 3;
int monedas = 10;

puntos += 50;
monedas += 5;
vidas--;
puntos *= 2;
monedas -= 3;
vidas++;

Console.WriteLine("--- ESTADO DE LA PARTIDA ---");
Console.WriteLine($"Puntos: {puntos}");
Console.WriteLine($"Vidas: {vidas}");
Console.WriteLine($"Monedas: {monedas}");
