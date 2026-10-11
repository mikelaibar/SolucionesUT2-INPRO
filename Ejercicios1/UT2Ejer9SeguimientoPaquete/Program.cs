using System;

string codigoSeguimiento = "PK00125A";
string nombreDestinatario = "Alex Ejemplo";
double pesoPaquete = 2.35;
int intentosEntrega = 1;
bool entregado = false;
char zonaReparto = 'B';
// No hay valoración porque el paquete todavía no se ha entregado.
double? valoracionServicio = null;

Console.WriteLine("--- SEGUIMIENTO DEL PAQUETE ---");
Console.WriteLine("Código: " + codigoSeguimiento);
Console.WriteLine("Destinatario: " + nombreDestinatario);
Console.WriteLine("Peso: " + pesoPaquete + " kg");
Console.WriteLine("Intentos de entrega: " + intentosEntrega);
Console.WriteLine("Entregado: " + entregado);
Console.WriteLine("Zona de reparto: " + zonaReparto);
Console.WriteLine("Valoración del servicio: " + valoracionServicio + " (null: pendiente de entrega)");
