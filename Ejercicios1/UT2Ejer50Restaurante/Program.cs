using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

int pedidos = 0;
double recaudacion = 0;
string otroPedido = "S";

while (otroPedido == "S")
{
    Console.Write("Nombre del cliente: ");
    string? cliente = Console.ReadLine();
    if (cliente == null)
        return;

    // Estas variables pertenecen al pedido y se reinician para cada cliente.
    double subtotal = 0;
    int unidadesPedidas = 0;

    while (true)
    {
        Console.WriteLine("--- CARTA ---");
        Console.WriteLine("1. Hamburguesa ........ 9.50 €");
        Console.WriteLine("2. Pizza .............. 12.00 €");
        Console.WriteLine("3. Ensalada ........... 8.00 €");
        Console.WriteLine("4. Bocadillo .......... 7.50 €");
        Console.WriteLine("5. Refresco ........... 2.50 €");
        Console.WriteLine("6. Finalizar pedido");
        Console.Write("Selecciona un producto: ");
        string opcion = Console.ReadLine() ?? "6";
        if (opcion == "6")
            break;

        string producto;
        double precio;
        switch (opcion)
        {
            case "1":
                producto = "Hamburguesa";
                precio = 9.50;
                break;
            case "2":
                producto = "Pizza";
                precio = 12.00;
                break;
            case "3":
                producto = "Ensalada";
                precio = 8.00;
                break;
            case "4":
                producto = "Bocadillo";
                precio = 7.50;
                break;
            case "5":
                producto = "Refresco";
                precio = 2.50;
                break;
            default:
                Console.WriteLine("Error: opción no válida.");
                continue;
        }

        Console.WriteLine($"Has seleccionado: {producto}");
        Console.Write("¿Cuántas unidades? ");
        int cantidad;
        if (!int.TryParse(Console.ReadLine(), out cantidad) || cantidad <= 0)
        {
            Console.WriteLine("Error: la cantidad debe ser mayor que 0.");
            continue;
        }

        double importeProducto = cantidad * precio;
        subtotal += importeProducto;
        unidadesPedidas += cantidad;
        Console.WriteLine($"{cantidad} unidades de {producto} x {precio:F2} € = {importeProducto:F2} €");
        Console.WriteLine("Producto añadido correctamente.");
        Console.WriteLine($"Importe actual del pedido: {subtotal:F2} €");
    }

    // El descuento se calcula una sola vez sobre el pedido completo.
    int porcentaje = 0;
    if (subtotal >= 60)
        porcentaje = 15;
    else if (subtotal >= 40)
        porcentaje = 10;
    else if (subtotal >= 20)
        porcentaje = 5;

    double descuento = subtotal * porcentaje / 100;
    double total = subtotal - descuento;
    Console.WriteLine("--- TICKET ---");
    Console.WriteLine($"Cliente: {cliente}");
    Console.WriteLine($"Unidades pedidas: {unidadesPedidas}");
    Console.WriteLine($"Subtotal: {subtotal:F2} €");
    Console.WriteLine($"Descuento aplicado: {porcentaje} %");
    Console.WriteLine($"Descuento: {descuento:F2} €");
    Console.WriteLine($"TOTAL: {total:F2} €");

    pedidos++;
    recaudacion += total;

    do
    {
        Console.Write("¿Deseas registrar otro pedido? (S/N): ");
        otroPedido = (Console.ReadLine() ?? "N").ToUpperInvariant();
        if (otroPedido != "S" && otroPedido != "N")
            Console.WriteLine("Error: introduce S o N.");
    } while (otroPedido != "S" && otroPedido != "N");
}

Console.WriteLine("--- CIERRE DEL RESTAURANTE ---");
Console.WriteLine($"Pedidos realizados: {pedidos}");
Console.WriteLine($"Recaudación total: {recaudacion:F2} €");
Console.WriteLine("Restaurante cerrado.");
