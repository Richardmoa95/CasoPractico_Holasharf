# CasoPractico_Holasharf
1. Webhook Receiver Pattern

Problema:
El TMS externo envía eventos en tiempo real sobre los cambios de estado de los pedidos.

Patrón elegido:
Webhook Receiver.

Justificación:
Permite recibir eventos externos mediante HTTP sin necesidad de consultar constantemente al TMS. Esto reduce latencia y permite reaccionar casi en tiempo real.

2. Message Queue Pattern

Problema:
El procesamiento del evento puede incluir varias operaciones: actualizar pedido, registrar histórico, guardar evidencias y notificar al cliente. No conviene hacer todo eso directamente dentro del request HTTP.

Patrón elegido:
Message Queue.

Justificación:
Permite desacoplar la recepción del webhook del procesamiento interno. La API responde rápido al TMS con 202 Accepted, mientras el Worker procesa el evento en segundo plano.

3. Background Worker Pattern

Problema:
Los eventos deben procesarse de forma asíncrona y tolerante a fallos.

Patrón elegido:
Background Worker.

Justificación:
Permite consumir mensajes de la cola interna sin bloquear la API. Además, centraliza el procesamiento y facilita la aplicación de reintentos.

4. Retry Pattern

Problema:
Algunos pasos pueden fallar temporalmente, como almacenamiento de evidencias, notificaciones o comunicación con el OMS.

Patrón elegido:
Retry Pattern.

Justificación:
Permite reintentar operaciones ante errores transitorios antes de considerar que el mensaje falló definitivamente.

5. Dead Letter Queue Pattern

Problema:
Si un mensaje falla después de varios reintentos, no debe perderse.

Patrón elegido:
Dead Letter Queue.

Justificación:
Permite almacenar mensajes fallidos para análisis, reprocesamiento manual o corrección posterior.

6. Idempotency Pattern

Problema:
Un webhook puede llegar más de una vez por reintentos del TMS o problemas de red.

Patrón elegido:
Idempotency.

Justificación:
Evita que el mismo evento sea procesado múltiples veces, lo cual podría causar errores como incrementar varias veces el contador de visitas o duplicar históricos/evidencias.

7. Repository Pattern

Problema:
La lógica de negocio no debe depender directamente de una base de datos, API externa o almacenamiento específico.

Patrón elegido:
Repository Pattern.

Justificación:
Permite abstraer el acceso a pedidos e históricos. En la prueba se usan repositorios en memoria, pero en producción podrían reemplazarse por SQL Server, PostgreSQL o una API real del OMS.

8. Ports and Adapters / Hexagonal Architecture

Problema:
El sistema se integra con varios componentes externos: TMS, OMS, storage, notificaciones y mensajería.

Patrón elegido:
Hexagonal Architecture.

Justificación:
Permite mantener la lógica de negocio independiente de frameworks, bases de datos, colas o servicios cloud. Los detalles externos se implementan como adaptadores intercambiables.

9. Aggregate Pattern

Problema:
El pedido tiene reglas que deben mantenerse consistentes: estado actual, estados finales, contador de visitas y devolución automática.

Patrón elegido:
Aggregate Pattern.

Justificación:
El agregado Order centraliza las reglas de negocio y evita modificaciones inconsistentes desde fuera del dominio.
