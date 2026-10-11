using System;

// Los números y los booleanos se escriben sin comillas.
int edad = 24;
double altura = 1.78;
// char usa comillas simples y string usa comillas dobles.
char inicial = 'M';
string nombre = "Mario";
bool activo = true;
int? puntos = null;
// En el código C# los decimales se escriben con punto.
double temperatura = 36.7;
// Un teléfono es un identificador, no una cantidad para calcular.
string telefono = "654123987";

Console.WriteLine("--- USUARIO ---");
Console.WriteLine("Nombre: " + nombre);
Console.WriteLine("Edad: " + edad);
Console.WriteLine("Altura: " + altura + " m");
Console.WriteLine("Inicial: " + inicial);
Console.WriteLine("Activo: " + activo);
Console.WriteLine("Puntos: " + puntos + " (null: no conocidos)");
Console.WriteLine("Temperatura: " + temperatura + " °C");
Console.WriteLine("Teléfono de ejemplo: " + telefono);
