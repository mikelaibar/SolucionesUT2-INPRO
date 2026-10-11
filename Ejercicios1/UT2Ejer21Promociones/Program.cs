using System;

double compra = 85.50;
int productos = 6;
bool esSocio = true;

if (compra > 50)
{
    Console.WriteLine("Envío gratuito");
}

if (productos >= 5)
{
    Console.WriteLine("Regalo promocional incluido");
}

if (esSocio)
{
    Console.WriteLine("Acumulas puntos con esta compra");
}

if (compra > 100)
{
    Console.WriteLine("Cupón de 10 € para tu próxima compra");
}
