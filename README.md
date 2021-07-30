[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]


<br />
<p align="center">
  <a href="https://github.com/ahcantarim/tutorial-rabbitmq">
    <img src=".github/logo.png" alt="tutorial-rabbitmq" width="80" height="80">
  </a>

  <h3 align="center">Tutoriais RabbitMQ</h3>

  <p align="center">
    Projeto de estudo para entender o funcionamento do RabbitMQ e suas aplicações práticas.
    <br />
    <br />
    <a href="https://github.com/ahcantarim/tutorial-rabbitmq/issues">Ver problemas abertos</a>
    ·
    <a href="https://github.com/ahcantarim/tutorial-rabbitmq/issues/new">Reportar um problema</a>
  </p>
</p>


## Sumário

<ol>
    <li>
        <a href="#sobre-este-projeto">Sobre este projeto</a>
        <ul>
            <li><a href="#tecnologias-utilizadas">Tecnologias utilizadas</a></li>
        </ul>
    </li>
    <li>
        <a href="#configurações-do-ambiente-de-desenvolvimento">Configurações do ambiente de desenvolvimento</a>
        <ul>
            <li><a href="#pré-requisitos">Pré-requisitos</a></li>
            <li><a href="#clonando-o-repositório">Clonando o repositório</a></li>
            <li><a href="#">Instalando as dependências</a></li>
        </ul>
    </li>
    <li>
        <a href="#">Introdução</a>
        <ul>
            <li><a href="#">Conectando aplicações no servidor do RabbitMQ</a></li>
            <li><a href="#">Declarando uma fila</a></li>
            <li><a href="#">Enviando mensagens</a></li>
            <li><a href="#">Recebendo mensagens</a></li>
        </ul>
    </li>
    <li>
        <a href="#">Tutorial 1 » "Hello World!"</a>
        <ul>
            <li><a href="#">Executando os projetos</a></li>
        </ul>
    </li>
    <li>
        <a href="#">Tutorial 2 » Work queues</a>
        <ul>
            <li><a href="#">Manual message acknowledgments (ack)</a></li>
            <li><a href="#">Message durability</a></li>
            <li><a href="#">Fair Dispatch</a></li>
            <li><a href="#">Executando os projetos</a></li>
        </ul>
    </li>
    <li>
        <a href="#">Tutorial 3 » Publish/Subscribe</a>
        <ul>
            <li><a href="#">Exchanges</a></li>
            <li><a href="#">Temporary queues</a></li>
            <li><a href="#">Bindings</a></li>
            <li><a href="#">Executando os projetos</a></li>
        </ul>
    </li>
    <li>
        <a href="#">Tutorial 4 » Routing</a>
        <ul>
            <li><a href="#">Direct exchange</a></li>
            <li><a href="#">Multiple bindings</a></li>
            <li><a href="#">Emitting logs</a></li>
            <li><a href="#">Subscribing</a></li>
            <li><a href="#">Executando os projetos</a></li>
        </ul>
    </li>
    <li>
        <a href="#">Tutorial 5 » Topics</a>
        <ul>
            <li><a href="#">#</a></li>
        </ul>
    </li>
    <li>
        <a href="#">Tutorial 6 » RPC</a>
        <ul>
            <li><a href="#">#</a></li>
        </ul>
    </li>
    <li>
        <a href="#">Tutorial 7 » Publisher Confirms</a>
        <ul>
            <li><a href="#">#</a></li>
        </ul>
    </li>
    <li><a href="#licença">Licença</a></li>
    <li><a href="#contato">Contato</a></li>
    <li><a href="#referências">Referências</a></li>
</ol>


## Sobre este projeto

Este repositório foi elaborado como projeto de estudo para entender o funcionamento do **RabbitMQ** e suas aplicações práticas.

Toda a documentação aqui transcrita tem como base a documentação oficial, que pode ser encontrada no site da ferramenta.

### Tecnologias utilizadas

