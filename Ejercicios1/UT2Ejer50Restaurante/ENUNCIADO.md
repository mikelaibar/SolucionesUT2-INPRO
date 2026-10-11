# Ejercicio 1.50

50. Restaurante
Crea un programa interactivo que permita gestionar los pedidos de un restaurante. El programa deberá atender a varios clientes, permitir que cada uno añada varios productos a su pedido y calcular tanto el importe de cada pedido como la recaudación total del restaurante.
El restaurante dispone de la siguiente carta:
--- CARTA ---
1. Hamburguesa ........ 9.50 €
2. Pizza .............. 12.00 €
3. Ensalada ........... 8.00 €
4. Bocadillo .......... 7.50 €
5. Refresco ........... 2.50 €
6. Finalizar pedido
1. Comenzar un pedido
Al comenzar cada nuevo pedido, solicita al usuario el nombre del cliente:
Nombre del cliente: Ane
Cada cliente comienza con un pedido vacío, por lo que deberás disponer de variables para almacenar, como mínimo:
- El importe acumulado de su pedido.
- El número total de unidades pedidas.
Estas variables deberán comenzar de nuevo desde cero cada vez que llegue un nuevo cliente.
2. Añadir productos al pedido
Después de introducir el nombre, muestra la carta y solicita una opción:
--- CARTA ---
1. Hamburguesa ........ 9.50 €
2. Pizza .............. 12.00 €
3. Ensalada ........... 8.00 €
4. Bocadillo .......... 7.50 €
5. Refresco ........... 2.50 €
6. Finalizar pedido
Selecciona un producto:
Si el usuario selecciona una opción entre 1 y 5, pregunta cuántas unidades desea:
Selecciona un producto: 2
Has seleccionado: Pizza
¿Cuántas unidades? 3
La cantidad debe ser mayor que 0.
Si introduce una cantidad igual o inferior a 0, muestra:
Error: la cantidad debe ser mayor que 0.
En ese caso, el producto no deberá añadirse al pedido y se volverá a mostrar la carta.
Si la cantidad es correcta, calcula el importe:
3 pizzas x 12.00 € = 36.00 €
Producto añadido correctamente.
Importe actual del pedido: 36.00 €
Después de añadir el producto, vuelve a mostrar la carta para que el cliente pueda seguir realizando su pedido.
3. Opciones incorrectas
Si se introduce una opción que no esté comprendida entre 1 y 6, muestra:
Error: opción no válida.
No debe modificarse el pedido y la carta deberá volver a mostrarse.
4. Finalizar el pedido
El cliente podrá seleccionar la opción:
6. Finalizar pedido
Cuando lo haga, deja de solicitar productos y calcula el descuento que corresponde al pedido completo.
Los descuentos son:
- Pedido inferior a 20 € → sin descuento.
- Desde 20 € hasta menos de 40 € → 5 % de descuento.
- Desde 40 € hasta menos de 60 € → 10 % de descuento.
- 60 € o más → 15 % de descuento.
Por ejemplo, para un pedido de 50 €:
Subtotal: 50.00 €
Descuento aplicado: 10 %
Descuento: 5.00 €
Total a pagar: 45.00 €
El descuento se calcula una única vez al finalizar el pedido, no después de añadir cada producto.
5. Mostrar el ticket
Una vez calculado el precio final, muestra:
--- TICKET ---
Cliente: Ane
Unidades pedidas: 6
Subtotal: 50.00 €
Descuento: 5.00 €
TOTAL: 45.00 €
Al finalizar correctamente un pedido:
- Incrementa el número total de pedidos realizados.
- Suma el importe final pagado por el cliente a la recaudación del restaurante.
6. Atender a otro cliente
Después de mostrar el ticket pregunta:
¿Deseas registrar otro pedido? (S/N):
Si el usuario introduce S:
- Comienza un nuevo pedido.
- Solicita el nombre del siguiente cliente.
- El subtotal y las unidades del nuevo pedido deben volver a 0.
- La recaudación y el número total de pedidos no deben reiniciarse.
Si introduce N, termina la jornada.
Si introduce cualquier otro carácter, vuelve a solicitar S o N hasta recibir una respuesta válida.
7. Cierre del restaurante
Cuando ya no se quieran registrar más clientes, muestra:
--- CIERRE DEL RESTAURANTE ---
Pedidos realizados: ...
Recaudación total: ... €
Restaurante cerrado.
Piensa en la estructura del programa: existe una repetición para atender a varios clientes y, dentro de cada cliente, otra repetición para permitir añadir varios productos. Por tanto, este ejercicio requerirá trabajar con bucles anidados.
Importante: algunas variables pertenecen a un único pedido y deben reiniciarse al comenzar el siguiente, mientras que otras pertenecen a toda la jornada y deben conservar su valor. Decide cuidadosamente dónde declaras e inicializas cada variable.
Ejemplo de interacción
Nombre del cliente: Ane
--- CARTA ---
1. Hamburguesa ........ 9.50 €
2. Pizza .............. 12.00 €
3. Ensalada ........... 8.00 €
4. Bocadillo .......... 7.50 €
5. Refresco ........... 2.50 €
6. Finalizar pedido
Selecciona un producto: 1
¿Cuántas unidades? 2
2 hamburguesas x 9.50 € = 19.00 €
Importe actual del pedido: 19.00 €
Selecciona un producto: 5
¿Cuántas unidades? 2
2 refrescos x 2.50 € = 5.00 €
Importe actual del pedido: 24.00 €
Selecciona un producto: 6
--- TICKET ---
Cliente: Ane
Unidades pedidas: 4
Subtotal: 24.00 €
Descuento: 1.20 €
TOTAL: 22.80 €
¿Deseas registrar otro pedido? (S/N): N
--- CIERRE DEL RESTAURANTE ---
Pedidos realizados: 1
Recaudación total: 22.80 €
Restaurante cerrado.
Antes de dar el programa por terminado, comprueba como mínimo:
- Un pedido formado por un único producto.
- Un pedido con varios productos.
- Un producto con varias unidades.
- Una cantidad igual o inferior a 0.
- Una opción de la carta inexistente.
- Pedidos pertenecientes a distintos tramos de descuento.
- Dos o más clientes consecutivos.
- Que los datos de un pedido no se mezclen con los del siguiente.
- Que la recaudación sí acumule todos los pedidos realizados.
Nombre del proyecto: UT2Ejer50Restaurante
