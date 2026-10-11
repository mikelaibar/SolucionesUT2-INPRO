using System;

string localidad = "Pamplona";
double temperatura = 16.5;
int humedad = 72;
bool estaLloviendo = false;
char direccionViento = 'N';
double velocidadViento = 12.7;

Console.WriteLine("--- ESTACIÓN METEOROLÓGICA ---");
Console.WriteLine("Localidad: " + localidad);
Console.WriteLine("Temperatura: " + temperatura + " °C");
Console.WriteLine("Humedad: " + humedad + " %");
Console.WriteLine("Está lloviendo: " + estaLloviendo);
Console.WriteLine("Dirección del viento: " + direccionViento);
Console.WriteLine("Velocidad del viento: " + velocidadViento + " km/h");
