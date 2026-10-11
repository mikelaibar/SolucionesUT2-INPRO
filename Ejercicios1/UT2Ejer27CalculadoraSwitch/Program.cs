using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

double numero1 = 20;
double numero2 = 5;
char operacion = '*';

switch (operacion)
{
    case '+':
        Console.WriteLine($"Resultado: {numero1 + numero2}");
        break;
    case '-':
        Console.WriteLine($"Resultado: {numero1 - numero2}");
        break;
    case '*':
        Console.WriteLine($"Resultado: {numero1 * numero2}");
        break;
    case '/':
        if (numero2 == 0)
        {
            Console.WriteLine("No se puede dividir entre cero");
        }
        else
        {
            Console.WriteLine($"Resultado: {numero1 / numero2}");
        }
        break;
    default:
        Console.WriteLine("Operación no válida");
        break;
}
