using System;

// Sistema elegido: ficha de un ordenador de una tienda.
string marca = "AulaPC";
string modelo = "Estudiante 15";
string procesador = "Intel Core i5";
int memoriaRam = 16;
int almacenamiento = 512;
double precio = 599.95;
double peso = 1.8;
bool disponible = true;
char seccion = 'I';
int unidadesDisponibles = 8;
double? valoracionMedia = null;

Console.WriteLine("--- ORDENADOR ---");
Console.WriteLine("Marca: " + marca);
Console.WriteLine("Modelo: " + modelo);
Console.WriteLine("Procesador: " + procesador);
Console.WriteLine("Memoria RAM: " + memoriaRam + " GB");
Console.WriteLine("Almacenamiento: " + almacenamiento + " GB");
Console.WriteLine("Precio: " + precio + " euros");
Console.WriteLine("Peso: " + peso + " kg");
Console.WriteLine("Disponible: " + disponible);
Console.WriteLine("Sección: " + seccion);
Console.WriteLine("Unidades disponibles: " + unidadesDisponibles);
Console.WriteLine("Valoración media: " + valoracionMedia + " (null: todavía no hay valoraciones)");
