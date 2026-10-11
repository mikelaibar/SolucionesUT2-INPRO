using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

double saldo = 45.00;
double precioVideojuego = 39.99;

if (saldo >= precioVideojuego)
{
    saldo = saldo - precioVideojuego;
    Console.WriteLine("Compra realizada correctamente");
}
else
{
    Console.WriteLine("Saldo insuficiente");
}

Console.WriteLine($"Saldo disponible: {saldo:F2} €");
