using System;

double totalCompra = 67.50;
bool esPremium = false;
bool tieneCupon = true;
int productos = 4;

bool compraSupera50 = totalCompra > 50;
bool clientePremium = esPremium;
bool cuponYProductosSuficientes = tieneCupon && productos >= 3;
bool envioGratuito = compraSupera50 || esPremium;
bool promocionEspecial = esPremium || (tieneCupon && totalCompra > 30);
bool sinAccesoPromocionExclusiva = !esPremium;

Console.WriteLine($"La compra supera los 50 €: {compraSupera50}");
Console.WriteLine($"El cliente es Premium: {clientePremium}");
Console.WriteLine($"Tiene cupón y al menos 3 productos: {cuponYProductosSuficientes}");
Console.WriteLine($"Obtiene envío gratuito: {envioGratuito}");
Console.WriteLine($"Puede utilizar la promoción especial: {promocionEspecial}");
Console.WriteLine($"No puede acceder a la promoción exclusiva: {sinAccesoPromocionExclusiva}");
