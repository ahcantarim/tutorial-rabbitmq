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


# Tutorial 3

[Publish/Subscribe](https://www.rabbitmq.com/tutorials/tutorial-three-dotnet.html)

No tutorial anterior foi criada uma fila de trabalho. Assume-se através de uma fila de trabalho que cada tarefa é entregue a exatamente um *worker*.

Agora será feito algo completamente diferente -- iremos entregar uma mesma mensagem a múltiplos `Consumers`.

Para ilustrar este padrão, foi criado um sistema de *log* simples. Consiste em dois programas -- o primeiro envia as mensagens de log e o segundo recebe e exibe as mesmas.

- `Tutorial.RabbitMQ.Console.EmitLog`: console para transmitir mensagens a uma *Exchange*;

- `Tutorial.RabbitMQ.Console.ReceiveLogs`: console para receber mensagens de uma *Exchange*;

Nesse sistema de *log*, cada cópia do `Consumer` que estiver sendo executada irá receber as mensagens. Assim, pode-se executar um receptor e direcionar os logs para o disco rígido (arquivo); e ao mesmo tempo pode-se executar outro receptor e visualizar os logs em tela.

Essencialmente, as mensagens publicadas serão transmitidas para todos os receptores.

###### Exchanges

Até aqui, enviamos e recebemos mensagens de e para uma fila. Agora introduziremos o conceito do modelo completo de mensageria com **RabbitMQ**.

A ideia principal do modelo de mensagens no **RabbitMQ** é que um `Producer` nunca envia nenhuma mensagem diretamente para uma fila. Na verdade, geralmente um `Producer` sequer sabe se uma mensagem será enviada para alguma fila.

Ao invés disso, o `Producer` pode apenas enviar mensagens para uma *exchange*.

Nos tutoriais anteriores não sabíamos nada sobre *exchanges*, mas ainda assim fomos capazes de enviar mensagens para filas. Isso foi possível pois estávamos usando a *exchange default*, a qual é identificada pela cadeia de caracteres vazia (`""`).

Quando a *exchange* informada for uma cadeia de caracteres vazia (*default* ou *nameless*), as mensagens são encaminhadas para a fila com o nome especificado no parâmetro `routingKey`, se ela existir.

###### Temporary queues

Anteriormente usamos filas que tinham nomes específicos. Nomear uma fila foi crucial naquele momento -- nós precisávamos apontar os *workers* para a mesma fila. Dar nome à filas é importante quando você quer compartilhá-la entre `Producers` e `Consumers`.

Mas esse não é o caso da aplicação de *log*. Aqui, nós queremos escutar todas as mensagens, não apenas um grupo delas. Também estamos interessados apenas no fluxo atual de mensagens e não nas antigas. Por isso, precisamos de duas coisas:

Em primeiro lugar, sempre que nos conectarmos ao **RabbitMQ** precisamos de uma fila nova e vazia. Para fazer isso nós podemos criar uma fila com um nome aleatório ou, ainda melhor, deixar o servidor escolher um nome aleatório para nós.

Em segundo lugar, assim que desconectarmos o `Consumer`, a fila deve ser automaticamente deletada.

Quando nós não informamos parâmetros para o método `QueueDeclare()`, criamos uma fila nomeada e não durável, exclusiva e auto deletável.

```csharp
var queueName = channel.QueueDeclare().QueueName;
```

Neste ponto, a propriedade `QueueName` contém um nome aleatório para a fila. Por exemplo, pode ser algo como `amq.gen-JzTY20BRgKO-HjmUJj0wLg`.

###### Bindings

Nós já criamos a *exchange* que espalha as mensagens e uma fila. Agora nós precisamos dizer para a *exchange* para enviar mensagens para nossa fila. Essa relação entre uma *exchange* e uma fila é chamanda de *binding*.

```csharp
channel.QueueBind(queue: queueName,
                  exchange: "logs",
                  routingKey: "");
```

A partir de agora, a *exchange* `logs` irá acrescentar mensagens em nossa fila.