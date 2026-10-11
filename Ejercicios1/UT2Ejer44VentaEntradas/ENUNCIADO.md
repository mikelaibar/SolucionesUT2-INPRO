# Ejercicio 1.44

44. Venta de entradas
Crea un programa que gestione la venta de entradas para un evento hasta que se agoten o el vendedor decida cerrar la venta.
Inicialmente existen:
int entradasDisponibles = 50;
double recaudacion = 0;
Solicita repetidamente cuántas entradas quiere comprar un cliente:
Entradas disponibles: 50
¿Cuántas entradas desea comprar? (0 para cerrar):
Cada entrada cuesta 12 €.
Aplica estas reglas:
- Una cantidad negativa no es válida.
- Si solicita más entradas de las disponibles, la venta no se realiza.
- Si introduce 0, se cierra la venta.
- Después de cada compra correcta actualiza las entradas disponibles y la recaudación.
- Si las entradas llegan a 0, el programa debe finalizar automáticamente mostrando ENTRADAS AGOTADAS.
Además, si un cliente compra 5 entradas o más, se aplicará un descuento del 10 % sobre su compra.
Al finalizar muestra:
--- CIERRE DE VENTAS ---
Entradas vendidas: ...
Entradas restantes: ...
Recaudación total: ... €
Decide cuándo resulta apropiado utilizar break y cuándo una operación incorrecta debe simplemente permitir continuar con el siguiente cliente.
Nombre del proyecto: UT2Ejer44VentaEntradas
