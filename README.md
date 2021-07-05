# Tutorial 1

[Basic Producer and Consumer](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html)

Foram escritos dois programas para enviar e receber mensagens em uma fila nomeada: um `Producer (Send)` que envia uma mensagem simples, e um `Consumer (Receive)` que recebe as mensagens e as exibe.

- `Tutorial.RabbitMQ.Console.Send`: console para adicionar mensagens em uma fila;

- `Tutorial.RabbitMQ.Console.Receive`: console para ler mensagens de uma fila;

# Tutorial 2

[Work Queues (aka: Task Queues)](https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html)

Foi criada uma `fila de trabalho` que é usada para distribuir tarefas que consomem tempo através de múltiplos `Consumers`.

Como não temos uma tarefa do mundo real para executar, como redimensionar imagens ou gerar arquivos PDF, simulamos algo nesse sentido através a função `Thread.Sleep()`. Consideramos o número de `.` na cadeia de caracteres enviada como sua complexidade: cada `.` contará como um segundo de "trabalho". Por exemplo, uma tarefa descrita por `Hello...` demorará 3 segundos para ser finalizada.

- `Tutorial.RabbitMQ.Console.NewTask`: console para adicionar mensagens em uma fila ;

- `Tutorial.RabbitMQ.Console.Worker`: console para ler mensagens de uma fila simulando um processamento para cada mensagem; pode ser executada mais de uma instância e as mensagens serão lidas alternadamente por cada uma;

###### Manual message acknowledgments (ack)

Foi alterado o valor do parâmetro `autoAck: false` no canal que consome a fila, visando realizar manualmente a confirmação/rejeição da mensagem recebida.

No manipulador de eventos de mensagem recebida, foi implementado o código `channel.BasicAck()` para confirmar manualmente o processamento da mensagem, após o término da mesma.

Usando esse código nós podemos ter certeza que mesmo que um `Consumer` seja finalizado no meio do processamento de uma mensagem, nada será perdido. Logo que isso ocorrer, todas as mensagens não confirmadas serão reenviadas para outros `Consumers`.

###### Message durability

Anteriormente (com o *manual message ack*), vimos como garantir que mesmo que o `Consumer` seja finalizado por algum motivo, a tarefa não seja perdida. Mas, da forma atual, as tarefas seriam perdidas se o servidor do **RabbitMQ** parasse.

Para que isso não aconteça, devemos marcar tanto a fila quanto as mensagens como *durable*.

Primeiro, foi alterado o valor do parâmetro `durable: true` nos canais que declaram a fila para envio (`Producer NewTask`) e recebimento (`Consumer Worker`) de mensagens.

Apesar do comando por si só estar correto, não funcionaria na configuração atual. Isso acontece pois uma fila com o nome atual já foi definida (e não como *durable*). O **RabbitMQ** não permite que uma fila existente seja redefinida com parâmetros diferentes e retornará um erro. Como alternativa, apenas declararemos a fila com um nome diferente.

Com essa alteração, temos certeza que a fila não será perdida se o **RabbitMQ** for reiniciado.

Agora, precisamos marcar nossas mensagens como *persistentes* e, para isso, utilizamos o trecho de código abaixo:

```csharp
var properties = channel.CreateBasicProperties();
properties.Persistent = true;
```

Adicionalmente, repassamos tais propriedades para o método `channel.BasicPublish()`.

> ATENÇÃO
> 
> Marcar as mensagens como persistentes não garante completamente que uma mensagem não será perdida.
> 
> Apesar disso dizer ao RabbitMQ para salvar a mensagem no disco, existe uma pequena janela de tempo quando o RabbitMQ aceita uma mensagem e ainda não salvou a mesma.
> 
> As garantias de persistência não são fortes, mas são mais do que o necessário para sistemas simples de enfileramento de mensagens.
> 
> Se você precisa de uma garantia melhor, então você pode usar as confirmações de publicação (https://www.rabbitmq.com/confirms.html).
