using System;

double temperatura = 27.5;

if (temperatura < 10)
{
    Console.WriteLine("Temperatura muy baja");
}
else if (temperatura < 18)
{
    Console.WriteLine("Hace frío");
}
else if (temperatura <= 25)
{
    Console.WriteLine("Temperatura agradable");
}
else if (temperatura <= 35)
{
    Console.WriteLine("Hace calor");
}
else
{
    Console.WriteLine("Temperatura muy alta");
}
