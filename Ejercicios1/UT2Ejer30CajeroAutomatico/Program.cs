using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

bool tarjetaActiva = true;
int pinCorrecto = 1234;
int pinIntroducido = 1234;
int intentosFallidos = 1;
double saldo = 850.00;
double cantidad = 200.00;
int operacion = 2;

if (!tarjetaActiva)
{
    Console.WriteLine("Tarjeta bloqueada");
}
else if (intentosFallidos >= 3)
{
    Console.WriteLine("Demasiados intentos. Tarjeta bloqueada.");
}
else if (pinIntroducido != pinCorrecto)
{
    Console.WriteLine("PIN incorrecto");
}
else
{
    switch (operacion)
    {
        case 1:
            Console.WriteLine($"Saldo actual: {saldo:F2} €");
            break;
        case 2:
            if (cantidad <= 0)
            {
                Console.WriteLine("La cantidad debe ser mayor que 0");
            }
            else if (cantidad > saldo)
            {
                Console.WriteLine("Saldo insuficiente");
            }
            else
            {
                saldo = saldo - cantidad;
                Console.WriteLine($"Nuevo saldo: {saldo:F2} €");
            }
            break;
        case 3:
            if (cantidad <= 0)
            {
                Console.WriteLine("La cantidad debe ser mayor que 0");
            }
            else
            {
                saldo = saldo + cantidad;
                Console.WriteLine($"Nuevo saldo: {saldo:F2} €");
            }
            break;
        case 4:
            Console.WriteLine("Gracias por utilizar el cajero.");
            break;
        default:
            Console.WriteLine("Operación no válida");
            break;
    }
}
