using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

Console.Write("¿Cuántos productos diferentes vas a comprar? ");
int productos = int.Parse(Console.ReadLine() ?? "0");
if (productos < 0)
{
    Console.WriteLine("Error: el número de productos no puede ser negativo.");
    return;
}

double importe = 0;
for (int producto = 1; producto <= productos; producto++)
{
    Console.Write("Nombre del producto: ");
    string nombre = Console.ReadLine() ?? "";
    Console.Write("Precio por unidad: ");
    double precio = double.Parse(Console.ReadLine() ?? "0");
    Console.Write("Cantidad: ");
    int cantidad = int.Parse(Console.ReadLine() ?? "0");

    double subtotal = precio * cantidad;
    importe += subtotal;
    Console.WriteLine($"Producto: {nombre}");
    Console.WriteLine($"Precio: {precio:F2} €");
    Console.WriteLine($"Cantidad: {cantidad}");
    Console.WriteLine($"Subtotal: {subtotal:F2} €");
}

// El descuento depende del importe de la compra completa.
double porcentaje = 0;
if (importe >= 200)
    porcentaje = 0.15;
else if (importe >= 100)
    porcentaje = 0.10;
else if (importe >= 50)
    porcentaje = 0.05;

double descuento = importe * porcentaje;
Console.WriteLine("--- FACTURA ---");
Console.WriteLine($"Productos comprados: {productos}");
Console.WriteLine($"Importe sin descuento: {importe:F2} €");
Console.WriteLine($"Descuento aplicado: {descuento:F2} €");
Console.WriteLine($"TOTAL: {importe - descuento:F2} €");