* [Docker](https://www.docker.com)
* [RabbitMQ](https://www.rabbitmq.com)


## Configurações do ambiente de desenvolvimento

Para obter uma cópia local atualizada e que possa ser executada corretamente, siga os passos abaixo.

### Pré-requisitos

Os tutoriais assumem que o **RabbitMQ** está instalado e sendo executado em `localhost` na porta padrão (`5672`).

Para acessar o ambiente de gerenciamento, utilize as informações abaixo:

- **Management:** http://localhost:15672
- **Username:** guest
- **Password:** guest

### Clonando o repositório

```bash
git clone https://github.com/ahcantarim/tutorial-rabbitmq.git
```

### Instalando as dependências

No diretório do projeto, executar o(s) comando(s):

```bash
dotnet restore
```


## Introdução

**RabbitMQ** é um *message broker*: ele aceita e encaminha mensagens. Você pode pensar sobre isso como se fossem os *Correios*: quando você coloca a carta que você quer em uma caixa de postagem, você pode ter certeza de que eventualmente o carteiro irá entregar sua carta ao destinatário. Nesta analogia, o **RabbitMQ** é a caixa de postagem, a agência dos *Correios* e o carteiro.

A maior diferença entre o **RabbitMQ** e uma agência dos *Correios* é que ele não lida com papel, ao invés disso aceita, armazena e encaminha blobs binários de dados ‒ *mensagens*.

O **RabbitMQ** ‒ e outras ferramentas de mensagens no geral ‒ usa alguns jargões:

- *Producing* significa nada mais do que *enviando*. Um programa que envia mensagens é um `Producer` (*produtor*):

![Producer](.github/producer.png)

- Uma `Queue` (*fila*) é o nome para uma caixa postal que vive dentro do **RabbitMQ**. Apesar das mensagens fluirem entre o **RabbitMQ** e suas aplicações, elas apenas podem ser armazenadas dentro de uma *fila*. Uma *fila* é apenas limitada pela memória e espaço em disco do servidor, e é essencialmente um grande *buffer de mensagens*. Vários `Producers` podem enviar mensagens que vão para uma fila, e vários `Consumers` podem tentar receber dados de uma fila. É assim que representamos uma fila:

![Queue](.github/queue.png)

- *Consuming* tem um significado parecido com *producing*. Um `Consumer` (*consumidor*) é um programa que majoritariamente espera para receber mensagens:

![Queue](.github/consumer.png)

Note que o `Producer`, `Consumer` e o *broker* não precisam residir no mesmo servidor. De fato na maioria das aplicações isso não acontece. Uma aplicação pode ser ao mesmo tempo um `Producer` e um `Consumer`, também.

### Conectando aplicações no servidor do RabbitMQ

Para criar uma conexão com o servidor, teremos sempre algo parecido com isso:

```csharp
class Send
{
    public static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            ...
        }
    }
}
```

A conexão abstrai a conexão *socket* e se encarrega de negociar a versão do protocolo e a autenticação para nós. Aqui, estamos conectando a um nó local do **RabbitMQ** ‒ por isso o *localhost*. Se nós quisermos nos conectar a um nó em um servidor diferente, simplesmente especificamos o *HostName* ou endereço IP aqui:

```csharp
var factory = new ConnectionFactory() { HostName = "xxx.xxx.xxx.xxx", Port = 5672, UserName = "xxx", Password = "xxx" };
```

### Declarando uma fila

Declarar uma fila é uma ação idempotente ‒ ela apenas será criada se ainda não existir. Por conta disso, é comum declararmos a fila tanto no `Producer` quanto no `Consumer`, pois queremos garantir que a fila exista antes de utilizá-la.

```csharp
channel.QueueDeclare(queue: "hello",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
```

### Enviando mensagens

O conteúdo de mensagem é um *array de bytes*, então você pode codificar qualquer coisa que quiser.

```csharp
string message = "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

channel.BasicPublish(exchange: "",
                     routingKey: "hello",
                     basicProperties: null,
                     body: body);

Console.WriteLine($" [x] Sent {message}");
```

### Recebendo mensagens

Considerando que sempre iremos obter as mensagens de forma assíncrona de uma fila do servidor, utilizaremos um *callback*. O manipulador de evento `EventingBasicConsumer.Received` nos permite isso.

```csharp
var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] Received {message}");
};

channel.BasicConsume(queue: "hello",
                     autoAck: true,
                     consumer: consumer);
```


## Tutorial 1 » "Hello World!"

[Basic Producer and Consumer](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html)

Neste tutorial, foram escritos dois programas: um produtor que envia uma mensagem única, e um consumidor que recebe mensagens e exibe-as em tela.

No diagrama abaixo, `P` é nosso produtor e `C` é nosso consumidor. A caixa no meio é uma fila.

![Queue](.github/tutorial-1-01.png)

### Sending

![Queue](.github/tutorial-1-02.png)

### Receiving

![Queue](.github/tutorial-1-03.png)

### Executando os projetos

- `Tutorial.RabbitMQ.Console.Send`: Produtor que conecta no **RabbitMQ**, envia uma mensagem única, e é finalizado.

- `Tutorial.RabbitMQ.Console.Receive`: Consumidor que fica escutando as mensagens do **RabbitMQ**. Diferente do produtor que envia uma única mensagem e é finalizado, ele será executado continuamente para escutar novas mensagens e exibi-las.

Você pode executar os projetos pelo `Visual Studio`, pelos executáveis gerados no diretório `bin`, ou através da linha de comando. Para o último caso, abra dois terminais.

Execute primeiro o `Consumer`:

```bash
cd Tutorial.RabbitMQ.Console.Receive
dotnet run
```

Depois execute o `Producer`:

```bash
cd Tutorial.RabbitMQ.Console.Send
dotnet run
```

O `Consumer` irá exibir as mensagens que obter do `Producer` via **RabbitMQ**. O `Consumer` continuará sendo executado, aguardando por mensagens, então você pode tentar executar um novo `Producer` a partir de outro terminal.

No próximo tutorial iremos criar uma simples fila de trabalho.


## Tutorial 2 » Work queues

[Work Queues (aka: Task Queues)](https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html)

Foi criada uma `fila de trabalho` que é usada para distribuir tarefas que consomem tempo através de múltiplos `Consumers`.

Como não temos uma tarefa do mundo real para executar, como redimensionar imagens ou gerar arquivos PDF, simulamos algo nesse sentido através a função `Thread.Sleep()`. Consideramos o número de `.` na cadeia de caracteres enviada como sua complexidade: cada `.` contará como um segundo de "trabalho". Por exemplo, uma tarefa descrita por `Hello...` demorará 3 segundos para ser finalizada.

![Queue](.github/tutorial-2-01.png)

### Manual message acknowledgments (ack)

Foi alterado o valor do parâmetro `autoAck: false` no canal que consome a fila, visando realizar manualmente a confirmação/rejeição da mensagem recebida.

No manipulador de eventos de mensagem recebida, foi implementado o código `channel.BasicAck()` para confirmar manualmente o processamento da mensagem, após o término da mesma.

```csharp
var channel = ((EventingBasicConsumer)sender).Model;
channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
```

Usando esse código nós podemos ter certeza que mesmo que um `Consumer` seja finalizado no meio do processamento de uma mensagem, nada será perdido. Logo que isso ocorrer, todas as mensagens não confirmadas serão reenviadas para outros `Consumers`.

### Message durability

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

### Fair Dispatch

Pode-se notar que o envio de mensagens aos `Consumers`, por vezes, pode não ser "justo". Por exemplo, em uma situação com dois *workers*, onde todas as mensagens *pares* tem um processamento pesado e todas as *ímpares* tem um processamento leve, um *worker* estará constantemente ocupado e o outro não fará nenhum trabalho pesado.

Isso acontece porque o **RabbitMQ** apenas envia a mensagem assim que ela entra na fila. Ele não olha para o número de mensagens não confirmadas de um `Consumer`.

![Queue](.github/tutorial-2-02.png)

Para alterar este comportamento, podemos usar o método `channel.BasicQos()` com um valor de `prefetchCount: 1`. Isso diz ao **RabbitMQ** para não dar mais de uma mensagem para um *worker* ao mesmo tempo. Ou, em outras palavras, não envie uma nova mensagem para um *worker* até que ele tenha processado e confirmado a anterior. Ao invés disso, ele irá enviá-la para o próximo *worker* que não estiver ocupado.

```csharp
channel.BasicQos(0, 1, false);
```


> NOTA SOBRE TAMANHO DA FILA
> 
> Se todos os workers estão ocupados, sua fila pode ficar cheia.
> 
> Você deve ficar de olho nisso, e talvez adicionar mais workers, ou ter alguma outra estratégia.

### Executando os projetos

- `Tutorial.RabbitMQ.Console.NewTask`: console para adicionar mensagens em uma fila ;

- `Tutorial.RabbitMQ.Console.Worker`: console para ler mensagens de uma fila simulando um processamento para cada mensagem; pode ser executada mais de uma instância e as mensagens serão lidas alternadamente por cada uma;

Você pode executar os projetos pelo `Visual Studio`, pelos executáveis gerados no diretório `bin`, ou através da linha de comando. Para o último caso, abra dois terminais.

Execute primeiro o `Consumer`:

```bash
cd Tutorial.RabbitMQ.Console.Worker
dotnet run
```

Depois execute o `Producer`:

```bash
cd Tutorial.RabbitMQ.Console.NewTask
dotnet run
```

Você também pode executar cada projeto mais de uma vez (usando mais de um terminal), para verificar como é feita a distribuição de mensagens entre eles. As opções de durabilidade permitem que a mensagem sobreviva mesmo que o **RabbitMQ** seja reiniciado (ou mesmo que um `Consumer` seja finalizado no meio do processamento de uma tarefa - neste caso, a tarefa será entregue a outro `Consumer` assim que possível). Adicionalmente, na execução do `Producer`, você pode informar um argumento com `.` para simular uma carga de trabalho maior:

```bash
cd Tutorial.RabbitMQ.Console.NewTask
dotnet run "Task que demora 5 segundos....."
dotnet run "Task que demora 3 segundos..."
dotnet run "Task que demora 20 segundos...................."
```


No próximo tutorial iremos aprender como enviar a mesma mensagem para vários `Consumers`.


## Tutorial 3 » Publish/Subscribe

[Publish/Subscribe](https://www.rabbitmq.com/tutorials/tutorial-three-dotnet.html)

No tutorial anterior foi criada uma fila de trabalho. Assume-se através de uma fila de trabalho que cada tarefa é entregue a exatamente um *worker*.

Agora será feito algo completamente diferente -- iremos entregar uma mesma mensagem a múltiplos `Consumers`.

Para ilustrar este padrão, foi criado um sistema de *log* simples. Consiste em dois programas -- o primeiro envia as mensagens de log e o segundo recebe e exibe as mesmas.

Nesse sistema de *log*, cada cópia do `Consumer` que estiver sendo executada irá receber as mensagens. Assim, pode-se executar um receptor e direcionar os logs para o disco rígido (arquivo); e ao mesmo tempo pode-se executar outro receptor e visualizar os logs em tela.

Essencialmente, as mensagens publicadas serão transmitidas para todos os receptores.

### Exchanges

Até aqui, enviamos e recebemos mensagens de e para uma fila. Agora introduziremos o conceito do modelo completo de mensageria com **RabbitMQ**.

A ideia principal do modelo de mensagens no **RabbitMQ** é que um `Producer` nunca envia nenhuma mensagem diretamente para uma fila. Na verdade, geralmente um `Producer` sequer sabe se uma mensagem será enviada para alguma fila.

Ao invés disso, o `Producer` pode apenas enviar mensagens para uma *exchange*.

![Queue](.github/tutorial-3-01.png)

Nos tutoriais anteriores não sabíamos nada sobre *exchanges*, mas ainda assim fomos capazes de enviar mensagens para filas. Isso foi possível pois estávamos usando a *exchange default*, a qual é identificada pela cadeia de caracteres vazia (`""`).

Quando a *exchange* informada for uma cadeia de caracteres vazia (*default* ou *nameless*), as mensagens são encaminhadas para a fila com o nome especificado no parâmetro `routingKey`, se ela existir.

### Temporary queues

Anteriormente usamos filas que tinham nomes específicos. Nomear uma fila foi crucial naquele momento -- nós precisávamos apontar os *workers* para a mesma fila. Dar nome à filas é importante quando você quer compartilhá-la entre `Producers` e `Consumers`.

Mas esse não é o caso da aplicação de *log*. Aqui, nós queremos escutar todas as mensagens, não apenas um grupo delas. Também estamos interessados apenas no fluxo atual de mensagens e não nas antigas. Por isso, precisamos de duas coisas:

Em primeiro lugar, sempre que nos conectarmos ao **RabbitMQ** precisamos de uma fila nova e vazia. Para fazer isso nós podemos criar uma fila com um nome aleatório ou, ainda melhor, deixar o servidor escolher um nome aleatório para nós.

Em segundo lugar, assim que desconectarmos o `Consumer`, a fila deve ser automaticamente deletada.

Quando nós não informamos parâmetros para o método `QueueDeclare()`, criamos uma fila nomeada e não durável, exclusiva e auto deletável.

```csharp
var queueName = channel.QueueDeclare().QueueName;
```

Neste ponto, a propriedade `QueueName` contém um nome aleatório para a fila. Por exemplo, pode ser algo como `amq.gen-JzTY20BRgKO-HjmUJj0wLg`.

### Bindings

![Queue](.github/tutorial-3-02.png)

Nós já criamos a *exchange* que espalha as mensagens e uma fila. Agora nós precisamos dizer para a *exchange* para enviar mensagens para nossa fila. Essa relação entre uma *exchange* e uma fila é chamanda de *binding*.

O *binding* é um relacionamento entre uma *exchange* e uma fila. Pode ser entendido da seguinte forma: a fila está interessada nas mensagens desta *exchange*.

```csharp
channel.QueueBind(queue: queueName,
                  exchange: "logs",
                  routingKey: "");
```

A partir de agora, a *exchange* `logs` irá acrescentar mensagens em nossa fila.

![Queue](.github/tutorial-3-03.png)

### Executando os projetos

- `Tutorial.RabbitMQ.Console.EmitLog`: console para transmitir mensagens a uma *Exchange*;

- `Tutorial.RabbitMQ.Console.ReceiveLogs`: console para receber mensagens de uma *Exchange*;

Você pode executar os projetos pelo `Visual Studio`, pelos executáveis gerados no diretório `bin`, ou através da linha de comando. Para o último caso, abra dois terminais.

Execute primeiro o `Consumer`. Se você quer salvar os *logs* em um arquivo, utilize o comando abaixo:

```bash
cd Tutorial.RabbitMQ.Console.ReceiveLogs
dotnet run > logs_from_rabbit.log
```

Se você quer ver os logs na tela, através de um novo terminal utilize o comando abaixo:

```bash
cd Tutorial.RabbitMQ.Console.ReceiveLogs
dotnet run
```

E para gerar os *logs* utilize o comando:

```bash
cd Tutorial.RabbitMQ.Console.EmitLog
dotnet run
```

No próximo tutorial iremos aprender como escutar um subconjunto de mensagens.


## Tutorial 4 » Routing

[Routing](https://www.rabbitmq.com/tutorials/tutorial-four-dotnet.html)

No tutorial anterior, criamos um sistema de *log* simples. Fomos capazes de transmitir mensagens para vários receptores.

Neste tutorial vamos adicionar uma funcionalidade à ele - vamos tornar possível se subscrever apenas a um subconjunto de mensagens. Por exemplo, teremos a possibilidade de direcionar apenas as mensagens de *erro crítico* para o arquivo em disco, enquanto ainda é possível exibir todas as mensagens de *log* em tela.

*Bindings* podem ter um parâmetro extra chamado `routingKey`. Para evitar a confusão com o parâmetro do método `BasicPublish`, iremos chamá-lo aqui de `binding key`. Essa é a forma que nós podemos criar um *binding* com uma *key*:

```csharp
channel.QueueBind(queue: queueName,
                  exchange: "direct_logs",
                  routingKey: "black");
```

O significado de um *binding key* dependo do tipo de *exchange*. As *exchanges* do tipo `Fanout`, a qual usamos previamente, simplesmente ignoram este valor.

### Direct exchange

Nosso sistema de *log* do tutorial anterior transmite todas as mensagens para todos os `Consumers`. Nós queremos extender isso para permitir filtrar mensagens baseadas em sua severidade. Por exemplo, nós podemos querer que o programa que está escrevendo mensagens de *log* no disco apenas receba mensagens de erro crítico, e não desperdice espaço em disco com mensagens de informação ou alertas.

Nós estávamos usando a exchange do tipo `Fanout`, a qual não nos dá muita flexibilidade - ela é apenas capaz de fazer uma transmissão "pouco esperta".

Ao invés disso, iremos usar uma *exchange* `Direct`. O algoritmo de roteamento por trás de uma *exchange* desse tipo é simples - uma mensagem vai para a fila onde o `binding key` corresponde exatamente ao `routing key` da mensagem.

Para ilustrar isso, considere a seguinte configuração:

![Queue](.github/tutorial-4-01.png)

Nessa configuração, podemos ver a *exchange* `Direct` chamada `X` com duas filas vinculadas à ela. A primeira fila está vinculada com um *binding key* `orange`, e a segunda tem dois *bindings*: um com o *binding key* `black` e outra com `green`.

Dessa forma, uma mensagem publicada para a *exchange* com uma *routing key* `orange` será roteada para a fila `Q1`. Mensagens com *routing key* `black` ou `green` irão para a fila `Q2`. Todas as outras mensagens serão descartadas.

### Multiple bindings

![Queue](.github/tutorial-4-02.png)

É perfeitamente legal se vincular à múltiplas filas com o mesmo *binding key*. Em nosso exemplo, nós poderíamos criar vínculos entre `X` e `Q1` com o *binding key* `black`. Neste caso, a *exchange* `Direct` se comportará como uma `Fanout` e irá transmitir a mensagem para todas as filas correspondentes. Uma mensagem com o *routing key* `black` será entregue tanto para `Q1` quanto para `Q2`.

### Emitting logs

Iremos usar este modelo para nosso sistema de *log*. Ao invés de uma `Fanout` iremos enviar mensagens para uma *exchange* `Direct`. Iremos fornecer a severidade do *log* como `routing key`. Desta forma, o `Consumer` poderá selecionar a severidade que deseja receber.

Como sempre, precisamos primeiro criar uma *exchange*:

```csharp
channel.ExchangeDeclare(exchange: "direct_logs", type: "direct");
```

E agora estamos prontos para enviar uma mensagem:

```csharp
var body = Encoding.UTF8.GetBytes(message);

channel.BasicPublish(exchange: "direct_logs",
                     routingKey: severity,
                     basicProperties: null,
                     body: body);
```

Para simplificar as coisas, iremos assumir que a `severity` poderá ser uma das seguintes: `info`, `warning` ou `error`.

### Subscribing

Receber mensagens irá funcionar como no tutorial anterior, com uma diferença - iremos criar um novo vínculo para cada severidade na qual estamos interessados.

```csharp
var queueName = channel.QueueDeclare().QueueName;

foreach(var severity in args)
{
    channel.QueueBind(queue: queueName,
                      exchange: "direct_logs",
                      routingKey: severity);
}
```

### Executando os projetos

![Queue](.github/tutorial-4-03.png)

- `Tutorial.RabbitMQ.Console.EmitLogDirect`: console para transmitir mensagens a uma *Exchange* com *routing key* especificado em forma de severidade do *log*;

- `Tutorial.RabbitMQ.Console.ReceiveLogsDirect`: console para receber mensagens de uma *Exchange*, vinculado à uma ou mais *routing key*;

Você pode executar os projetos pelo `Visual Studio`, pelos executáveis gerados no diretório `bin`, ou através da linha de comando. Para o último caso, abra dois terminais.

Se você quer salvar em arquivo apenas as mensagens de *log* de `warning` e `error`, utilize o comando abaixo:

```bash
cd Tutorial.RabbitMQ.Console.ReceiveLogsDirect
dotnet run warning error > logs_from_rabbit.log
```

Se você quer ver os logs na tela, através de um novo terminal utilize o comando abaixo:

```bash
cd Tutorial.RabbitMQ.Console.ReceiveLogsDirect
dotnet run info warning error
```

E, por exemplo, para gerar os *logs* de `error` utilize o comando:

```bash
cd Tutorial.RabbitMQ.Console.EmitLogDirect
dotnet run error "Run. Run. Or it will explode."
```

No próximo tutorial veremos como escutar por mensagens baseadas em um modelo.


## Tutorial 5 » Topics

No tutorial anterior nós melhoramos nosso sistema de *logs*. Ao invés de utilizar uma *exchange* `Fanout` capaz de apenas espalhar as mensagens, nós utilizamos uma `Direct`e ganhamos a possibilidade de receber os *logs* de forma seletiva.

Apesar disso, ainda temos limitações - nossa *exchange* não pode rotear as mensagens baseadas em critérios múltiplos.

Em nosso sistema de *log*, nós podemos querer subscrever não apenas baseado na severidade da mensagem, mas também baseado na fonte que emitiu o *log*. Você pode conhecer esse conceito da ferramenta *unix syslog*, a qual roteia os *logs* baseado tanto na severidade (`info`, `warn`, `crit`, ...) e facilidade (`auth`, `cron`, `kern`, ...).

Isso poderia nos dar muito mais flexibilidade - nós podemos querer escutar apenas os erros críticos vindo do `cron`, mas também todos os *logs* vindos do `kern`.

Para implementar isso em nosso sistema de *logs*, nós precisamos aprender sobre uma *exchange* um pouco mais complexa: `Topic`.

### Topic exchange

Mensagens enviadas a uma *exchange* `Topic` não podem ter um `routing key` arbitrário - ele *deve ser* uma lista de palavras, separadas por ponto. As palavras podem ser qualquer coisa, mas comumente elas especificam algumas funcionalidades conectadas à mensagem. Alguns exemplos de *routing key* válidos: `stock.usd.nyse`, `nyse.vmw`, `quick.orange.rabbit`. Também podem existir quantas palavras você quiser no *routing key*, desde que ele seja limitado a 255 bytes.

O *binding key* precisa também estar no mesmo formato. A lógica por trás de uma *exchange* `Topic` é parecida com uma `Direct` - uma mensagem enviada com um *routing key* específico será entregue para todas as filas que estão vinculadas com um *binding key* correspondente. Entretanto há dois casos especiais para os *binding keys*:

- `*` (asteriscos) podem ser substitudos para exatamente uma palavra.
- `#` (sustenidos) podem ser substitutos para nenhuma ou mais palavras.

É mais fácil explicar isso em um exemplo:

![Queue](.github/tutorial-5-01.png)

Neste exemplo, iremos enviar mensagens onde todas elas descrevem animais. As mensagens serão enviadas com um *routing key* que consiste de três palavras (e dois pontos para separação). A primeira palavra no *routing key* irá descrever a Velocidade, a segunda a Cor e a terceira a Espécie: `{velocidade}.{cor}.{espécie}`.

Criamos três vínculos: `Q1` está vinculada com o *binding key* `*.laranja.*` e `Q2` com `*.*.coelho` e `lento.#`.

Esses vínculos podem ser resumidos como:

- `Q1` está interessada em todos os animais laranjas.
- `Q2` quer escutar tudo sobre coelhos, e tudo sobre animais lentos.

Uma mensagem com um *routing key* `rápido.laranja.coelho` será entregue para as duas filas. A mensagem `lento.laranja.elefante` também irá para ambas. Por outro lado, `rápido.laranja.raposa` irá apenas para a primeira fila, e `lento.marrom.raposa` irá apenas para a segunda fila. `lento.rosa.coelho` será entregue para a  segunda fila apenas uma vez, mesmo que corresponda a dois vínculos. `rápido.marrom.raposa` não corresponde a nenhum vínculo então será descartada.

O que acontece se nós quebrarmos o contrato e enviar uma mensagem com uma ou quatro palavras, como `laranja` ou `rápido.laranja.macho.coelho`? Bem, essas mensagens não correspondem a nenhum vínculo e serão perdidas.

Por outro lado, `lento.laranja.macho.coelho`, mesmo que tenha quatro palavras, irá corresponder ao último vínculo e será entregue à segunda fila.

> NOTA SOBRE TOPIC EXCHANGE
> 
> Esse tipo de *exchange* é poderosa e pode se comportar como outras *exchanges*.
> 
> Quando uma fila é vinculada com `#` (sustenido) no *binding key* ela irá receber todas as mensagens, independentemente do *routing key* - como uma *exchange* `Fanout`.
> 
> Quando os caracteres especiais `*` (asterisco) e `#` (sustenido) não são usados nos vínculos, a *exchange* `Topic` irá se comportar da mesma forma que uma `Direct`.

### Executando os projetos

- `Tutorial.RabbitMQ.Console.EmitLogTopic`: console para transmitir mensagens a uma *Exchange* com *routing key* assumidos como tendo duas palavras: `facilidade.severidade`;

- `Tutorial.RabbitMQ.Console.ReceiveLogsTopic`: console para receber mensagens de uma *Exchange*, especificando o vínculo para o *routing key*;

Você pode executar os projetos pelo `Visual Studio`, pelos executáveis gerados no diretório `bin`, ou através da linha de comando. Para o último caso, abra dois terminais.

Para receber todos os *logs*, utilize o comando abaixo:

```bash
cd Tutorial.RabbitMQ.Console.ReceiveLogsTopic
dotnet run "#"
```

Para receber todos os *logs* da facilidade `kern`:

```bash
cd Tutorial.RabbitMQ.Console.ReceiveLogsTopic
dotnet run "kern.*"
```

Ou se você quiser ouvir apenas sobre `critical` *logs*:

```bash
cd Tutorial.RabbitMQ.Console.ReceiveLogsTopic
dotnet run "*.critical"
```

Você também pode criar múltiplos vínculos:

```bash
cd Tutorial.RabbitMQ.Console.ReceiveLogsTopic
dotnet run "kern.*" "*.critical"
```

E para gerar um *log* com o *routing key* `kern.critical`, por exemplo, utilize o comando:

```bash
cd Tutorial.RabbitMQ.Console.EmitLogTopic
dotnet run "kern.critical" "A critical kernel error."
```

Divirta-se brincando com esses programas. Note que o código não assume nada sobre os *routing* ou *binding keys*, você pode querer fazer algo com mais de dois parâmetros no *routing key*.

No próximo tutorial, vamos descobrir como fazer uma mensagem de ida e volta, como uma chamada de procedimento remoto (RPC).


## Tutorial 6 » RPC

- [ ] `TODO: Documentar`


## Tutorial 7 » Publisher Confirms

- [ ] `TODO: Documentar`


## Estudo adicional

### Testes de carga básica

- [ ] `TODO: Documentar`

### Limite da fila

- [ ] `TODO: Documentar`


## Licença

Distribuído através da licença MIT. Veja `LICENSE` para mais informações.


## Contato

André Cantarim

[![LinkedIn][linkedin-shield]][linkedin-url]


## Referências

* [RabbitMQ .NET client API reference online](https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.html)
* [RabbitMQ .NET tutorials](https://github.com/rabbitmq/rabbitmq-tutorials/tree/master/dotnet)


<a href="#sumário">🔝 Voltar ao topo</a>


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/ahcantarim/tutorial-rabbitmq.svg?style=for-the-badge
[contributors-url]: https://github.com/ahcantarim/tutorial-rabbitmq/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/ahcantarim/tutorial-rabbitmq.svg?style=for-the-badge
[forks-url]: https://github.com/ahcantarim/tutorial-rabbitmq/network/members
[stars-shield]: https://img.shields.io/github/stars/ahcantarim/tutorial-rabbitmq.svg?style=for-the-badge
[stars-url]: https://github.com/ahcantarim/tutorial-rabbitmq/stargazers
[issues-shield]: https://img.shields.io/github/issues/ahcantarim/tutorial-rabbitmq.svg?style=for-the-badge
[issues-url]: https://github.com/ahcantarim/tutorial-rabbitmq/issues
[license-shield]: https://img.shields.io/github/license/ahcantarim/tutorial-rabbitmq.svg?style=for-the-badge
[license-url]: https://github.com/ahcantarim/tutorial-rabbitmq/blob/master/LICENSE
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/ahcantarim
[product-screenshot]: .github/screenshot.png