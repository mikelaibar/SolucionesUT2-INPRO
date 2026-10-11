using System;

double precioCamiseta = 19.95;
int cantidadCamisetas = 2;
double precioPantalon = 35.50;
int cantidadPantalones = 1;

double totalCamisetas = precioCamiseta * cantidadCamisetas;
double totalPantalones = precioPantalon * cantidadPantalones;
double totalCompra = totalCamisetas + totalPantalones;

Console.WriteLine("--- TICKET ---");
Console.WriteLine($"Camisetas: {totalCamisetas:F2} €");
Console.WriteLine($"Pantalones: {totalPantalones:F2} €");
Console.WriteLine($"TOTAL: {totalCompra:F2} €");
