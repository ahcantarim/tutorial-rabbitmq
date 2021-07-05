- [RabbitMQ .NET client API reference online](https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.html)
- [RabbitMQ .NET tutorials](https://github.com/rabbitmq/rabbitmq-tutorials/tree/master/dotnet)

# Pré-requisitos

Garanta que o **RabbitMQ** está instalado e sendo executado em `localhost` na porta padrão `5672`.

- **Management:** http://localhost:15672
- **Username:** guest
- **Password:** guest


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

```csharp
var channel = ((EventingBasicConsumer)sender).Model;
channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
```

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


> NOTA SOBRE PERSISTÊNCIA DE MENSAGENS
> 
> Marcar as mensagens como persistentes não garante completamente que uma mensagem não será perdida.
> 
> Apesar disso dizer ao RabbitMQ para salvar a mensagem no disco, existe uma pequena janela de tempo quando o RabbitMQ aceita uma mensagem e ainda não salvou a mesma.
> 
> As garantias de persistência não são fortes, mas são mais do que o necessário para sistemas simples de enfileramento de mensagens.
> 
> Se você precisa de uma garantia melhor, então você pode usar as confirmações de publicação (https://www.rabbitmq.com/confirms.html).


###### Fair Dispatch

Pode-se notar que o envio de mensagens aos `Consumers`, por vezes, pode não ser "justo". Por exemplo, em uma situação com dois *workers*, onde todas as mensagens *pares* tem um processamento pesado e todas as *ímpares* tem um processamento leve, um *worker* estará constantemente ocupado e o outro não fará nenhum trabalho pesado.

Isso acontece porque o **RabbitMQ** apenas envia a mensagem assim que ela entra na fila. Ele não olha para o número de mensagens não confirmadas de um `Consumer`.

Para alterar este comportamento, podemos usar o método `channel.BasicQos()` com um valor de `prefetchCount: 1`. Isso diz ao **RabbitMQ** para não dar mais de uma mensagem para um *worker* ao mesmo tempo. Ou, em outras palavras, não envie uma nova mensagem para um *worker* até que ele tenha processado e confirmado a anterior. Ao invés disso, ele irá enviá-la para o próximo *worker* que não estiver ocupado.

```csharp
channel.BasicQos(0, 1, false);
```


> NOTA SOBRE TAMANHO DA FILA
> 
> Se todos os workers estão ocupados, sua fila pode ficar cheia.
> 
> Você deve ficar de olho nisso, e talvez adicionar mais workers, ou ter alguma outra estratégia.


x