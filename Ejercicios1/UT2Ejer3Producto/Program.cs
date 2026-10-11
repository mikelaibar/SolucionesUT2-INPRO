using System;

string nombreProducto = "Teclado mecánico";
// Un código es texto: así conservamos los ceros iniciales.
string codigoProducto = "00125A";
double precio = 49.95;
int unidadesDisponibles = 12;
bool disponible = true;
char seccion = 'I';

Console.WriteLine("--- PRODUCTO ---");
Console.WriteLine("Nombre: " + nombreProducto);
Console.WriteLine("Código: " + codigoProducto);
Console.WriteLine("Precio: " + precio + " euros");
Console.WriteLine("Unidades: " + unidadesDisponibles);
Console.WriteLine("Disponible: " + disponible);
Console.WriteLine("Sección: " + seccion);
