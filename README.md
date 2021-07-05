# Tutorial 1

[Basic Producer and Consumer](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html)

Foram escritos dois programas para enviar e receber mensagens em uma fila nomeada: um `Producer` que envia uma mensagem simples, e um `Consumer` que recebe as mensagens e as exibe.

- `Tutorial.RabbitMQ.Console.Send`: console para adicionar mensagens em uma fila;

- `Tutorial.RabbitMQ.Console.Receive`: console para ler mensagens de uma fila;

# Tutorial 2

[Work Queues (aka: Task Queues)](https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html)

Foi criada uma `fila de trabalho` que é usada para distribuir tarefas que consomem tempo através de múltiplos `Consumers`.

Como não temos uma tarefa do mundo real para executar, como redimensionar imagens ou gerar arquivos PDF, simulamos algo nesse sentido através a função `Thread.Sleep()`. Consideramos o número de `.` na cadeia de caracteres enviada como sua complexidade: cada `.` contará como um segundo de "trabalho". Por exemplo, uma tarefa descrita por `Hello...` demorará 3 segundos para ser finalizada.

- `Tutorial.RabbitMQ.Console.NewTask`: console para adicionar mensagens em uma fila ;

- `Tutorial.RabbitMQ.Console.Worker`: console para ler mensagens de uma fila simulando um processamento para cada mensagem; pode ser executada mais de uma instância e as mensagens serão lidas alternadamente por cada uma;

Também foi alterado o valor do parâmetro `autoAck: false` no canal que consome a fila, visando realizar manualmente a confirmação/rejeição da mensagem recebida.

No manipulador de eventos de mensagem recebida, foi implementado o código para confirmar manualmente o processamento da mensagem (`channel.BasicAck()`), após o término da mesma.

Usando esse código nós podemos ter certeza que mesmo que um `Consumer` seja finalizado no meio do processamento de uma mensagem, nada será perdido. Logo que isso ocorrer, todas as mensagens não confirmadas serão reenviadas para outros `Consumers`.

