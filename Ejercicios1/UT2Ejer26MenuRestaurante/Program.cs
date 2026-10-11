using System;

int opcion = 2;

switch (opcion)
{
    case 1:
        Console.WriteLine("Has elegido Hamburguesa - 9.50 €");
        break;
    case 2:
        Console.WriteLine("Has elegido Pizza - 11.00 €");
        break;
    case 3:
        Console.WriteLine("Has elegido Ensalada - 8.00 €");
        break;
    case 4:
        Console.WriteLine("Has elegido Bocadillo - 7.50 €");
        break;
    case 5:
        Console.WriteLine("Hasta pronto");
        break;
    default:
        Console.WriteLine("Opción incorrecta");
        break;
}
