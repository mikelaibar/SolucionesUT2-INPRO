using System;

string marca = "Seat";
string modelo = "Ibiza";
string matricula = "0000ABC";
int anioFabricacion = 2018;
double precio = 8500.0;
int kilometrosRecorridos = 95000;
bool tieneItvEnVigor = true;
// No usamos cero para un dato desconocido: usamos null.
int? propietariosAnteriores = null;
double? consumoMedio = null;

Console.WriteLine("--- VEHÍCULO DE SEGUNDA MANO ---");
Console.WriteLine("Marca: " + marca);
Console.WriteLine("Modelo: " + modelo);
Console.WriteLine("Matrícula de ejemplo: " + matricula);
Console.WriteLine("Año de fabricación: " + anioFabricacion);
Console.WriteLine("Precio: " + precio + " euros");
Console.WriteLine("Kilómetros recorridos: " + kilometrosRecorridos + " km");
Console.WriteLine("ITV en vigor: " + tieneItvEnVigor);
Console.WriteLine("Propietarios anteriores: " + propietariosAnteriores + " (null: desconocido)");
Console.WriteLine("Consumo medio (L/100 km): " + consumoMedio + " (null: desconocido)");
